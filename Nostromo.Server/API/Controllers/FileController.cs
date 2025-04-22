using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Nostromo.Server.Scheduling.Jobs;
using Microsoft.AspNetCore.Http;
using Nostromo.Server.API.Models;
using Nostromo.Server.Database;
using Nostromo.Server.Services;
using Nostromo.Server.API.Enums;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IFileRenamerService _fileRenamerService;

        public FileController(
            ILogger<FileController> logger,
            ISchedulerFactory schedulerFactory,
            IFileRenamerService fileRenamerService)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
            _fileRenamerService = fileRenamerService;
        }

        [HttpGet("download/tmdb/{mediaType}/{mediaId}/poster")]
        public async Task<IResult> DownloadTmdbPoster(string mediaType, int mediaId, int? seasonNumber = null, int? episodeNumber = null)
        {
            try
            {
                if (!Enum.TryParse<MediaTypes>(mediaType, true, out var parsedMediaType))
                {
                    return ApiResults.BadRequest("Invalid media type: Allowed values are: movie, tv, season and episode");
                }
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey($"DownloadPoster_{mediaType}_{mediaId}", "TmdbDownloads");
                var triggerKey = new TriggerKey($"DownloadPosterTrigger_{mediaType}_{mediaId}", "TmdbDownloads");
  
                var jobDetail = JobBuilder.Create<DownloadTmdbImageJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData(DownloadTmdbImageJob.MEDIA_ID_KEY, mediaId)
                    .UsingJobData(DownloadTmdbImageJob.MEDIA_TYPE_KEY, mediaType)
                    .Build();

                if (parsedMediaType == MediaTypes.Season || parsedMediaType == MediaTypes.Episode)
                {
                    if (seasonNumber == null)
                    {
                        return ApiResults.BadRequest("Season number is required for season and episode posters");
                    }
                    jobDetail.JobDataMap.Put(DownloadTmdbImageJob.SEASON_NUMBER_KEY, seasonNumber.Value);
                }

                if (parsedMediaType == MediaTypes.Episode)
                {
                    if (episodeNumber == null)
                    {
                        return ApiResults.BadRequest("Episode number is required for episode posters");
                    }
                    jobDetail.JobDataMap.Put(DownloadTmdbImageJob.EPISODES_NUMBER_KEY, episodeNumber.Value);
                }
                
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .StartNow()
                    .Build();

                await scheduler.ScheduleJob(jobDetail, trigger);
                _logger.LogInformation("Scheduled poster download job for {MediaType} {MediaId}", mediaType, mediaId);

                return ApiResults.Success(new
                {
                    Message = $"Download job scheduled for {mediaType} {mediaId}",
                    JobKey = jobKey.ToString(),
                    TriggerKey = triggerKey.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling download job for {MediaType} {MediaId}", mediaType, mediaId);
                return ApiResults.ServerError($"Error scheduling download job: {ex.Message}");
            }
        }

        [HttpGet("job/{group}/{name}/status")]
        public async Task<IResult> GetJobStatus(string group, string name)
        {
            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey(name, group);
                var jobDetail = await scheduler.GetJobDetail(jobKey);

                if (jobDetail == null)
                {
                    return ApiResults.NotFound("Job not found");
                }

                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                var trigger = triggers.FirstOrDefault();
                var state = await scheduler.GetTriggerState(trigger?.Key ?? new TriggerKey("unknown"));

                return ApiResults.Success(new
                {
                    JobKey = jobKey.ToString(),
                    Status = state.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking job status for {Group}/{Name}", group, name);
                return ApiResults.ServerError($"Error checking job status: {ex.Message}");
            }
        }

        [HttpPost("move")]
        public async Task<IResult> MoveFile(string file, string newFile)
        {
            var result = await _fileRenamerService.RenameFile(file, newFile);
            return result ? ApiResults.Success("File renamed") : ApiResults.ServerError("Error");
        }

        [HttpPost("rename_with_metadata")]
        public async Task<IResult> RenameWithMetadata(int videoPlaceID)
        {
            var result = await _fileRenamerService.RenameWithMetadata(videoPlaceID);
            return result ? ApiResults.Success("File renamed") : ApiResults.ServerError("Error");
        }
    }
}