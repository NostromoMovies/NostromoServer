using System.Collections.Generic; // List
namespace Nostromo.Server.API.Models
{
    public class TmdbResponse
    {
        public List<TmdbMovie> results { get; set; } = new List<TmdbMovie>();
    }
}