using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

﻿using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database.Repositories;

public class SeasonRepository : Repository<Season>, ISeasonRepository
{
    public SeasonRepository(NostromoDbContext context) : base(context)
    {
    }
    
    public async Task<int?> GetSeasonIdAsync(int showId, int seasonNumber)
    {
        return await _context.Seasons
            .Where(s=> s.TvShowID == showId && s.SeasonNumber == seasonNumber)
            .Select(s => (int?)s.SeasonID)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Season>> GetSeasonByShowIdAsync(int showId)
    {
        return await _context.Seasons
            .Where(s => s.TvShowID == showId)
            .ToListAsync();
    }
    public async Task<(bool exists, string path)> GetPosterPathAsync(int id, int seasonNumber)
    {
        // First check if show exists
        var season = await GetSeasonIdAsync(id, seasonNumber);
        if (season == null)
            return (false, string.Empty);

        var imagePath = Path.Combine(Utils.ApplicationPath, $"posters/{id}_season_{seasonNumber}_poster.jpg");

        return (File.Exists(imagePath), imagePath);
    }
}