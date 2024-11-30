using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Scheduling;

namespace Nostromo.Server.Services
{
    public interface IFileWatcherService
    {
        Task StartWatchingPathsAsync(IEnumerable<string> paths, CancellationToken cancellationToken);
        Task StartWatchingPathAsync(string path);
        Task StopWatchingPathAsync(string path);
    }

    public class FileWatcherService : IFileWatcherService
    {
        private readonly List<FileSystemWatcher> _watchers;
        private readonly IScheduler _scheduler;
        private readonly ILogger<FileWatcherService> _logger;

        public FileWatcherService(IScheduler scheduler, ILogger<FileWatcherService> logger)
        {
            _watchers = new List<FileSystemWatcher>();
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task StartWatchingPathsAsync(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            foreach (var path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await StartWatchingPathAsync(path);
            }
        }

        public async Task StartWatchingPathAsync(string path)
        {
            StartWatchingPath(path);
            await TriggerProcessVideoJob(path);
        }

        public async Task StopWatchingPathAsync(string path)
        {
            var watcher = _watchers.FirstOrDefault(w => w.Path == path);
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                _watchers.Remove(watcher);
                _logger.LogInformation($"Stopped watching: {path}");
            }
        }

        private void StartWatchingPath(string path)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogWarning($"The directory '{path}' does not exist, skipping.");
                return;
            }

            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };

            watcher.Created += async (sender, e) => await OnChangedAsync(e.FullPath, e.ChangeType);
            watcher.Changed += async (sender, e) => await OnChangedAsync(e.FullPath, e.ChangeType);
            watcher.Deleted += async (sender, e) => await OnChangedAsync(e.FullPath, e.ChangeType);

            _watchers.Add(watcher);
            _logger.LogInformation($"Started watching: {path}");
        }

        private async Task OnChangedAsync(string filePath, WatcherChangeTypes changeType)
        {
            _logger.LogInformation($"File {changeType}: {filePath}");

            if (changeType == WatcherChangeTypes.Created)
            {
                const int maxAttempts = 10;
                const int delayMs = 500;

                for (int i = 0; i < maxAttempts; i++)
                {
                    if (IsFileReady(filePath))
                    {
                        await TriggerProcessVideoJob(filePath);
                        return;
                    }

                    _logger.LogInformation($"File not ready, attempt {i + 1} of {maxAttempts}: {filePath}");
                    await Task.Delay(delayMs);
                }

                _logger.LogWarning($"File could not be accessed after {maxAttempts} attempts: {filePath}");
            }
        }

        private async Task TriggerProcessVideoJob(string path)
        {
            try
            {
                var job = JobBuilder.Create<ProcessVideoJob>()
                    .WithIdentity("ProcessVideoJob_" + path)
                    .UsingJobData("FilePath", path)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("ProcessVideoJobTrigger_" + path)
                    .StartNow()
                    .Build();

                await _scheduler.ScheduleJob(job, trigger);
                _logger.LogInformation($"Scheduled ProcessVideoJob for directory: {path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering ProcessVideoJob for path: {Path}", path);
            }
        }

        private bool IsFileReady(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                    return false;

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
    }
}