using System.Collections.Generic; // List
using System.Text.Json.Serialization; // JsonPropertyName

namespace Nostromo.Server.API.Models
{
    public class TmdbRecommendation
    {
        [JsonPropertyName("poster_path")]
        public string posterPath { get; set; }
        [JsonPropertyName("adult")]
        public bool adult { get; set; }
        [JsonPropertyName("overview")]
        public string overview { get; set; }
        [JsonPropertyName("release_date")]
        public string releaseDate { get; set; }

        [JsonPropertyName("original_language")]
        public string OriginalLanguage { get; set; }

        [JsonPropertyName("genre_ids")]
        public List<int> genreIds { get; set; }
        [JsonPropertyName("id")]
        public int id { get; set; }
        [JsonPropertyName("original_title")]
        public string originalTitle { get; set; }
        [JsonPropertyName("title")]
        public string title { get; set; }
        [JsonPropertyName("backdrop_path")]
        public string backdropPath { get; set; }
        [JsonPropertyName("popularity")]
        public float popularity { get; set; }
        [JsonPropertyName("vote_count")]
        public int voteCount { get; set; }
        [JsonPropertyName("video")]
        public bool video { get; set; }
        [JsonPropertyName("vote_average")]
        public float voteAverage { get; set; }
        // runtime not available through json
        public int? runtime { get; set; }
        public int TMDBMovieID { get; set; }
    }
}