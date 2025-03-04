using Nostromo.Server.Services;
using Quartz;
using Quartz.Impl;
using System.Threading.Tasks;

public abstract class ChainableFileRenameJob : IJob
{
    private const string ChainJobClass = "ChainedJobClass";
    private const string ChainJobName = "ChainedJobName";
    private const string ChainJobGroup = "ChainedJobGroup";

    private readonly IFileRenamerService _fileRenamer;

    protected ChainableFileRenameJob(IFileRenamerService fileRenamer)
    {
        _fileRenamer = fileRenamer;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var success = await ExecuteFileOperation(context);

            if (success && context.JobDetail.JobDataMap.ContainsKey(ChainJobClass))
            {
                await Chain(context);
            }
        }
        catch (Exception ex)
        {
            //context.Scheduler.Context.GetLogger()?.LogError(ex, "Job execution failed");
            throw new JobExecutionException(ex);
        }
    }

    private async Task<bool> ExecuteFileOperation(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var originalFile = dataMap.GetString("OriginalFilePath");
        var newName = dataMap.GetString("NewFileName");

        return await _fileRenamer.RenameFile(originalFile, newName);
    }

    private async Task Chain(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var jobType = (Type)dataMap.Get(ChainJobClass);
        var jobName = dataMap.GetString(ChainJobName);
        var jobGroup = dataMap.GetString(ChainJobGroup);

        var nextJobData = new JobDataMap
        {
            ["OriginalFilePath"] = dataMap.GetString("NewFileName"), // Carry forward the new path
            ["NewFileName"] = dataMap.GetString("NextFileName")
        };

        var jobDetail = JobBuilder.Create(jobType)
            .WithIdentity(jobName, jobGroup)
            .UsingJobData(nextJobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobName}-trigger", $"{jobGroup}-triggers")
            .StartNow()
            .Build();

        await context.Scheduler.ScheduleJob(jobDetail, trigger);
    }

    protected void SetNextJob(IJobExecutionContext context, Type nextJobType,
                            string nextJobName, string nextJobGroup, string nextFileName)
    {
        var dataMap = context.JobDetail.JobDataMap;
        dataMap.Put(ChainJobClass, nextJobType);
        dataMap.Put(ChainJobName, nextJobName);
        dataMap.Put(ChainJobGroup, nextJobGroup);
        dataMap.Put("NextFileName", nextFileName);
    }
}