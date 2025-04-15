using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.Database.Repositories;
public interface ITvEpisodeRepository : IRepository<Episode>
{
    //Task<TvShow> SearchAsync(string searchTerm);
    
    Task<int?> GetEpisodeIdAsync(int showId, int seasonId, int episodeNumber);
    
    Task<List<Episode>> GetEpisodeBySeasonIdAsync(int seasonId);
    Task<(bool exists, string path)> GetPosterPathAsync(int id, int seasonNumber, int seasonId, int episodeNumber);
    //Task<IEnumerable<TvShow>> SearchGenreAsync(List<int> genreIds);

    //Task<IEnumerable<TvShow>> SortMovieByRatings();
}