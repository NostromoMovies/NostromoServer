﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Server.Utilities.FileSystemWatcher;
using System.Collections.Concurrent;
using Quartz;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;
using System.IO;

namespace Nostromo.Server.Services;

public class WatcherSettings
{
    public int ProcessingIntervalSeconds { get; set; } = 5;
}

public class FileWatcherService : IHostedService, IDisposable
{
    private readonly List<RecoveringFileSystemWatcher> _watchers;
    private readonly ILogger<FileWatcherService> _logger;
    private readonly WatcherSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentQueue<string> _changedFiles;
    private readonly ISchedulerFactory _schedulerFactory;
    private IScheduler? _scheduler;
    private Timer? _timer;
    private bool _isDisposed;
    private readonly CancellationTokenSource _serviceCts = new();

    public FileWatcherService(
        IEnumerable<RecoveringFileSystemWatcher> watchers,
        ILogger<FileWatcherService> logger,
        IOptions<WatcherSettings> settings,
        ISchedulerFactory schedulerFactory,
        IServiceProvider serviceProvider)
    {
        _watchers = watchers?.ToList() ?? throw new ArgumentNullException(nameof(watchers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings.Value;
        _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _changedFiles = new ConcurrentQueue<string>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("File watcher service is starting...");

        // Initialize and start the Quartz scheduler
        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        // Set the job factory
        _scheduler.JobFactory = _serviceProvider.GetRequiredService<IJobFactory>();

        await _scheduler.Start(cancellationToken);

        foreach (var watcher in _watchers)
        {
            _logger.LogInformation("Starting watcher for directory: {Path}", watcher.Path);
            watcher.FileAdded += OnFileChanged;
            watcher.FileDeleted += OnFileDeleted;
            watcher.Start();
        }

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(_settings.ProcessingIntervalSeconds));
    }

    private void OnFileChanged(object? sender, string filePath)
    {
        _logger.LogInformation("File change detected: {FilePath}", filePath);
        _changedFiles.Enqueue(filePath);
    }

    private void OnFileDeleted(object? sender, string filePath)
    {
        _logger.LogInformation("File deleted: {FilePath}", filePath);

        // Try to cancel any scheduled job for this file
        if (_scheduler != null)
        {
            var jobKey = new JobKey($"ProcessVideoJob_{filePath}", "ProcessVideoGroup");
            _scheduler.DeleteJob(jobKey, _serviceCts.Token).ConfigureAwait(false);
        }
    }

    private async void DoWork(object? state)
    {
        if (_scheduler == null) return;

        try
        {
            while (_changedFiles.TryDequeue(out string? filePath))
            {
                _logger.LogInformation("Processing file from queue: {FilePath}", filePath);
                try
                {
                    await ProcessFileAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file: {FilePath}", filePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in file processing loop");
        }
    }

    private async Task ProcessFileAsync(string filePath)
    {
        if (_scheduler == null) return;

        _logger.LogInformation("Preparing to process file: {FilePath}", filePath);

        // Wait for file to be ready with timeout and retries
        var readyCheckStart = DateTime.UtcNow;
        var retryCount = 0;
        const int maxRetries = 5;

        while (true)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File no longer exists: {FilePath}", filePath);
                    return;
                }

                if (!IsFileReady(filePath))
                {
                    if (DateTime.UtcNow - readyCheckStart > TimeSpan.FromMinutes(5))
                    {
                        _logger.LogWarning("Timed out waiting for file to be ready: {FilePath}", filePath);
                        return;
                    }
                    _logger.LogDebug("File is not ready yet: {FilePath}", filePath);
                    await Task.Delay(1000, _serviceCts.Token);
                    continue;
                }

                // Get file info to ensure we can access the file
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists || fileInfo.Length == 0)
                {
                    if (retryCount++ >= maxRetries)
                    {
                        _logger.LogWarning("Max retries reached waiting for file: {FilePath}", filePath);
                        return;
                    }
                    _logger.LogDebug("Retrying for file: {FilePath}", filePath);
                    await Task.Delay(2000, _serviceCts.Token); // Wait 2 seconds between retries
                    continue;
                }

                var jobKey = new JobKey($"ProcessVideoJob_{filePath}", "ProcessVideoGroup");
                var triggerKey = new TriggerKey($"ProcessVideoTrigger_{filePath}", "ProcessVideoGroup");

                // Create job detail
                var jobDetail = JobBuilder.Create<ProcessVideoJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData("FilePath", filePath)
                    .Build();

                // Create trigger
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .StartNow()
                    .Build();

                // Schedule the job
                await _scheduler.ScheduleJob(jobDetail, trigger, _serviceCts.Token);
                _logger.LogInformation("Successfully scheduled ProcessVideoJob for {FilePath}", filePath);
                return; // Success - exit the method
            }
            catch (IOException ex)
            {
                if (retryCount++ >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to access file after {RetryCount} retries: {FilePath}", retryCount, filePath);
                    return;
                }
                _logger.LogDebug("File still locked, retry {RetryCount}: {FilePath}", retryCount, filePath);
                await Task.Delay(2000, _serviceCts.Token); // Wait 2 seconds between retries
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing file: {FilePath}", filePath);
                return;
            }
        }
    }

    private bool IsFileReady(string filename)
    {
        try
        {
            // First check if the file exists
            if (!File.Exists(filename))
                return false;

            // Attempt to open with FileShare.None to ensure file is not locked
            using var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            return stream.Length > 0;
        }
        catch (IOException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error checking if file is ready: {FileName}", filename);
            return false;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("File watcher service is stopping...");

        _timer?.Change(Timeout.Infinite, 0);
        _serviceCts.Cancel();

        if (_scheduler != null)
        {
            // Shutdown the scheduler
            await _scheduler.Shutdown(true, cancellationToken);
        }

        // Unsubscribe from events and clean up watchers
        foreach (var watcher in _watchers)
        {
            watcher.FileAdded -= OnFileChanged;
            watcher.FileDeleted -= OnFileDeleted;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _timer?.Dispose();
            _serviceCts.Dispose();

            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
        }

        _isDisposed = true;
    }
}