using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Settings;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using System.Net;
using System.Text.Json;
using Nostromo.Server.API.Enums;

//using TvRecommendation = Nostromo.Server.API.Models.TvRecommendation;

namespace Nostromo.Server.Services
{

    public interface ITmdbService
    {
        Task<TmdbMovieResponse> GetMovieById(int id);
        Task<TmdbImageCollection> GetMediaImagesById(string mediaType, int id, int? seasonNumber = null, int? episodeNumber = null);
        Task<int?> GetMovieRuntime(int movieId);
        Task<(IEnumerable<TmdbMovieResponse> Results, int TotalResults)> SearchMovies(string query);
        Task<Dictionary<int, string>> GetGenreDictionary();
        Task<TmdbRecommendationsResponse> GetRecommendation(int movieId);
        Task<TvRecommendationListResponse> GetTvRecommendation(int showId);
        Task<int?> GetKeywordId(string keyword);
        Task<(IEnumerable<TmdbMovieResponse> Results, int TotalResults)> SearchMoviesByKeyword(string keyword);
        Task<TmdbCreditsWrapper> GetMovieCreditsAsync(int movieId);
        Task<GenreResponse> GetGenresForMovie(int movieId);
        Task<TvEpisodeCreditWrapper> GetTvEpisodeCreditsAsync(int showId, int seasonNumber, int episodeNumber);
        Task<TvShowCreditWrapper> GetTvShowCreditsAsync(int showId);
        Task<TmdbTvResponse> GetTvShowById(int showId);
        Task<TmdbTvEpisodeResponse> GetTvEpisodeById(int showId, int seasonNumber, int episodeNumber, int seasonID);
        Task<string?> GetCertificationAsync(int movieId);
        Task<string?> GetTvCertificationAsync(int showId);
    }

    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService; //TODO: remove
        private readonly IMovieRepository _movieRepository;
        private readonly ILogger<TmdbService> _logger;
        private readonly string _tmdbApiKey;
        private readonly IOptions<TmdbSettings> _settings;
        private readonly ITvShowRepository _tvShowRepository;
        private readonly ITvEpisodeRepository _tvEpisodeRepository;

        public TmdbService(
            HttpClient httpClient,
            IDatabaseService databaseService,
            IOptions<ServerSettings> serverSettings,
            ILogger<TmdbService> logger,
            IMovieRepository movieRepository,
            IOptions<TmdbSettings> settings,
            ITvShowRepository tvShowRepository,
            ITvEpisodeRepository tvEpisodeRepository)
        {
            _httpClient = httpClient;
            _databaseService = databaseService;
            _movieRepository = movieRepository;
            _logger = logger;
            _tmdbApiKey = serverSettings.Value.TmdbApiKey
                ?? throw new ArgumentNullException(nameof(serverSettings), "TMDB API key not configured");

            _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _settings = settings;
            _tvShowRepository = tvShowRepository;
            _tvEpisodeRepository = tvEpisodeRepository;
        }

        public async Task<Dictionary<int, string>> GetGenreDictionary()
        {
            try
            {
                var genreUrl = $"genre/movie/list?api_key={_tmdbApiKey}";
                var genreResponse = await _httpClient.GetFromJsonAsync<GenreResponse>(genreUrl)
                    .ConfigureAwait(false)
                    ?? throw new NotFoundException("Genre list not found");
                
                var tvGenreUrl = $"genre/tv/list?api_key={_tmdbApiKey}";
                var tvGenreResponse = await _httpClient.GetFromJsonAsync<GenreResponse>(tvGenreUrl)
                                        .ConfigureAwait(false)
                                    ?? throw new NotFoundException("Tv Genre list not found");
                
                var genreDict = new Dictionary<int, string>();

                var combinedGenres = genreResponse.genres.Concat(tvGenreResponse.genres)
                    .GroupBy(g => g.id)
                    .Select(g => g.First());
                
                if (genreResponse.genres != null)
                {
                    foreach (var genre in combinedGenres)
                    {
                        genreDict[genre.id] = genre.name;
                        await _databaseService.InsertGenreAsync(genre);
                    }
                    _logger.LogInformation("Successfully fetched and stored {Count} genres", genreResponse.genres.Count);
                }
                return genreDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching genre dictionary");
                throw;
            }
        }

