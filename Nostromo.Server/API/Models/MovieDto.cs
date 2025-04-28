using System.Collections.Generic;

namespace Nostromo.Models
{
    public record MovieDto(
        int TmdbMovieId,
        string Title,
        string Overview,
        string ReleaseDate,
        IEnumerable<string> Genres,
        string? PosterPath,
        string? BackdropPath,
        int? Runtime);
}