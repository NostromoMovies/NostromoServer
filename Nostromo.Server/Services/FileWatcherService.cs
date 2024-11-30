using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Quartz;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Scheduling;

namespace Nostromo.Server.Services
{
    public class FileWatcherService
    {
        private readonly IImportFolderRepository _importFolderRepository;
        private readonly List<FileSystemWatcher> _watchers;
        private readonly IScheduler _scheduler;
        private readonly ILogger<FileWatcherService> _logger;

        public FileWatcherService(IImportFolderRepository importFolderRepository, IScheduler scheduler, ILogger<FileWatcherService> logger)
        {
            _importFolderRepository = importFolderRepository;
            _watchers = new List<FileSystemWatcher>();
            _scheduler = scheduler;
            _logger = logger;
        }

        // Start watching the folders that are loaded from the database
        public async Task StartWatchingAsync(CancellationToken cancellationToken)
        {
            var watchedPaths = await _importFolderRepository.GetWatchedPathsAsync();

            foreach (var path in watchedPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                StartWatchingPath(path);
            }
        }

        // Dynamically add a new directory to be watched
        public async Task AddDirectoryToWatchAsync(string path)
        {
            // Save the new path to the database
            await _importFolderRepository.SaveWatchedPathsAsync(new List<string> { path });

            // Start watching the new path
            StartWatchingPath(path);

            // Trigger the ProcessVideoJob for the new directory
            await TriggerProcessVideoJob(path);
        }

        // Dynamically remove a directory from the watch list
        public async Task RemoveDirectoryFromWatchAsync(string path)
        {
            var watcher = _watchers.FirstOrDefault(w => w.Path == path);
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                _watchers.Remove(watcher);
            }

            // Get all currently watched paths
            var watchedPaths = await _importFolderRepository.GetWatchedPathsAsync();

            // Remove the specified path and save the updated list
            watchedPaths.Remove(path);
            await _importFolderRepository.SaveWatchedPathsAsync(watchedPaths);
        }

        // Helper method to start watching a specific path
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

            // Add event handlers for changes in the directory
            watcher.Created += async (sender, e) =>
            {
                await OnChangedAsync(e.FullPath, e.ChangeType);
            };
            watcher.Changed += async (sender, e) =>
            {
                await OnChangedAsync(e.FullPath, e.ChangeType);
            };
            watcher.Deleted += async (sender, e) =>
            {
                await OnChangedAsync(e.FullPath, e.ChangeType);
            };

            _watchers.Add(watcher);
            _logger.LogInformation($"Started watching: {path}");
        }

        private async Task OnChangedAsync(string filePath, WatcherChangeTypes changeType)
        {
            _logger.LogInformation($"File {changeType}: {filePath}");

            if (changeType == WatcherChangeTypes.Created)
            {
                await TriggerProcessVideoJob(filePath);
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
    }
}