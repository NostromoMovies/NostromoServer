using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Utilities;
using System.IO;

namespace Nostromo.Server.Database;

public static class DatabaseStartup
{
    public static IServiceCollection AddNostromoDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get the connection string directly
        var dbDirectory = Path.Combine(Utils.ApplicationPath, "Database");
        var dbPath = Path.Combine(dbDirectory, "nostromo.db");
        Directory.CreateDirectory(dbDirectory);

        var connectionString = $"Data Source={dbPath}";

        // Register the DbContext with the connection string
        services.AddDbContext<NostromoDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        // Add Repository services
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
        services.AddScoped<IImportFolderRepository, ImportFolderRepository>();
        services.AddScoped<IVideoPlaceRepository, VideoPlaceRepository>();


        return services;
    }
}