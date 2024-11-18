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
        private readonly FileWatcherService _fileWatcherService;
        private static DateTime? _startTime;

        public NostromoServer(FileWatcherService fileWatcherService)
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
