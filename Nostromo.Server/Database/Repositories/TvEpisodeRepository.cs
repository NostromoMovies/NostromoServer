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
}