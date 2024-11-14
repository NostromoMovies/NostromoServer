using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Server.Utilities.FileSystemWatcher;
using System.Collections.Concurrent;
using Quartz;
using Nostromo.Server.Scheduling;

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
        ISchedulerFactory schedulerFactory)
    {
        _watchers = watchers?.ToList() ?? throw new ArgumentNullException(nameof(watchers));
        if (_watchers.Count == 0) throw new ArgumentException("At least one watcher must be provided", nameof(watchers));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings.Value;
        _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        _changedFiles = new ConcurrentQueue<string>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("File watcher service is starting...");

        // Initialize and start the Quartz scheduler
        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await _scheduler.Start(cancellationToken);

        foreach (var watcher in _watchers)
        {
            watcher.FileAdded += OnFileChanged;
            watcher.FileDeleted += OnFileDeleted;
            watcher.Start();
        }

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(_settings.ProcessingIntervalSeconds));
    }

    private void OnFileChanged(object? sender, string filePath)
    {
        _logger.LogDebug("File change detected: {FilePath}", filePath);
        _changedFiles.Enqueue(filePath);
    }

    private void OnFileDeleted(object? sender, string filePath)
    {
        _logger.LogInformation("File deleted: {FilePath}", filePath);

        // Try to cancel any scheduled job for this file
        if (_scheduler != null)
        {
            var jobKey = new JobKey($"HashJob_{filePath}", "FileHashing");
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

        // Wait for file to be ready with timeout
        var readyCheckStart = DateTime.UtcNow;
        while (!IsFileReady(filePath))
        {
            if (DateTime.UtcNow - readyCheckStart > TimeSpan.FromMinutes(5))
            {
                _logger.LogWarning("Timed out waiting for file to be ready: {FilePath}", filePath);
                return;
            }
            await Task.Delay(1000, _serviceCts.Token);
        }

        var jobKey = new JobKey($"HashJob_{filePath}", "FileHashing");
        var triggerKey = new TriggerKey($"HashTrigger_{filePath}", "FileHashing");

        // Create job detail
        var jobDetail = JobBuilder.Create<HashFileJob>()
            .WithIdentity(jobKey)
            .UsingJobData(HashFileJob.FILE_PATH_KEY, filePath)
            .Build();

        // Create trigger
        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .StartNow()
            .Build();

        // Schedule the job
        await _scheduler.ScheduleJob(jobDetail, trigger, _serviceCts.Token);
    }

    private bool IsFileReady(string filename)
    {
        try
        {
            using var inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None);
            return inputStream.Length > 0;
        }
        catch (IOException)
        {
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