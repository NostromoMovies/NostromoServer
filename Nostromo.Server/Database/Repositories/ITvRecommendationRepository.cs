using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.Database.Repositories;
public interface ITvRecommendationRepository : IRepository<TvRecommendation>
{
    Task<List<TvRecommendation>> GetTvRecommendationAsync(int showId);
    
}