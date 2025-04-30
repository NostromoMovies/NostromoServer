using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

﻿using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Utilities;

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

    public async Task<(bool exists, string path)> GetPosterPathAsync(int id)
    {
        var movie = await GetByIdAsync(id);
        if (movie == null)
            return (false, string.Empty);

        var imagePath = Path.Combine(Utils.ApplicationPath, $"posters/{id}_poster.jpg");

        return (File.Exists(imagePath), imagePath);
    }

    
    public async Task<IEnumerable<TMDBMovie>> SortMovieByRatings()
    {

        var movies = await _context.Movies
            .OrderByDescending(m => m.VoteAverage)
            .ToListAsync();


        return movies;


    }
    public async Task<IEnumerable<TMDBMovie>> SearchGenreAsync(List<int> genreIds)
    {
       

        return await _context.Movies
            .Include(m => m.Genres) 
            .Where(m => m.Genres != null && m.Genres.Any(g => genreIds.Contains(g.GenreID)))
            .ToListAsync(); 
    }

    public async Task<TMDBMovie> GetByTMDBIdAsync(int tmdbId)
    {
        return await Query()
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m => m.MovieID == tmdbId);
    }
}