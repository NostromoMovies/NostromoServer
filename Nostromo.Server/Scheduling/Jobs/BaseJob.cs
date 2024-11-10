using Microsoft.Extensions.Logging;
using Nostromo.Server.Scheduling.Jobs;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Scheduling.Jobs;

public abstract class BaseJob : IJob
{ 
    public ILogger _logger;
    public abstract string Name { get; }
    public abstract string Type { get; }
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await ProcessJob();
        }
        catch (Exception ex)
        {
            //log exception or whatever
        }
    }

    public abstract Task ProcessJob();
}

public abstract class BaseJob<T> : BaseJob
{
    public override abstract Task<T> ProcessJob();
}