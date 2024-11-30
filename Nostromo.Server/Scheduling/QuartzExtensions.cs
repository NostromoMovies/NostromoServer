using Microsoft.Extensions.DependencyInjection;
using Nostromo.Server.Scheduling.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Nostromo.Server.Scheduling;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzServices(this IServiceCollection services)
    {
        // Add Quartz services
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        services.AddSingleton<IJobFactory, JobFactory>();

        // Add IScheduler as singleton
        services.AddSingleton(provider =>
        {
            var factory = provider.GetRequiredService<ISchedulerFactory>();
            var scheduler = factory.GetScheduler().Result;
            scheduler.JobFactory = provider.GetRequiredService<IJobFactory>();
            return scheduler;
        });

        // Register jobs
        services.AddTransient<HashFileJob>();
        services.AddTransient<DownloadTmdbImageJob>();
        services.AddTransient<ProcessVideoJob>();
        services.AddTransient<DownloadMovieMetadataJob>();

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        return services;
    }
}