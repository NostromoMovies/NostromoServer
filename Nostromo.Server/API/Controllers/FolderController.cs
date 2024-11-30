using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Nostromo.Server.Services;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/folders")]
    public class FolderController : ControllerBase
    {
        private readonly ILogger<FolderController> _logger;
        private readonly FileWatcherService _fileWatcherService;
        private readonly IImportFolderRepository _importFolderRepository;

        public FolderController(
            ILogger<FolderController> logger,
            FileWatcherService fileWatcherService,
            IImportFolderRepository importFolderRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileWatcherService = fileWatcherService ?? throw new ArgumentNullException(nameof(fileWatcherService));
            _importFolderRepository = importFolderRepository ?? throw new ArgumentNullException(nameof(importFolderRepository));
        }

        [HttpGet("set")]
        public async Task<IActionResult> SetFolder([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogError("Folder path is required.");
                return BadRequest("Folder path is required.");
            }

            try
            {
                // Check if folder exists in watched paths
                var watchedPaths = await _importFolderRepository.GetWatchedPathsAsync();
                if (watchedPaths.Contains(path))
                {
                    _logger.LogInformation("Folder already exists in the database: {FolderPath}", path);
                    await _fileWatcherService.AddDirectoryToWatchAsync(path);
                    return Conflict($"Folder is already being managed: {path}");
                }

                // Create new import folder
                var newFolder = new ImportFolder
                {
                    FolderLocation = path,
                    IsDropSource = 0,
                    IsDropDestination = 0,
                    IsWatched = 1,
                    ImportFolderType = 0
                };

                await _importFolderRepository.AddAsync(newFolder);

                // Start watching the folder
                await _fileWatcherService.AddDirectoryToWatchAsync(path);

                _logger.LogInformation("Folder added and watching started: {FolderPath}", path);
                return Ok($"Folder added successfully: {path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding folder: {FolderPath}", path);
                return StatusCode(500, "An error occurred while adding the folder.");
            }
        }

        [HttpGet("test")]
        public IActionResult TestEndpoint()
        {
            return Ok("Test endpoint hit!");
        }

        [HttpGet("remove")]
        public async Task<IActionResult> RemoveFolder([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogError("Folder path is required.");
                return BadRequest("Folder path is required.");
            }

            try
            {
                // Find and remove the folder from the database
                var folders = await _importFolderRepository.Query()
                    .Where(f => f.FolderLocation == path)
                    .ToListAsync();

                if (!folders.Any())
                {
                    _logger.LogInformation("Folder not found in database: {FolderPath}", path);
                    return NotFound($"Folder not found: {path}");
                }

                foreach (var folder in folders)
                {
                    await _importFolderRepository.DeleteAsync(folder.ImportFolderID);
                }

                // Stop watching the folder
                await _fileWatcherService.RemoveDirectoryFromWatchAsync(path);

                _logger.LogInformation("Folder removed and watching stopped: {FolderPath}", path);
                return Ok($"Folder removed successfully: {path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing folder: {FolderPath}", path);
                return StatusCode(500, "An error occurred while removing the folder.");
            }
        }
    }
}