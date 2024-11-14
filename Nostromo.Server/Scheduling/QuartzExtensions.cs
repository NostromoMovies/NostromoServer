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

        // Register jobs
        services.AddTransient<HashFileJob>();

        return services;
    }
}