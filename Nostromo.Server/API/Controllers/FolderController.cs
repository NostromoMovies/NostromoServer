using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;
using Nostromo.Server.Services;
using Nostromo.Server.Database;
using Nostromo.Server.Scheduling;
using Microsoft.EntityFrameworkCore;  // Add this for EF Core

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/folders")]
    public class FolderController : ControllerBase
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger<FolderController> _logger;
        private readonly FileWatcherService _fileWatcherService;
        private readonly NostromoDbContext _dbContext;

        public FolderController(IScheduler scheduler, ILogger<FolderController> logger, FileWatcherService fileWatcherService, NostromoDbContext dbContext)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileWatcherService = fileWatcherService ?? throw new ArgumentNullException(nameof(fileWatcherService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Handle folder setting (using GET for query parameter)
        [HttpGet("set")]
        public async Task<IActionResult> SetFolder([FromQuery] string path)  // Accepting the path as a query parameter
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogError("Folder path is required.");
                return BadRequest("Folder path is required.");
            }

            // Check if the folder already exists in the database
            var folderExists = await _dbContext.ImportFolders
                .AnyAsync(f => f.FolderLocation == path);

            if (folderExists)
            {
                _logger.LogInformation("Folder already exists in the database: {FolderPath}", path);
                // Optionally, check if it's being watched
                _fileWatcherService.AddDirectoryToWatchAsync(path); // Add to the watcher if needed
                return Conflict($"Folder is already being managed: {path}");
            }

            var jobKey = new JobKey($"FolderManagementJob_Set_{path}", "FolderGroup");
            var jobDetail = JobBuilder.Create<FolderManagementJob>()
                .WithIdentity(jobKey)
                .UsingJobData("FolderPath", path)
                .UsingJobData("Action", "create") // Action to create folder
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"FolderTrigger_Set_{path}", "FolderGroup")
                .StartNow()
                .Build();

            try
            {
                // Schedule folder management job for creating the folder
                await _scheduler.ScheduleJob(jobDetail, trigger);

                // Add folder to FileWatcherService
                _fileWatcherService.AddDirectoryToWatchAsync(path);

                _logger.LogInformation("Folder creation job scheduled for {FolderPath}", path);
                return Ok($"Folder creation job scheduled for: {path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling folder creation job.");
                return StatusCode(500, "An error occurred while scheduling the folder creation job.");
            }
        }

        [HttpGet("test")]
        public IActionResult TestEndpoint()
        {
            return Ok("Test endpoint hit!");
        }


        // Handle folder removal (using GET for query parameter)
        [HttpGet("remove")]
        public async Task<IActionResult> RemoveFolder([FromQuery] string path)  // Accepting the path as a query parameter
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogError("Folder path is required.");
                return BadRequest("Folder path is required.");
            }

            var jobKey = new JobKey($"FolderManagementJob_Remove_{path}", "FolderGroup");
            var jobDetail = JobBuilder.Create<FolderManagementJob>()
                .WithIdentity(jobKey)
                .UsingJobData("FolderPath", path)
                .UsingJobData("Action", "remove") // Action to remove folder
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"FolderTrigger_Remove_{path}", "FolderGroup")
                .StartNow()
                .Build();

            try
            {
                // Schedule folder management job for removing the folder
                await _scheduler.ScheduleJob(jobDetail, trigger);

                // Remove folder from FileWatcherService
                _fileWatcherService.RemoveDirectoryFromWatchAsync(path);

                _logger.LogInformation("Folder removal job scheduled for {FolderPath}", path);
                return Ok($"Folder removal job scheduled for: {path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling folder removal job.");
                return StatusCode(500, "An error occurred while scheduling the folder removal job.");
            }
        }
    }
}

