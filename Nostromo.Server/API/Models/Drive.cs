using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.API.Models
{
    public class Drive : Folder
    {
        public DriveType Type { get; set; }
    }
}
