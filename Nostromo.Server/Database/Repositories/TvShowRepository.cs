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
}