using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Nostromo.Server.Database;
using Nostromo.Server.Services;

public class DataSeeder
{
    private readonly NostromoDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly TmdbService _tmdbService;

    public DataSeeder(NostromoDbContext context, HttpClient httpClient, TmdbService tmdbService)
    {
        _context = context;
        _httpClient = httpClient;
        _tmdbService = tmdbService;
    }

    public async Task SeedDataFromApiAsync()
    {
        // Fetch genre dictionary from the API
        var genreDictionary = await _tmdbService.GetGenreDictionary();

        if (genreDictionary == null || !genreDictionary.Any())
        {
            Console.WriteLine("No genres found or API call failed.");
            return;
        }

        // Convert Dictionary<int, string> to List<Genre>
        var genreList = genreDictionary.Select(kvp => new Genre
        {
            GenreID = kvp.Key,      
            Name = kvp.Value   
        }).ToList();

        // Add genres to the database context and save
        _context.Genres.AddRange(genreList);
        await _context.SaveChangesAsync();

        Console.WriteLine("Genres successfully seeded to the database.");
    }

}