using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.API.Models
{
    public class TmdbImageCollection
    {
        public int Id { get; set; }
        public List<TmdbImage> Backdrops { get; set; } = new();
        public List<TmdbImage> Logos { get; set; } = new();
        public List<TmdbImage> Posters { get; set; } = new();
    }
}
