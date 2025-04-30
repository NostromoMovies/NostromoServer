using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Utilities;
using System.IO;
using System.Net.Http.Headers;
using Nostromo.Server.Services;
using Nostromo.Server.Settings;

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
       
        
        services.AddScoped<IVideoRepository, VideoRepository>();
        
        
        /*
        services.AddHttpClient<TmdbService>(client =>
        {
            // You can configure the HttpClient here, if needed
            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        // Register TmdbService with its dependencies
        services.AddScoped<ITmdbService, TmdbService>();

        // Register TmdbSettings using IOptions
        services.Configure<TmdbSettings>(configuration.GetSection("TmdbSettings"));
        */
        
    


        return services;
    }
}