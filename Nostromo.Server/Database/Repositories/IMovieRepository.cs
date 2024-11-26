using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.Database.Repositories;
public interface IMovieRepository : IRepository<TMDBMovie>
{
    // Only need to declare methods unique to movies
    Task<IEnumerable<TMDBMovie>> SearchAsync(string searchTerm);
    // No need to redeclare GetByIdAsync, AddAsync etc - we get those from IRepository<TMDBMovie>
}