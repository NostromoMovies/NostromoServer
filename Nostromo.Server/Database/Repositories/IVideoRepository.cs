namespace Nostromo.Server.Database.Repositories
{
    public interface IVideoRepository : IRepository<Video>
    {
        Task<IEnumerable<Video>> RecentlyAddedMoviesAsync();
    }
}
