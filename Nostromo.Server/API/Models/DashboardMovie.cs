using System.Collections.Generic; // List
using System.Text.Json.Serialization; // JsonPropertyName
namespace Nostromo.Models;

public class MovieDashBoard
{
    [JsonPropertyName("poster_path")]
    public string posterPath { get; set; }
    [JsonPropertyName("genre_ids")]
    public List<int> genreIds { get; set; }
    [JsonPropertyName("id")]
    public int id { get; set; }
    [JsonPropertyName("original_title")]
    public string originalTitle { get; set; }

}