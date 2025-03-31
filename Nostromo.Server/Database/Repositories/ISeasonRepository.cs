using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.Database.Repositories;
public interface ISeasonRepository : IRepository<Season>
{
    Task<int?> GetSeasonIdAsync(int showId, int seasonNumber);
    
}