using Microsoft.Extensions.DependencyInjection;
using Nostromo.Server.Database;

namespace Nostromo.Server.Repositories
{
    public static class RepositoryStartup
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseFactory>();

            return services;
        }
    }
}
