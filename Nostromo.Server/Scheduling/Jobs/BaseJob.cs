using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Nostromo.Server.Scheduling.Jobs
{
    public abstract class BaseJob : IJob
    {
        public ILogger? _logger;
        public abstract string Name { get; }
        public abstract string Type { get; }

        protected IJobExecutionContext Context { get; private set; }

        public async Task Execute(IJobExecutionContext context)
        {
            Context = context;
            try
            {
                await ProcessJob();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing job {JobName} of type {JobType}", Name, Type);
                throw;
            }
        }

        public abstract Task ProcessJob();
    }

    public abstract class BaseJob<T> : BaseJob
    {
        public override abstract Task<T> ProcessJob();
    }
}