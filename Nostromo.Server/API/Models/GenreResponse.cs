using System.Collections.Generic; // List
namespace Nostromo.Models;

public class GenreResponse{
    public List<TmdbGenre> genres {get; set;} = new List<TmdbGenre>();
}