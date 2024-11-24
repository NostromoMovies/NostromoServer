using Microsoft.Extensions.Logging;
using Quartz;
using System.Collections.Generic;
using System.Linq;
using Nostromo.Server.Scheduling.Jobs;

namespace Nostromo.Server.Scheduling
{
    public class ConsolidateJob : BaseJob
    {
        private readonly ILogger<ConsolidateJob> _logger;

        public ConsolidateJob(ILogger<ConsolidateJob> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override string Name => "Consolidate Job";
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

            _logger.LogInformation("Starting ConsolidateJob for file: {FilePath}", filePath);

            var jobs = GetJobs(filePath);
            foreach (var (jobDetail, trigger) in jobs)
            {
                await Context.Scheduler.ScheduleJob(jobDetail, trigger);
                _logger.LogInformation("Scheduled job: {JobKey}", jobDetail.Key);
            }

            _logger.LogInformation("Consolidate job workflow for file {FilePath} is complete.", filePath);
        }

        private List<(IJobDetail JobDetail, ITrigger Trigger)> GetJobs(string filePath)
        {
            var jobs = new List<(IJobDetail, ITrigger)>();

            var hashJob = JobBuilder.Create<HashFileJob>()
                .UsingJobData(HashFileJob.FILE_PATH_KEY, filePath)
                .WithIdentity(new JobKey($"HashJob_{filePath}", "ConsolidateGroup"))
                .Build();

            var hashTrigger = TriggerBuilder.Create()
                .StartNow()
                .WithIdentity(new TriggerKey($"HashTrigger_{filePath}", "ConsolidateGroup"))
                .Build();

            jobs.Add((hashJob, hashTrigger));

            return jobs;
        }
    }
}
