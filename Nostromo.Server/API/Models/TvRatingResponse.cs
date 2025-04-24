namespace Nostromo.Server.API.Models;
using System.Text.Json.Serialization;
public class TvRatingResponse
{
    [JsonPropertyName("iso_3166_1")]
    public string Iso3166_1 { get; set; } = string.Empty;

    [JsonPropertyName("rating")]
    public string Rating { get; set; } = string.Empty;
}