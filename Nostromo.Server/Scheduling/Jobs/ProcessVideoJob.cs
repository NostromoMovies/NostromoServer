using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.IO;
using System.Threading.Tasks;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Services;

namespace Nostromo.Server.Scheduling;

public class ProcessVideoJob : BaseJob
{
    private readonly ILogger<ProcessVideoJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ProcessVideoJob(
        ILogger<ProcessVideoJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public override string Name => "Process Video Job";
    public override string Type => "Workflow Orchestration";

    public override async Task ProcessJob()
    {
        var jobDataMap = Context.MergedJobDataMap;
        var filePath = jobDataMap.GetString("FilePath");

        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogError("FilePath is missing in job data map.");
            return;
        }

        _logger.LogInformation("Starting ProcessVideoJob for file: {FilePath}", filePath);

        var jobId = Guid.NewGuid().ToString();
        var hashJobKey = new JobKey(jobId, "ConsolidateGroup");

        var hashJob = JobBuilder.Create<HashFileJob>()
            .UsingJobData(HashFileJob.FILE_PATH_KEY, filePath)
            .WithIdentity(hashJobKey)
            .Build();

        var hashTrigger = TriggerBuilder.Create()
            .StartNow()
            .WithIdentity(new TriggerKey(Guid.NewGuid().ToString(), "ConsolidateGroup"))
            .Build();

        await Context.Scheduler.ScheduleJob(hashJob, hashTrigger);

        _logger.LogInformation("Scheduled HashFileJob with Job ID: {JobId} for file: {FilePath}", jobId, filePath);

        _logger.LogInformation("Process video job workflow for file {FilePath} is complete.", filePath);
    }
}

