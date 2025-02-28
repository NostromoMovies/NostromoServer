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

        _logger.LogInformation("Scheduled DownloadMovieMetadataJob with Job ID: {JobId} for hash: {Hash}", metadataJobId, computedHash);

        return computedHash;
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
}