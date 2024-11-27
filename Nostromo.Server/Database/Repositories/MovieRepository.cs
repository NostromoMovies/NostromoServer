using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database.Repositories;
public class MovieRepository : Repository<TMDBMovie>, IMovieRepository
{
    public MovieRepository(NostromoDbContext context) : base(context)
    {
    }

    //override makes sense here
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

    public async Task<(bool exists, string path)> GetPosterPathAsync(int id)
    {
        // First check if movie exists
        var movie = await GetByIdAsync(id);
        if (movie == null)
            return (false, string.Empty);

        var imagePath = Path.Combine(Utils.ApplicationPath, $"posters/{id}_poster.jpg");

        return (File.Exists(imagePath), imagePath);
    }
}