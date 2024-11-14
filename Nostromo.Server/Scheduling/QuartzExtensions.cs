using Microsoft.Extensions.DependencyInjection;
using Nostromo.Server.Scheduling;
using Quartz;
using Quartz.Impl;

namespace Nostromo.Server.Extensions;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzServices(this IServiceCollection services)
    {
        // Add Quartz services
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

        // Register jobs
        services.AddTransient<HashFileJob>();

        return services;
    }
}