namespace Nostromo.Server.API.Models;


public class TmdbMovieRecommendationResponse
{
    public int Page { get; set; }
    public List<TmdbMovieRecommendation> Results { get; set; }
}