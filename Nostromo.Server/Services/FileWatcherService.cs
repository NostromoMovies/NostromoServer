using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Server.Utilities.FileSystemWatcher;
using System.Collections.Concurrent;
using Quartz;
using Nostromo.Server.FileHelper;

namespace Nostromo.Server.Services;

public class WatcherSettings
{
    public int ProcessingIntervalSeconds { get; set; } = 5;
}

public class FileWatcherService : IHostedService, IDisposable
{
    private readonly List<RecoveringFileSystemWatcher> _watchers;
    private readonly ConcurrentDictionary<string, Timer> _fileTimers = new();
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
        if (_watchers.Count == 0) throw new ArgumentException("At least one watcher must be provided", nameof(watchers));

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
        _logger.LogInformation("ProcessFile called for: {FilePath}", filePath);

        // If we're already processing this file, skip
        if (_fileTimers.ContainsKey(filePath))
        {
            _logger.LogDebug("File already being processed: {FilePath}", filePath);
            return;
        }

        _logger.LogInformation("Setting up timer for: {FilePath}", filePath);

        var timer = new Timer(async (state) =>
        {
            try
            {
                _logger.LogDebug("Timer triggered for: {FilePath}", filePath);

                if (!IsFileReady(filePath))
                {
                    _logger.LogDebug("File not ready yet: {FilePath}", filePath);
                    return;
                }

                _logger.LogInformation("=== Starting hash calculation for {FilePath} ===", filePath);

                var hashes = Hasher.CalculateHashes(filePath, (filename, progress) =>
                {
                    if (progress % 10 == 0) // Log every 10%
                    {
                        _logger.LogInformation("Hashing progress for {FileName}: {Progress}%", filename, progress);
                    }
                    return 0;
                });

                _logger.LogInformation(
                    "=== Hash Results for {FilePath} ===\n" +
                    "MD5:   {MD5}\n" +
                    "SHA1:  {SHA1}\n" +
                    "CRC32: {CRC32}",
                    filePath, hashes.MD5, hashes.SHA1, hashes.CRC32
                );

                // Clean up timer
                if (_fileTimers.TryRemove(filePath, out var oldTimer))
                {
                    _logger.LogDebug("Cleaned up timer for: {FilePath}", filePath);
                    oldTimer.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FilePath}", filePath);

                if (_fileTimers.TryRemove(filePath, out var oldTimer))
                {
                    oldTimer.Dispose();
                }
            }
        }, null, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);

        _fileTimers[filePath] = timer;
    }

    private static bool IsFileReady(string filename)
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
            foreach (var fileTimer in _fileTimers.Values)
            {
                fileTimer.Dispose();
            }
            _fileTimers.Clear();
        }

        _isDisposed = true;
    }
}