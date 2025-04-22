using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

﻿using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database.Repositories;

public class TvEpisodeRepository : Repository<Episode>, ITvEpisodeRepository
{
    public TvEpisodeRepository(NostromoDbContext context) : base(context)
    {
    }

    public async Task<int?> GetEpisodeIdAsync(int showId, int seasonId, int episodeNumber)
    {
        var episodeId = await _context.Episodes
            .Where(s=>s.SeasonID == seasonId && s.EpisodeNumber == episodeNumber)
            .Select(s=>(int?)s.EpisodeID)
            .FirstOrDefaultAsync();
        
        return episodeId;
        
    }

    public async Task<List<Episode>> GetEpisodeBySeasonIdAsync(int seasonId)
    {
        return await _context.Episodes
            .Where(s => s.SeasonID == seasonId)
            .ToListAsync();
    }
    public async Task<(bool exists, string path)> GetPosterPathAsync(int id, int seasonNumber, int seasonId, int episodeNumber)
    {
        // First check if movie exists
        var episodeId= GetEpisodeIdAsync(id, seasonId, episodeNumber);
        if (episodeId == null)
            return (false, string.Empty);

        var imagePath = Path.Combine(Utils.ApplicationPath, $"posters/{id}_season_{seasonNumber}_episode_{episodeNumber}_poster.jpg");

        return (File.Exists(imagePath), imagePath);
    }
}