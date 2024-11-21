using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.API.Models
{
    public class TmdbImage
    {
        public double AspectRatio { get; set; }
        public int Height { get; set; }
        public string? Iso_639_1 { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public double VoteAverage { get; set; }
        public int VoteCount { get; set; }
        public int Width { get; set; }
    }
}
