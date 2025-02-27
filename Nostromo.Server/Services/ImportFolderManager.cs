using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.API.Models;
using System.Linq;

namespace Nostromo.Server.Services
{
    public interface IImportFolderManager
    {
        Task<bool> AddImportFolderAsync(string path, int type);
        Task<bool> RemoveImportFolderAsync(string path);
        Task<List<string>> GetWatchedFoldersAsync();
        Task<bool> IsFolderWatchedAsync(string path);
        Task InitializeWatchersAsync(CancellationToken cancellationToken);
    }

    public class ImportFolderManager : IImportFolderManager
    {
        private readonly IImportFolderRepository _importFolderRepository;
        private readonly IFileWatcherService _fileWatcherService;
        private readonly ILogger<ImportFolderManager> _logger;

        public ImportFolderManager(
            IImportFolderRepository importFolderRepository,
            IFileWatcherService fileWatcherService,
            ILogger<ImportFolderManager> logger)
        {
            _importFolderRepository = importFolderRepository;
            _fileWatcherService = fileWatcherService;
            _logger = logger;
        }
 
        public async Task InitializeWatchersAsync(CancellationToken cancellationToken)
        {
            try
            {
                var watchedPaths = await GetWatchedFoldersAsync();
                await _fileWatcherService.StartWatchingPathsAsync(watchedPaths, cancellationToken);
                _logger.LogInformation("Initialized watchers for {Count} folders", watchedPaths.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing folder watchers");
                throw;
            }
        }

        public async Task<bool> AddImportFolderAsync(string path, int type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogError("Path cannot be null or empty");
                    return false;
                }

                if (!Directory.Exists(path))
                {
                    _logger.LogError("Directory does not exist: {Path}", path);
                    return false;
                }

                if (await IsFolderWatchedAsync(path))
                {
                    _logger.LogInformation("Folder is already being watched: {Path}", path);
                    return true;
                }

                var newFolder = new ImportFolder
                {
                    FolderLocation = path,
                    IsDropSource = 0,
                    IsDropDestination = 0,
                    IsWatched = 1,
                    ImportFolderType = type
                };

                await _importFolderRepository.AddAsync(newFolder);
                await _fileWatcherService.StartWatchingPathAsync(path);

                _logger.LogInformation("Successfully added and started watching folder: {Path}", path);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding import folder: {Path}", path);
                return false;
            }
        }

        public async Task<bool> RemoveImportFolderAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogError("Path cannot be null or empty");
                    return false;
                }

                var folders = await _importFolderRepository.Query()
                    .Where(f => f.FolderLocation == path)
                    .ToListAsync();

                if (!folders.Any())
                {
                    _logger.LogInformation("Folder not found: {Path}", path);
                    return false;
                }

                foreach (var folder in folders)
                {
                    await _importFolderRepository.DeleteAsync(folder.ImportFolderID);
                }

                await _fileWatcherService.StopWatchingPathAsync(path);
                _logger.LogInformation("Successfully removed folder from watching: {Path}", path);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing import folder: {Path}", path);
                return false;
            }
        }

        public async Task<List<string>> GetWatchedFoldersAsync()
        {
            try
            {
                return await _importFolderRepository.GetWatchedPathsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting watched folders");
                return new List<string>();
            }
        }

        public async Task<bool> IsFolderWatchedAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return false;
                }

                var watchedPaths = await GetWatchedFoldersAsync();
                return watchedPaths.Contains(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if folder is watched: {Path}", path);
                return false;
            }
        }
    }
}