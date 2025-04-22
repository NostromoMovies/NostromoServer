using System.Text.Json.Serialization;
namespace Nostromo.Server.API.Models;

public class TmdbTvSeasonResponse {
    [JsonPropertyName("id")]
    public int SeasonID { get; set; }
    
    [JsonPropertyName("name")]
    public string seasonName { get; set; }
        
    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; set; }
        
    [JsonPropertyName("air_date")]
    public string Airdate { get; set; }
        
    [JsonPropertyName("episode_count")]
    public int EpisodeCount { get; set; }
      
    [JsonPropertyName("overview")]
    public string Overview { get; set; }
        
    [JsonPropertyName("poster_path")]
    public string PosterPath { get; set; }
       
    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }
    
}