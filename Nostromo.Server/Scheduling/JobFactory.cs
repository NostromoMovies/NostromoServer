using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Scheduling.Jobs;
using Quartz;
using Quartz.Spi;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

public class JobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<JobFactory> _logger;

    public JobFactory(IServiceProvider serviceProvider, ILogger<JobFactory> logger, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        try
        {
            var jobType = bundle.JobDetail.JobType;
            var job = (IJob)_serviceProvider.GetRequiredService(jobType);

            if (job is BaseJob baseJob)
            {
                baseJob._logger = _loggerFactory.CreateLogger(bundle.JobDetail.Key.Name.Replace(".", "․"));
            }

            return job;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating job {JobType}", bundle.JobDetail.JobType);
            throw;
        }
    }

    public void ReturnJob(IJob job)
    {
        var disposable = job as IDisposable;
        disposable?.Dispose();
    }
}