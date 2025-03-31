using System.Text.Json.Serialization;
using Nostromo.Models;

namespace Nostromo.Server.API.Models;

public class TvRecommendationResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
        
    [JsonPropertyName("adult")]
    public bool Adult { get; set; }
        
    [JsonPropertyName("backdrop_path")]
    public string backdropPath { get; set; }
    
    [JsonPropertyName("original_name")]
    public string OriginalName { get; set; }
        
    [JsonPropertyName("overview")]
    public string Overview { get; set; }
        
    [JsonPropertyName("poster_path")]
    public string PosterPath { get; set; }
        
    [JsonPropertyName("media_type")]
    public string MediaType { get; set; }
        
    [JsonPropertyName("genre_ids")]
    public List<GenreResponse> Genres { get; set; }
     
    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }
        
    [JsonPropertyName("first_air_date")]
    public string firstAirDate { get; set; }
        
    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }
        
    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }
}