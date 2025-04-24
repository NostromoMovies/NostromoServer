namespace Nostromo.Server.API.Models;
using System.Text.Json.Serialization;

public class TvRatingListResponse
{
    [JsonPropertyName("results")]
    public List<TvRatingResponse>  Results { get; set; }
}