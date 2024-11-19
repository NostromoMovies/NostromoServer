using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

public class MovieRepository : IMovieRepository
{
    private readonly NostromoDbContext _context;

    public MovieRepository(NostromoDbContext context)
    {
        _context = context;
    }

    public async Task<TMDBMovie> GetByIdAsync(int id)
    {
        return await _context.Movies
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m => m.MovieID == id);
    }

    public async Task<IEnumerable<TMDBMovie>> SearchAsync(string searchTerm)
    {
        return await _context.Movies
            .Include(m => m.Genres)
            .Where(m => m.Title.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task AddAsync(TMDBMovie movie)
    {
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TMDBMovie movie)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }
    }
}