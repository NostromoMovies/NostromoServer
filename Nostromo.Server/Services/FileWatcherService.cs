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
        private readonly IFileSystemRepository _fileSystemRepository;
        private readonly List<FileSystemWatcher> _watchers;
        private readonly IScheduler _scheduler;
        private readonly ILogger<FileWatcherService> _logger;

        public FileWatcherService(IFileSystemRepository fileSystemRepository, IScheduler scheduler, ILogger<FileWatcherService> logger)
        {
            _fileSystemRepository = fileSystemRepository;
            _watchers = new List<FileSystemWatcher>();
            _scheduler = scheduler;
            _logger = logger;
        }

        // Start watching the folders that are loaded from the database
        public async Task StartWatchingAsync(CancellationToken cancellationToken)
        {
            var watchedPaths = await _fileSystemRepository.LoadWatchedPathsAsync();

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
            await _fileSystemRepository.SaveWatchedPathsAsync(new List<string> { path });

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

            // Remove the path from the database
            await _fileSystemRepository.SaveWatchedPathsAsync(new List<string> { path });
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

        // Event handler for file system changes
        private async Task OnChangedAsync(string filePath, WatcherChangeTypes changeType)
        {
            _logger.LogInformation($"File {changeType}: {filePath}");

            // Trigger the ProcessVideoJob when a file is created or modified
            if (changeType == WatcherChangeTypes.Created)
            {
                await TriggerProcessVideoJob(filePath);
            }
        }

        // Method to trigger the ProcessVideoJob
        private async Task TriggerProcessVideoJob(string path)
        {
            try
            {
                var job = JobBuilder.Create<ProcessVideoJob>()
                    .WithIdentity("ProcessVideoJob_" + path)  // Unique job identity based on the directory path
                    .UsingJobData("FilePath", path)  // Pass directory path to the job
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("ProcessVideoJobTrigger_" + path)
                    .StartNow()  // Start the job immediately
                    .Build();

                // Schedule the job with Quartz
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
