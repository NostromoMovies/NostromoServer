using System.Text.Json.Serialization;
namespace Nostromo.Server.API.Models;

public class TmdbTvEpisodeResponse {
    [JsonPropertyName("id")]
    public int EpisodeID { get; set; }
        
    [JsonPropertyName("name")]
    public string EpisodeName { get; set; }
       
    [JsonPropertyName("air_date")]
    public string Airdate { get; set; }
        
    [JsonPropertyName("overview")]
    public string Overview { get; set; }
    
    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; set; }
        
    [JsonPropertyName("runtime")]
    public int Runtime { get; set; }
        
    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }
        
    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }
        
    [JsonPropertyName("still_path")]
    public string? StillPath { get; set; }
    
}