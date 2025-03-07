using System.Text.Json.Serialization;
namespace Nostromo.Server.API.Models;

public class TmdbTvResponse {
    [JsonPropertyName("adult")]
    public bool Adult{get; set;}
        
    [JsonPropertyName("genre_ids")]
    public List<int>? GenreIds{get; set;}
    
    [JsonPropertyName("id")]
    public int Id{get; set;}
    
    [JsonPropertyName("original_language")]
    public string? OriginalLanguage{get; set;}
    
    [JsonPropertyName("original_name")]
    public string? OriginalName{get; set;}
    
    [JsonPropertyName("overview")]
    public string? Overview{get; set;}
    
    [JsonPropertyName("popularity")]
    public string? Popularity{get; set;}
    
    [JsonPropertyName("poster_path")]
    public string? PosterPath{get; set;}
    
    [JsonPropertyName("first_air_date")]
    public string? FirstAirDate{get; set;}
    
    [JsonPropertyName("vote_average")]
    public double? VoteAverage{get; set;}
    
    [JsonPropertyName("vote_count")]
    public int? VoteCount{get; set;}
}
