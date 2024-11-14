using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Scheduling.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Nostromo.Server.Extensions;

public class NostromoJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NostromoJobFactory> _logger;

    public NostromoJobFactory(IServiceProvider serviceProvider, ILogger<NostromoJobFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        try
        {
            var jobType = bundle.JobDetail.JobType;
            var job = (IJob)_serviceProvider.GetRequiredService(jobType);

            // If it's a BaseJob, set up its logger
            if (job is BaseJob baseJob)
            {
                baseJob._logger = _serviceProvider.GetRequiredService<ILogger<HashFileJob>>();
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

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzServices(this IServiceCollection services)
    {
        // Add Quartz services
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        services.AddSingleton<IJobFactory, NostromoJobFactory>();

        // Register jobs
        services.AddTransient<HashFileJob>();

        return services;
    }
}