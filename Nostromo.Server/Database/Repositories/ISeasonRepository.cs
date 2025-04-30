using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.Database.Repositories;
public interface ISeasonRepository : IRepository<Season>
{
    Task<int?> GetSeasonIdAsync(int showId, int seasonNumber);

    Task<List<Season>> GetSeasonByShowIdAsync(int showId);
    Task<(bool exists, string path)> GetPosterPathAsync(int id, int seasonNumber);
}