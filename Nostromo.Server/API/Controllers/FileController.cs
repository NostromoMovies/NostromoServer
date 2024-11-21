using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Nostromo.Server.Scheduling.Jobs;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly ISchedulerFactory _schedulerFactory;

        public FileController(
            ILogger<FileController> logger,
            ISchedulerFactory schedulerFactory)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
        }

        [HttpPost("download/tmdb/{movieId}/poster")]
        public async Task<IActionResult> DownloadTmdbPoster(int movieId)
        {
            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();

                var jobKey = new JobKey($"DownloadPoster_{movieId}", "TmdbDownloads");
                var triggerKey = new TriggerKey($"DownloadPosterTrigger_{movieId}", "TmdbDownloads");

                // Create job detail
                var jobDetail = JobBuilder.Create<DownloadTmdbImageJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData("MovieId", movieId)
                    .UsingJobData("SavePath", Path.Combine("Images", "Posters", $"{movieId}_poster.jpg"))
                    .Build();

                // Create trigger
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .StartNow()
                    .Build();

                // Schedule the job
                await scheduler.ScheduleJob(jobDetail, trigger);

                _logger.LogInformation("Scheduled poster download job for movie {MovieId}", movieId);

                return Ok(new
                {
                    Message = $"Download job scheduled for movie {movieId}",
                    JobKey = jobKey.ToString(),
                    TriggerKey = triggerKey.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling download job for movie {MovieId}", movieId);
                return StatusCode(500, new { Message = "Error scheduling download job", Error = ex.Message });
            }
        }

        [HttpGet("job/{group}/{name}/status")]
        public async Task<IActionResult> GetJobStatus(string group, string name)
        {
            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey(name, group);

                var jobDetail = await scheduler.GetJobDetail(jobKey);
                if (jobDetail == null)
                {
                    return NotFound(new { Message = "Job not found" });
                }

                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                var trigger = triggers.FirstOrDefault();
                var state = await scheduler.GetTriggerState(trigger?.Key ?? new TriggerKey("unknown"));

                return Ok(new { JobKey = jobKey.ToString(), Status = state.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking job status for {Group}/{Name}", group, name);
                return StatusCode(500, new { Message = "Error checking job status", Error = ex.Message });
            }
        }
    }
}