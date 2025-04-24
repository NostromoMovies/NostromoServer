using System.Text.Json.Serialization;
using FluentNHibernate.Testing.Values;

namespace Nostromo.Server.API.Models;

public class TvRecommendationListResponse
{
    [JsonPropertyName("results")]
    public List<TvRecommendationResponse>? Results { get; set; }
    
}