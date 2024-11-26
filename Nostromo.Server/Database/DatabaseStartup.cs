// DatabaseStartup.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database;

public static class DatabaseStartup
{
    public static IServiceCollection AddNostromoDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbDirectory = Path.Combine(Utils.ApplicationPath, "Database");
        var dbPath = Path.Combine(dbDirectory, "nostromo.db");

        // Ensure directory exists
        Directory.CreateDirectory(dbDirectory);

        // Add DbContext
        services.AddDbContext<NostromoDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Add Repository services
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;

    }
}
