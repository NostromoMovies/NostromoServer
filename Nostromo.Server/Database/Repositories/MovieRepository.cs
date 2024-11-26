using Microsoft.EntityFrameworkCore;

namespace Nostromo.Server.Database.Repositories;
public class MovieRepository : Repository<TMDBMovie>, IMovieRepository
{
    public MovieRepository(NostromoDbContext context) : base(context)
    {
    }

    public override async Task<TMDBMovie> GetByIdAsync(int id)
    {
        return await Query()
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m => m.MovieID == id);
    }

    public async Task<IEnumerable<TMDBMovie>> SearchAsync(string searchTerm)
    {
        return await Query()
            .Include(m => m.Genres)
            .Where(m => m.Title.Contains(searchTerm))
            .ToListAsync();
    }
}