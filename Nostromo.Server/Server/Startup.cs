using Microsoft.Extensions.DependencyInjection;
using Nostromo.Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Server
{
    public class Startup
    {
        public Startup() { }

        public async Task Start()
        {
            try
            {
                var nostromoServer = Utils.ServiceContainer.GetRequiredService<NostromoServer>();
            }
            catch (Exception e)
            {
                //log exception
            }
        }
    }
}
