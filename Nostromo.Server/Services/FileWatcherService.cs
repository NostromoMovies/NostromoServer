using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Server.Utilities.FileSystemWatcher;
using System.Collections.Concurrent;
using Quartz;
using Nostromo.Server.FileHelper;
using System.Collections.Generic;

namespace Nostromo.Server.Services;

public class WatcherSettings
{
    public int ProcessingIntervalSeconds { get; set; } = 5;
}

public class FileWatcherService : IHostedService, IDisposable
{
    private readonly List<RecoveringFileSystemWatcher> _watchers;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _fileProcessing = new();
    private readonly ILogger<FileWatcherService> _logger;
    private readonly WatcherSettings _settings;
    private readonly ConcurrentQueue<string> _changedFiles;
    private Timer? _timer;
    private bool _isDisposed;
    private readonly CancellationTokenSource _serviceCts = new();

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
            watcher.FileAdded += OnFileChanged;
            watcher.FileDeleted += OnFileDeleted;
            watcher.Start();
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

        // Cancel any ongoing processing for the deleted file
        if (_fileProcessing.TryRemove(filePath, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private void DoWork(object? state)
    {
        try
        {
            while (_changedFiles.TryDequeue(out string? filePath))
            {
                try
                {
                    ProcessFileAsync(filePath).ConfigureAwait(false);
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
        // If we're already processing this file, skip
        if (_fileProcessing.ContainsKey(filePath)) return;

        var cts = CancellationTokenSource.CreateLinkedTokenSource(_serviceCts.Token);
        _fileProcessing[filePath] = cts;

        try
        {
            // Wait for file to be ready with timeout
            var readyCheckStart = DateTime.UtcNow;
            while (!IsFileReady(filePath))
            {
                if (DateTime.UtcNow - readyCheckStart > TimeSpan.FromMinutes(5))
                {
                    _logger.LogWarning("Timed out waiting for file to be ready: {FilePath}", filePath);
                    return;
                }
                await Task.Delay(1000, cts.Token);
            }

            _logger.LogInformation("Starting hash calculation for {FilePath}", filePath);

            var result = await NativeHasher.CalculateHashesAsync(
                filePath,
                (filename, progress) =>
                {
                    _logger.LogDebug("Hashing progress for {FileName}: {Progress}%", filename, progress);
                },
                cts.Token
            );

            _logger.LogInformation(
                "File hashed successfully: {FilePath}\nMD5: {MD5}\nSHA1: {SHA1}\nCRC32: {CRC32}\nED2K: {ED2K}\nTime: {Time:N2}s",
                filePath,
                result.MD5,
                result.SHA1,
                result.CRC32,
                result.ED2K,
                result.ProcessingTime.TotalSeconds
            );
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            _logger.LogInformation("Hash calculation cancelled for {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing file: {FilePath}", filePath);
        }
        finally
        {
            if (_fileProcessing.TryRemove(filePath, out var token))
            {
                token.Dispose();
            }
        }
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

        // Cancel all ongoing processing
        foreach (var cts in _fileProcessing.Values)
        {
            cts.Cancel();
        }

        // Wait for cancellation to complete with timeout
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (_fileProcessing.Count > 0 && !timeoutCts.Token.IsCancellationRequested)
        {
            await Task.Delay(100, timeoutCts.Token);
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

            foreach (var cts in _fileProcessing.Values)
            {
                cts.Dispose();
            }
            _fileProcessing.Clear();
        }

        _isDisposed = true;
    }
}