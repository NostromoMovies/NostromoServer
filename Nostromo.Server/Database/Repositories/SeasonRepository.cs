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
}