using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Server.Utilities.FileSystemWatcher;
using System.Collections.Concurrent;
using Quartz;

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
    private Timer? _timer;
    private bool _isDisposed;

    public FileWatcherService(
        IEnumerable<RecoveringFileSystemWatcher> watchers,
        ILogger<FileWatcherService> logger,
        IOptions<WatcherSettings> settings)
    {
        _watchers = watchers?.ToList() ?? throw new ArgumentNullException(nameof(watchers));
        if (!_watchers.Any()) throw new ArgumentException("At least one watcher must be provided", nameof(watchers));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings.Value;
        _changedFiles = new ConcurrentQueue<string>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("File watcher service is starting...");

        foreach (var watcher in _watchers)
        {
            // Subscribe to watcher events
            watcher.FileAdded += OnFileChanged;
            watcher.FileDeleted += OnFileDeleted;

            // Start the watcher
            watcher.Start();

            //_logger.LogInformation("Watching directory: {Directory}", watcher.WatchedDirectory);
        }

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(_settings.ProcessingIntervalSeconds));

        return Task.CompletedTask;
    }

    private void OnFileChanged(object? sender, string filePath)
    {
        _logger.LogDebug("File change detected: {FilePath}", filePath);
        _changedFiles.Enqueue(filePath);
    }

    private void OnFileDeleted(object? sender, string filePath)
    {
        _logger.LogInformation("File deleted: {FilePath}", filePath);
        // Handle deletions if needed
    }

    private void DoWork(object? state)
    {
        try
        {
            while (_changedFiles.TryDequeue(out string? filePath))
            {
                try
                {
                    ProcessFile(filePath);
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

    private void ProcessFile(string filePath)
    {
        _logger.LogInformation("Processing file: {FilePath}", filePath);

        // TODO: Add your file processing logic here
        // For example:
        // 1. Validate the file is complete/not being written to
        // 2. Move to processing directory
        // 3. Update database
        // 4. Trigger media processing
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("File watcher service is stopping...");

        _timer?.Change(Timeout.Infinite, 0);

        // Process any remaining files
        while (_changedFiles.TryDequeue(out string? filePath))
        {
            try
            {
                ProcessFile(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing remaining file: {FilePath}", filePath);
            }
        }

        // Unsubscribe from events and clean up watchers
        foreach (var watcher in _watchers)
        {
            watcher.FileAdded -= OnFileChanged;
            watcher.FileDeleted -= OnFileDeleted;
        }

        await Task.CompletedTask;
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
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
        }

        _isDisposed = true;
    }
}