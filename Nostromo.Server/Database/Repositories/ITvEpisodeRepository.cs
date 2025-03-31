using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.Database.Repositories;
public interface ITvEpisodeRepository : IRepository<Episode>
{
    //Task<TvShow> SearchAsync(string searchTerm);
    //Task<(bool exists, string path)> GetPosterPathAsync(int id);

    //Task<IEnumerable<TvShow>> SearchGenreAsync(List<int> genreIds);

    //Task<IEnumerable<TvShow>> SortMovieByRatings();
}