namespace Nostromo.Server.Database.Repositories;

using Microsoft.EntityFrameworkCore;

public class VideoRepository : Repository<Video>, IVideoRepository
{
    public VideoRepository(NostromoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Video>> RecentlyAddedMoviesAsync()
    {
        return await _context.Videos
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(); 
    }
}