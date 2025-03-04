using System.Collections.Generic; // List
using System.Text.Json.Serialization; // JsonPropertyName

namespace Nostromo.Server.API.Models
{
    public class TmdbRecommendationsResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<TmdbRecommendation> Results { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }
    }
}
