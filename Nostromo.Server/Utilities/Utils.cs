using Nostromo.Server.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Utilities
{
    public static class Utils
    {
        public static NostromoServer NostromoServer { get; set; }
        public static IServiceProvider ServiceContainer { get; set; }

        //settings provider

        private static string _applicationPath = null;

        //public static string ApplicationPath
    }
}
