// IMovieRepository.cs

namespace Nostromo.Server.Database.Repositories;

public interface IMovieRepository
{
    Task<TMDBMovie> GetByIdAsync(int id);
    Task<IEnumerable<TMDBMovie>> SearchAsync(string searchTerm);
    Task AddAsync(TMDBMovie movie);
    Task UpdateAsync(TMDBMovie movie);
    Task DeleteAsync(int id);
}