        public async Task<TmdbMovieResponse> GetMovieById(int id)
        {
            try
            {
                var movieUrl = $"movie/{id}?api_key={_tmdbApiKey}";
                var movie = await _httpClient.GetFromJsonAsync<TmdbMovieResponse>(movieUrl)
                    ?? throw new NotFoundException($"Movie with ID {id} not found");

                var newMovie = new TMDBMovie(movie);

                await _movieRepository.AddAsync(newMovie);

                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie details for ID: {MovieId}", id);
                throw;
            }
        }

        public async Task<TmdbImageCollection> GetMediaImagesById(string mediaTypeString, int id, int? seasonNumber = null, int? episodeNumber = null)
        {
            try
            {
                MediaTypes mediaType = (MediaTypes)Enum.Parse(typeof(MediaTypes), mediaTypeString, true);
                //var imageUrl =  $"movie/{id}/images?api_key={_tmdbApiKey}";
                _logger.LogInformation("MediaType is {mediaType}");
                var imageUrl = mediaType switch
                {
                    MediaTypes.Movie => $"movie/{id}/images?api_key={_tmdbApiKey}",
                    MediaTypes.Tv => $"tv/{id}/images?api_key={_tmdbApiKey}",
                    MediaTypes.Season => $"tv/{id}/season/{seasonNumber}/images?api_key={_tmdbApiKey}",
                    MediaTypes.Episode => $"tv/{id}/season/{seasonNumber}/episode/{episodeNumber}/images?api_key={_tmdbApiKey}",
                    _ => throw new ArgumentException($"Unknown media type: {mediaTypeString}"),
                };
                
                var images = await _httpClient.GetFromJsonAsync<TmdbImageCollection>(imageUrl)
                    ?? throw new NotFoundException($"Images for movie with ID {id} not found");
                return images;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException($"Movie with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching images for movie with ID: {MovieId}", id);
                throw;
            }
        }

        public async Task<int?> GetMovieRuntime(int movieId)
        {
            try
            {
                var movie = await GetMovieById(movieId);
                return movie.runtime;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching runtime for movie {MovieId}", movieId);
                throw;
            }
        }

        // weird
        public async Task<(IEnumerable<TmdbMovieResponse> Results, int TotalResults)> SearchMovies(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    throw new ArgumentException("Search query cannot be empty", nameof(query));
                }

                var tmdbUrl = $"search/movie?api_key={_tmdbApiKey}&query={Uri.EscapeDataString(query)}";
                var response = await _httpClient.GetFromJsonAsync<TmdbResponse>(tmdbUrl)
                    ?? throw new NotFoundException("No search results found");

                if (response.results == null || !response.results.Any())
                {
                    return (Array.Empty<TmdbMovieResponse>(), 0);
                }

                // var genreDict = await GetGenreDictionary();

                // apparently not thread-safe
                // Process each movie
                //foreach (var movieResponse in response.results)
                //{
                //    try
                //    {                                     // why?
                //        movieResponse.runtime = await GetMovieRuntime(movieResponse.id);
                //        TMDBMovie movie = new TMDBMovie(movieResponse);
                //        await _movieRepository.AddAsync(movie);

                //        var genres = movieResponse.genreIds
                //            .Select(id => genreDict.ContainsKey(id) ? genreDict[id] : "Unknown")
                //            .ToList();

