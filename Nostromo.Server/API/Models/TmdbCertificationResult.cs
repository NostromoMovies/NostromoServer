using System.Collections.Generic; // List
using System.Text.Json.Serialization; // JsonPropertyName
namespace Nostromo.Models;

public class TmdbCertificationResult
{
    [JsonPropertyName("iso_3166_1")]
    public string Iso3166_1 { get; set; }

    [JsonPropertyName("release_dates")]
    public List<TmdbCertificationEntry> ReleaseDates { get; set; }
}