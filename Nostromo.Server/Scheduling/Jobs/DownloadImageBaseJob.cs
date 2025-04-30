using Microsoft.Extensions.Logging;
using Quartz;

namespace Nostromo.Server.Scheduling.Jobs
{
    public abstract class DownloadImageBaseJob : BaseJob, IImageDownloadJob
    {
        public string? ParentName { get; set; }

        public int ImageId { get; set; }

        public virtual string RemoteURL { get; set; }

        public virtual string RemotePath { get; set; }

        public virtual string SavePath { get; set; }


        public DownloadImageBaseJob(ILogger<DownloadImageBaseJob> logger)
        {
            _logger = logger;
        }
        public override string Name => "Download Image Job";
        public override async Task ProcessJob()
        {

        }
    }
}