                //        _logger.LogDebug(
                //            "Processed movie: {Title}, Genres: {Genres}, Runtime: {Runtime}",
                //            movieResponse.title,
                //            string.Join(", ", genres),
                //            movieResponse.runtime);
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, "Error processing movie {Title} (ID: {Id})", movieResponse.title, movieResponse.id);
                //        // Continue processing other movies
                //    }
                //}

                return (response.results, response.results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with query: {Query}", query);
                throw;
            }
        }

        public async Task<TmdbRecommendationsResponse> GetRecommendation(int movieId)
        {
            try
            {
                string tmdbUrl = $"movie/{movieId}/recommendations?api_key={_tmdbApiKey}";

                HttpResponseMessage response = await _httpClient.GetAsync(tmdbUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("TMDb API request failed with status code: {StatusCode}", response.StatusCode);
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var recommendations = JsonSerializer.Deserialize<TmdbRecommendationsResponse>(
                    jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (recommendations?.Results == null || !recommendations.Results.Any())
                {
                    _logger.LogWarning("No recommendations found for movie ID: {MovieId}", movieId);
                    return recommendations;
                }

                foreach (var recommendation in recommendations.Results)
                {
                    await _databaseService.StoreTmdbRecommendationsAsync(movieId, recommendation);
                }

                _logger.LogInformation("Fetched and stored {Count} recommendations for movie ID {MovieId}",
                    recommendations.Results.Count, movieId);

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching or storing movie recommendations for movie ID: {MovieId}", movieId);
                throw;
            }
        }

        public async Task<TvRecommendationListResponse> GetTvRecommendation(int showId)
        { 
            try
            {
                string tmdbUrl = $"tv/{showId}/recommendations?api_key={_tmdbApiKey}";

                HttpResponseMessage response = await _httpClient.GetAsync(tmdbUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("TMDb API request failed with status code: {StatusCode}", response.StatusCode);
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var recommendations = JsonSerializer.Deserialize<TvRecommendationListResponse>(
                    jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                

                _logger.LogInformation("Fetched  {Count} recommendations for Tv Show ID {ShowId}",
                    recommendations.Results.Count, showId);

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tv recommendations for movie ID: {ShowId}", showId);
                throw;
            }
        }

        public async Task<int?> GetKeywordId(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    throw new ArgumentException("Keyword cannot be empty", nameof(keyword));

                var keywordUrl = $"search/keyword?api_key={_tmdbApiKey}&query={Uri.EscapeDataString(keyword)}";
                var response = await _httpClient.GetFromJsonAsync<TmdbKeywordResponse>(keywordUrl);

                if (response?.results == null || response.results.Count == 0)
                {
                    _logger.LogWarning("No keyword found for query: {Keyword}", keyword);
                    return null;
                }

                return response.results.First().id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching keyword ID for: {Keyword}", keyword);
                throw;
            }
        }


        public async Task<(IEnumerable<TmdbMovieResponse> Results, int TotalResults)> SearchMoviesByKeyword(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    throw new ArgumentException("Keyword cannot be empty", nameof(keyword));

                var keywordId = await GetKeywordId(keyword);
                if (keywordId == null)
                    return (Array.Empty<TmdbMovieResponse>(), 0);

                var tmdbUrl = $"discover/movie?api_key={_tmdbApiKey}&with_keywords={keywordId}";
                var response = await _httpClient.GetFromJsonAsync<TmdbResponse>(tmdbUrl)
                    ?? throw new NotFoundException("No movies found for keyword");

                var movieResults = response.results ?? new List<TmdbMovieResponse>();
                return (movieResults, movieResults.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movies for keyword: {Keyword}", keyword);
                throw;
            }
        }

        public async Task<TmdbCreditsWrapper> GetMovieCreditsAsync(int movieId)
        {
            string url = $"movie/{movieId}/credits?api_key={_tmdbApiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TmdbCreditsWrapper>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<GenreResponse> GetGenresForMovie(int movieId)
        {
            string url = $"movie/{movieId}?api_key={_tmdbApiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();

            var movieDetails = JsonSerializer.Deserialize<TmdbMovieResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (movieDetails?.genreIds == null)
                return new GenreResponse { genres = new List<TmdbGenre>() };

            return new GenreResponse
            {
                genres = movieDetails.genreIds
                    .Select(g => new TmdbGenre { id = g.id, name = g.name })
                    .ToList()
            };
        }


        public async Task<TvEpisodeCreditWrapper> GetTvEpisodeCreditsAsync(int showId, int seasonNumber, int episodeNumber)
        {
            string url = $"tv/{showId}/season/{seasonNumber}/episode/{episodeNumber}/credits?api_key={_tmdbApiKey}";
            var credits = await _httpClient.GetFromJsonAsync<TvEpisodeCreditWrapper>(url);

            return credits;
        }

        public async Task<TvShowCreditWrapper> GetTvShowCreditsAsync(int showId)
        {
            string url = $"tv/{showId}/credits?api_key={_tmdbApiKey}";
            var credits = await _httpClient.GetFromJsonAsync<TvShowCreditWrapper>(url);
            return credits;
        }
        public async Task<TmdbTvResponse> GetTvShowById(int showId)
        {
            try
            {
                var existingShow = await _tvShowRepository.GetByIdAsync(showId);

                if (existingShow == null)
                {
                    var showUrl = $"tv/{showId}?api_key={_tmdbApiKey}";
                    var show = await _httpClient.GetFromJsonAsync<TmdbTvResponse>(showUrl)
                               ?? throw new NotFoundException($"Tv Show with ID {showId} not found");

                    var newShow = new TvShow(show);

                    await _tvShowRepository.AddAsync(newShow);

                    return show;
                }
                else
                {
                    var response = new TmdbTvResponse
                    {
                        Adult = existingShow.Adult,
                        BackdropPath = existingShow.BackdropPath,
                        FirstAirDate = existingShow.FirstAirDate?.ToString("MM/dd/yyyy"),
                        Id = existingShow.TvShowID,
                        OriginalLanguage = existingShow.OriginalLanguage,
                        Overview = existingShow.Overview,
                        Popularity = existingShow.Popularity,
                        PosterPath = existingShow.PosterPath,
                        OriginalName = existingShow.OriginalName
                    };
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tv show details for ID: {ShowId}", showId);
                throw;
            }
        }
        
        public async Task<TmdbTvEpisodeResponse> GetTvEpisodeById(int showId, int seasonNumber, int episodeNumber, int seasonID)
        {
            try
            {
                var episodeUrl = $"tv/{showId}/season/{seasonNumber}/episode/{episodeNumber}?api_key={_tmdbApiKey}";
                var episode = await _httpClient.GetFromJsonAsync<TmdbTvEpisodeResponse>(episodeUrl)
                           ?? throw new NotFoundException($"Tv Show with ID {showId} not found");

                var newEpisode = new Episode(episode, seasonID, episodeNumber);

                await _tvEpisodeRepository.AddAsync(newEpisode);

                return episode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tv show details for ID: {ShowId}", showId);
                throw;
            }
        }

        public async Task<string?> GetCertificationAsync(int movieId)
        {
            var url = $"movie/{movieId}/release_dates?api_key={_tmdbApiKey}";
            var response = await _httpClient.GetFromJsonAsync<TmdbCertificationResponse>(url);

            var usRelease = response?.Results.FirstOrDefault(r => r.Iso3166_1 == "US");
            var certification = usRelease?.ReleaseDates?
                .Where(r => !string.IsNullOrWhiteSpace(r.Certification))
                .OrderByDescending(r => r.ReleaseDate)
                .FirstOrDefault()?.Certification;

            return string.IsNullOrWhiteSpace(certification) ? null : certification;
        }
        
        public async Task<string?> GetTvCertificationAsync(int showId)
        {
            var url = $"tv/{showId}/content_ratings?api_key={_tmdbApiKey}";
            var response = await _httpClient.GetFromJsonAsync<TvRatingListResponse>(url);

            if (response.Results == null || response == null)
            {
                return null;
            }
            var usRelease = response?.Results.FirstOrDefault(r => r.Iso3166_1 == "US");
            var certification = usRelease?.Rating;

            return string.IsNullOrWhiteSpace(certification) ? null : certification;
        }
    }

    public class NotFoundException(string message) : Exception(message)
    {

    }
}
