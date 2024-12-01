using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.API.Models
{
    public class FoldersAndFiles
    {
        public int Folders { get; set; }
        public int Files { get; set; }
    }

    public class Folder
    {
        public string Path { get; set; }

        [DefaultValue(false)]
        public bool IsAccessible { get; set; }

        public FoldersAndFiles Sizes { get; set; }
    }
}
