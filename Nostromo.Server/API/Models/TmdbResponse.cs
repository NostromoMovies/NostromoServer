using System.Collections.Generic; // List
namespace Nostromo.Models;

public class TmdbResponse{
    public List<TmdbMovieResponse> results {get; set;} = new List<TmdbMovieResponse>();
}