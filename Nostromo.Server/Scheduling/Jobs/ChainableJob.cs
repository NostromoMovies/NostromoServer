using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

public abstract class ChainableJob : IJob
{
    private const string ChainJobClass = "ChainedJobClass";
    private const string ChainJobName  = "ChainedJobName";
    private const string ChainJobGroup = "ChainedJobGroup";

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            // Execute actual job logic
            await DoExecute(context);

            // Check if we need to chain another job
            if (context.JobDetail.JobDataMap.ContainsKey(ChainJobClass))
            {
                await Chain(context);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions appropriately
            throw new JobExecutionException(ex);
        }
    }

    private async Task Chain(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;

        var jobType = (Type)dataMap.Get(ChainJobClass);
        var jobName = dataMap.GetString(ChainJobName);
        var jobGroup = dataMap.GetString(ChainJobGroup);

        // Remove chaining parameters to prevent infinite loops
        dataMap.Remove(ChainJobClass);
        dataMap.Remove(ChainJobName);
        dataMap.Remove(ChainJobGroup);

        // Create job detail
        var jobDetail = JobBuilder.Create(jobType)
            .WithIdentity(jobName, jobGroup)
            .UsingJobData(dataMap)
            .Build();

        // Create trigger that starts immediately
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobName}-trigger", $"{jobGroup}-triggers")
            .StartNow()
            .Build();

        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.ScheduleJob(jobDetail, trigger);
    }

    protected abstract Task DoExecute(IJobExecutionContext context);

    protected void ChainJob(IJobExecutionContext context, Type jobType,
                           string jobName, string jobGroup)
    {
        var dataMap = context.JobDetail.JobDataMap;
        dataMap.Put(ChainJobClass, jobType);
        dataMap.Put(ChainJobName, jobName);
        dataMap.Put(ChainJobGroup, jobGroup);
    }
}