using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

﻿using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database.Repositories;

public class TvRecommendationRepository : Repository<TvRecommendation>, ITvRecommendationRepository
{
    public TvRecommendationRepository(NostromoDbContext context) : base(context)
    {
    }

    public async Task<List<TvRecommendation>> GetTvRecommendationAsync(int showId)
    {
        return await _context.TvRecommendations
            .Where(t => t.ShowId == showId)
            .ToListAsync();
    }
    
}