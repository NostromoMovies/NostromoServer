using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

namespace Nostromo.Server.Scheduling
{
    public class FolderManagementJob : IJob
    {
        private readonly ILogger<FolderManagementJob> _logger;
        private readonly NostromoDbContext _dbContext;
        private readonly IScheduler _scheduler;

        public FolderManagementJob(ILogger<FolderManagementJob> logger, NostromoDbContext dbContext, IScheduler scheduler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var folderPath = context.MergedJobDataMap.GetString("FolderPath");
            var action = context.MergedJobDataMap.GetString("Action");

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(action))
            {
                _logger.LogError("Folder path or action is missing.");
                return;
            }

            try
            {
                if (action.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    await AddFolderToDatabaseAsync(folderPath);
                    CreateFileSystemWatcher(folderPath);  // Create the watcher if the folder is added
                }
                else if (action.Equals("remove", StringComparison.OrdinalIgnoreCase))
                {
                    await RemoveFolderFromDatabaseAsync(folderPath);
                }
                else
                {
                    _logger.LogError("Invalid action specified: {Action}", action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error managing folder: {FolderPath}", folderPath);
            }
        }

        private async Task AddFolderToDatabaseAsync(string folderPath)
        {
            var existingFolder = await _dbContext.ImportFolders
                .FirstOrDefaultAsync(f => f.FolderLocation == folderPath);

            if (existingFolder != null)
            {
                _logger.LogInformation("Folder already exists in the database: {FolderPath}", folderPath);
                return;
            }

            var newFolder = new ImportFolder
            {
                FolderLocation = folderPath,
                IsDropSource = 0,  // Set appropriate default values or based on your logic
                IsDropDestination = 0,
                IsWatched = 1  // Assuming you want to watch the folder by default
            };

            _dbContext.ImportFolders.Add(newFolder);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Folder added to database: {FolderPath}", folderPath);
        }

        private async Task RemoveFolderFromDatabaseAsync(string folderPath)
        {
            var folder = await _dbContext.ImportFolders
                .FirstOrDefaultAsync(f => f.FolderLocation == folderPath);

            if (folder == null)
            {
                _logger.LogInformation("Folder does not exist in the database: {FolderPath}", folderPath);
                return;
            }

            _dbContext.ImportFolders.Remove(folder);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Folder removed from database: {FolderPath}", folderPath);
        }

        // Method to create a FileSystemWatcher and start watching a folder
        private void CreateFileSystemWatcher(string folderPath)
        {
            try
            {
                // Check if the folder exists before creating the watcher
                if (!Directory.Exists(folderPath))
                {
                    _logger.LogWarning("The folder does not exist: {FolderPath}", folderPath);
                    return;
                }

                var fileSystemWatcher = new FileSystemWatcher(folderPath)
                {
                    EnableRaisingEvents = true,  // Enable event raising
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite  // What events to watch for
                };

                // Event handlers for file system changes
                fileSystemWatcher.Created += (sender, e) =>
                {
                    _logger.LogInformation("File created: {FilePath}", e.FullPath);
                };

                fileSystemWatcher.Deleted += (sender, e) =>
                {
                    _logger.LogInformation("File deleted: {FilePath}", e.FullPath);
                };

                fileSystemWatcher.Changed += (sender, e) =>
                {
                    _logger.LogInformation("File changed: {FilePath}", e.FullPath);
                };

                fileSystemWatcher.Renamed += (sender, e) =>
                {
                    _logger.LogInformation("File renamed from {OldFilePath} to {NewFilePath}", e.OldFullPath, e.FullPath);
                };

                _logger.LogInformation("Started watching folder: {FolderPath}", folderPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating file system watcher for folder: {FolderPath}", folderPath);
            }
        }
    }
}
