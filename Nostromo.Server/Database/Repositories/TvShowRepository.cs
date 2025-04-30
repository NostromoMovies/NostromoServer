using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

﻿using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database.Repositories;

public class TvShowRepository : Repository<TvShow>, ITvShowRepository
{
    public TvShowRepository(NostromoDbContext context) : base(context)
    {
    }
    
    public async Task<(bool exists, string path)> GetPosterPathAsync(int id)
    {
        // First check if show exists
        var show = await GetByIdAsync(id);
        if (show == null)
            return (false, string.Empty);

        var imagePath = Path.Combine(Utils.ApplicationPath, $"posters/{id}_poster.jpg");

        return (File.Exists(imagePath), imagePath);
    }
    
}