/*using System;
using System.IO;
using System.Threading.Tasks;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Services;

namespace Nostromo.Server.Scheduling;

public class ProcessVideoJob : BaseJob
{
    private readonly ILogger<ProcessVideoJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    
    public ProcessVideoJob(
        ILogger<ProcessVideoJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }


    public override string Name => "Process Video Job";
    public override string Type => "Workflow Orchestration";

    public override async Task ProcessJob()
    {
        var jobDataMap = Context.MergedJobDataMap;
        var filePath = jobDataMap.GetString("FilePath");

        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogError("FilePath is missing in job data map.");
            return;
        }

        _logger.LogInformation("Starting ProcessVideoJob for file: {FilePath}", filePath);

        // Wait for the hash job to finish and retrieve the computed hash
        var computedHash = await ComputeHashAndScheduleMetadataJob(filePath);
        if (string.IsNullOrWhiteSpace(computedHash))
        {
            _logger.LogError("Failed to compute hash for file: {FilePath}. Aborting metadata job scheduling.", filePath);
            return;
        }

        _logger.LogInformation("Process video job workflow for file {FilePath} is complete.", filePath);
    }

    private async Task<string> ComputeHashAndScheduleMetadataJob(string filePath)
    {
        var jobId = Guid.NewGuid().ToString();

        var hashJobKey = new JobKey(jobId, "ConsolidateGroup");

        var hashJob = JobBuilder.Create<HashFileJob>()
            .UsingJobData(HashFileJob.FILE_PATH_KEY, filePath)
            .WithIdentity(hashJobKey)
            .Build();

        var hashTrigger = TriggerBuilder.Create()
            .StartNow()
            .WithIdentity(new TriggerKey(Guid.NewGuid().ToString(), "ConsolidateGroup"))
            .Build();

        await Context.Scheduler.ScheduleJob(hashJob, hashTrigger);

        _logger.LogInformation("Scheduled HashFileJob with Job ID: {JobId} for file: {FilePath}", jobId, filePath);

        // Wait for the hash to be computed
        string computedHash = await WaitForHashCompletion(filePath);

        if (string.IsNullOrWhiteSpace(computedHash))
        {
            _logger.LogError("Hash computation failed for file: {FilePath}", filePath);
            return null;
        }

        _logger.LogInformation("Computed hash for file {FilePath}: {Hash}", filePath, computedHash);

        var metadataJobId = Guid.NewGuid().ToString();
        var metadataJobKey = new JobKey(metadataJobId, "ConsolidateGroup");

        var metadataJob = JobBuilder.Create<DownloadMovieMetadataJob>()
            .UsingJobData(DownloadMovieMetadataJob.HASH_KEY, computedHash)
            .WithIdentity(metadataJobKey)
            .Build();

        var metadataTrigger = TriggerBuilder.Create()
            .StartNow()
            .WithIdentity(new TriggerKey(Guid.NewGuid().ToString(), "ConsolidateGroup"))
            .Build();

        await Context.Scheduler.ScheduleJob(metadataJob, metadataTrigger);
        
        //await TemporaryMetadataJob(computedHash);
        _logger.LogInformation("Scheduled DownloadMovieMetadataJob with Job ID: {JobId} for hash: {Hash}", metadataJobId, computedHash);

        return computedHash;
    }

    private async Task TemporaryMetadataJob(string fileHash)
    {
        //check if hash value is null or empty, if it is -- break
        if (string.IsNullOrWhiteSpace(fileHash))
        {
            _logger.LogError("File hash is missing or invalid for metadata download.");
            return;
        }

        _logger.LogInformation("Starting direct metadata download for file hash: {FileHash}", fileHash);

        try
        {
            _logger.LogInformation("Step 1: {FileHash}", fileHash);
            var scope = _serviceProvider.CreateScope();
            var _databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
            var _tmdbService = scope.ServiceProvider.GetRequiredService<ITmdbService>();
            
            var videoId = await _databaseService.GetVideoIdByHashAsync(fileHash);
            if (videoId == null)
            {
                _logger.LogWarning("No video found in Videos table for hash: {FileHash}", fileHash);
                return;
            }
            _logger.LogInformation("Step 2: {FileHash}", fileHash);
            
            var createdAt = await _databaseService.GetCreatedAtByVideoIdAsync(videoId);
            _logger.LogInformation("Found VideoID {VideoID} for hash: {FileHash}", videoId, fileHash);

            var movieId = Context.JobDetail.JobDataMap.GetInt("TMDBMovieID");

            _logger.LogInformation("Found MovieID {MovieID} for VideoID {VideoID}", movieId, videoId);

            var movieDetails = await _databaseService.GetMovieAsync(movieId);
            if (movieDetails == null)
            {
                _logger.LogWarning("Movie details not found for MovieID: {MovieID}", movieId);
                return;
            }

            await _databaseService.InsertExampleHashAsync(fileHash, movieId, movieDetails.Title);
            _logger.LogInformation("Inserted ExampleHash entry: {FileHash}, {MovieID}, {Title}", fileHash, movieId, movieDetails.Title);

            int retryCount = 0;
            while (retryCount < 10)
            {
                await Task.Delay(500);
                var (verifyInsertion, _, _) = await _databaseService.GetMovieIdByHashAsync(fileHash);
                
                if (verifyInsertion != null)
                {
                    _logger.LogInformation("Confirmed ExampleHash insertion for {Hash}", fileHash);
                    break;
                }
                retryCount++;
            }

            try
            {
                var creditsWrapper = await _tmdbService.GetMovieCreditsAsync(movieId);
                if (creditsWrapper?.Cast != null)
                {
                    await _databaseService.StoreMovieCastAsync(movieId, creditsWrapper.Cast);
                    _logger.LogInformation("Stored cast for movie ID {MovieId}", movieId);
                    await _databaseService.StoreMovieCrewAsync(movieId, creditsWrapper.Crew);
                    _logger.LogInformation("Stored crew for movie ID {MovieId}", movieId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing movie cast for ID {MovieId}", movieId);
            }

            var crossRef = new CrossRefVideoTMDBMovie
            {
                TMDBMovieID = movieId,
                VideoID = videoId.Value,
                CreatedAt = createdAt
            };

            await _databaseService.InsertCrossRefAsync(crossRef);
            _logger.LogInformation("Linked TMDBMovieID {TMDBMovieID} to VideoID {VideoID}", movieId, videoId);

            await _databaseService.MarkVideoAsRecognizedAsync(videoId);
            _logger.LogInformation("Marked VideoID {VideoID} as recognized.", videoId);

            try
            {
                var recommendationsResponse = await _tmdbService.GetRecommendation(movieId);
                if (recommendationsResponse != null && recommendationsResponse.Results.Any())
                {
                    foreach (var recommendation in recommendationsResponse.Results)
                    {
                        await _databaseService.StoreTmdbRecommendationsAsync(movieId, recommendation);
                    }

                    _logger.LogInformation("Successfully stored {Count} recommendations for MovieID {MovieID}",
                        recommendationsResponse.Results.Count, movieId);
                }
                else
                {
                    _logger.LogWarning("No recommendations found for MovieID: {MovieID}", movieId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recommendations for MovieID {MovieID}", movieId);
            }
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Movie ID not found on TMDB: {Message}", ex.Message);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing metadata for file hash: {FileHash}", fileHash);
            throw;
        }
    }
  
    
    private async Task<string> WaitForHashCompletion(string filePath)
    {
        string computedHash = null;
        int retries = 120; // Allow up to 120 retries
        int delay = 5000; // 5 seconds delay between retries

        var fileName = Path.GetFileName(filePath);

        while (retries > 0)
        {
            // Create a new scope for each database operation
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<NostromoDbContext>();

                // Query the database for the computed hash
                var video = await dbContext.Videos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.FileName == fileName, Context.CancellationToken);

                if (video != null && !string.IsNullOrWhiteSpace(video.ED2K))
                {
                    computedHash = video.ED2K;
                    _logger.LogInformation("Hash found in database for {FilePath}: {Hash}", filePath, computedHash);
                    return computedHash;
                }
            }

            retries--;
            _logger.LogDebug("Hash not found for {FilePath}. Retrying in {Delay}ms...", filePath, delay);
            await Task.Delay(delay);
        }

        _logger.LogError("Failed to retrieve hash for {FilePath} after multiple retries.", filePath);

        return null;
    }
}*/