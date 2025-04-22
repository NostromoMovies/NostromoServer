using System;
using System.Collections.Generic; // List
using System.Text.Json.Serialization; // JsonPropertyName

namespace Nostromo.Models
{
    public class TmdbCertificationEntry
    {
        [JsonPropertyName("certification")]
        public string Certification { get; set; }

        [JsonPropertyName("release_date")]
        public DateTime ReleaseDate { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("iso_639_1")]
        public string Iso639_1 { get; set; }

        [JsonPropertyName("descriptors")]
        public List<string> Descriptors { get; set; }
    }
}