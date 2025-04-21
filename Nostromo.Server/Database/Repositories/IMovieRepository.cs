using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.Database.Repositories;
public interface IMovieRepository : IRepository<TMDBMovie>
{
    Task<IEnumerable<TMDBMovie>> SearchAsync(string searchTerm);
    Task<(bool exists, string path)> GetPosterPathAsync(int id);

    Task<IEnumerable<TMDBMovie>> SearchGenreAsync(List<int> genreIds);

    Task<IEnumerable<TMDBMovie>> SortMovieByRatings();
    
    
}