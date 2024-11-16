using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nostromo.Models;

[ApiController]
[Route("api/[controller]")]
public class TmdbController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseService _databaseService;
    private readonly string _tmdbApiKey = "cbd64d95c4c66beed284bd12701769ec";

    public TmdbController(HttpClient httpClient, DatabaseService databaseService)
    {
        _httpClient = httpClient;
        _databaseService = databaseService;
    }

    // Method to fetch genre list and return as a dictionary
    private async Task<Dictionary<int, string>> GetGenreDictionary()
    {
        var genreUrl = $"https://api.themoviedb.org/3/genre/movie/list?api_key={_tmdbApiKey}";
        var genreResponse = await _httpClient.GetFromJsonAsync<GenreResponse>(genreUrl);

        var genreDict = new Dictionary<int, string>();
        if (genreResponse?.genres != null)
        {
            foreach (var genre in genreResponse.genres)
            {
                genreDict[genre.id] = genre.name;

                // Save each genre to the database
                _databaseService.InsertGenre(genre);
            }
        }

        return genreDict;
    }

    // Method to fetch movie details to get the runtime
    private async Task<int?> GetMovieRuntime(int movieId)
    {
        var movieDetailsUrl = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={_tmdbApiKey}";
        var movieDetails = await _httpClient.GetFromJsonAsync<TmdbMovie>(movieDetailsUrl);

        return movieDetails?.runtime;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Get(string query = "Inception")
    {
        // TMDB API endpoint for searching movies
        var tmdbUrl = $"https://api.themoviedb.org/3/search/movie?api_key={_tmdbApiKey}&query={query}";

        try
        {
            // Fetch the list of genres and save them to the database
            var genreDict = await GetGenreDictionary();

            // Send the request and get the response as TmdbResponse
            var response = await _httpClient.GetFromJsonAsync<TmdbResponse>(tmdbUrl);

            if (response != null && response.results.Count > 0)
            {
                foreach (var movie in response.results)
                {
                    // Fetch runtime for each movie using its ID
                    movie.runtime = await GetMovieRuntime(movie.id);

                    // Map genre IDs to genre names
                    var genres = movie.genreIds.Select(id => genreDict.ContainsKey(id) ? genreDict[id] : "Unknown").ToList();

                    // Log the movie information to the console
                    Console.WriteLine($"Title: {movie.title}, Genres: {string.Join(", ", genres)}, Runtime: {movie.runtime} minutes, Release Date: {movie.releaseDate}");

                    // Save the movie to the database
                    _databaseService.InsertMovie(movie);
                }
            }
            else
            {
                Console.WriteLine("No results found.");
            }

            // Return the response as JSON
            return Ok(response);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
            return StatusCode(500, "Error contacting TMDB API.");
        }
    }
}
