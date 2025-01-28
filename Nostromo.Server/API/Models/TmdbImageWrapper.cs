using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.API.Models
{
    public class TmdbImageWrapper
    {
        public string ApiVersion { get; set; }
        public TmdbImageCollection Data { get; set; }
    }
}
