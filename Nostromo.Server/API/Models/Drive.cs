using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.API.Models
{
    public class Drive
    {
        public string? Path { get; set; }
        
        public int Sizes { get; set; }

        public bool IsAccessible { get; set; }


    }
}
