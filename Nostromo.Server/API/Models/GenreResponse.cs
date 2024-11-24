using System.Collections.Generic;
namespace Nostromo.Server.API.Models
{
    public class GenreResponse
    {
        public List<TmdbGenre> genres { get; set; } = new List<TmdbGenre>();
    }
}