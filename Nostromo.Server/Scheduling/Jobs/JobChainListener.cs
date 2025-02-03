using Microsoft.Extensions.Logging;
using Quartz;

namespace Nostromo.Server.Scheduling.Jobs
{
    internal class JobChainListener : IJobListener
    {

        private readonly Dictionary<JobKey, JobKey> chainLinks = new();
        private readonly ILogger<JobChainListener> _logger;

        public string Name => "JobChainListener";

        public JobChainListener(ILogger<JobChainListener> logger)
        {
            _logger = logger;
        }

        public void AddJobChainLink(JobKey firstJob, JobKey secondJob)
        {
            chainLinks[firstJob] = secondJob;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
        {
            if (jobException != null) return;

            if (chainLinks.TryGetValue(context.JobDetail.Key, out JobKey nextJob))
            {
                try
                {
                    await context.Scheduler.TriggerJob(nextJob, cancellationToken);
                    _logger.LogInformation("Chained job {CurrentJob} to {NextJob}",
                        context.JobDetail.Key, nextJob);
                }
                catch (SchedulerException se)
                {
                    _logger.LogError(se, "Failed to chain to job {NextJob}", nextJob);
                }
            }
        }
    }
}
