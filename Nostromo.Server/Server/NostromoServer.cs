using Nostromo.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Server
{
    public class NostromoServer


    {
        private readonly IFileWatcherService _fileWatcherService;
        private static DateTime? _startTime;

        public NostromoServer(IFileWatcherService fileWatcherService)
        {
            _fileWatcherService = fileWatcherService;
        }

        public bool StartUpServer()
        {
            // startup stuff
            return true;
        }
    }
}
