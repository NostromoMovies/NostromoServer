using System.Collections.Generic; // List
using System.Text.Json.Serialization; // JsonPropertyName
namespace Nostromo.Models;

public class TmdbCertificationResponse
{
    [JsonPropertyName("results")]
    public List<TmdbCertificationResult> Results { get; set; }
}