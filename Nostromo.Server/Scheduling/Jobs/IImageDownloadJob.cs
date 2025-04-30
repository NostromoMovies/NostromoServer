using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Scheduling.Jobs
{
    public interface IImageDownloadJob : IJob
    {
        string? ParentName { get; set; }

        int ImageId { get; set; }
    }
}
