using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Nostromo.Models;
using Nostromo.Server.API.Controllers;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;
using System.Text.Json;
using Xunit;

namespace Nostromo.Tests.Controllers
{
    public class TmdbControllerTests
    {
        private readonly Mock<ITmdbService> _mockTmdbService;
        private readonly Mock<ILogger<TmdbController>> _mockLogger;
        private readonly TmdbController _controller;
        private readonly IServiceProvider _serviceProvider;

        public TmdbControllerTests()
        {
            _mockTmdbService = new Mock<ITmdbService>();
            _mockLogger = new Mock<ILogger<TmdbController>>();
            _controller = new TmdbController(_mockTmdbService.Object, _mockLogger.Object);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(_mockTmdbService.Object);
            _serviceProvider = services.BuildServiceProvider();
        }

        private async Task<(int StatusCode, string Body)> GetResultDetails(IResult result)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };
            httpContext.Response.Body = new MemoryStream();

            await result.ExecuteAsync(httpContext);

            httpContext.Response.Body.Position = 0;
            using var reader = new StreamReader(httpContext.Response.Body);
            var body = await reader.ReadToEndAsync();

            return (httpContext.Response.StatusCode, body);
        }

        #region GetMovieById Tests

        [Fact]
        public async Task GetMovieById_ValidId_ReturnsMovie()
        {
            // Arrange
            var movieId = 123;
            var expectedMovie = new TmdbMovieResponse
            {
                id = movieId,
                title = "Test Movie"
            };
            _mockTmdbService.Setup(x => x.GetMovieById(movieId))
                           .ReturnsAsync(expectedMovie);

            // Act
            var result = await _controller.GetMovieById(movieId);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(200, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(movieId, response.GetProperty("data").GetProperty("id").GetInt32());
            Assert.Equal("Test Movie", response.GetProperty("data").GetProperty("title").GetString());
        }

        [Fact]
        public async Task GetMovieById_MovieNotFound_ReturnsNotFound()
        {
            // Arrange
            var movieId = 999;
            _mockTmdbService.Setup(x => x.GetMovieById(movieId))
                           .ThrowsAsync(new NotFoundException($"Movie {movieId} not found"));

            // Act
            var result = await _controller.GetMovieById(movieId);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(404, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(404, response.GetProperty("error").GetProperty("code").GetInt32());
            Assert.Contains("not found", response.GetProperty("error").GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetMovieById_ServiceError_Returns500()
        {
            // Arrange
            var movieId = 123;
            _mockTmdbService.Setup(x => x.GetMovieById(movieId))
                           .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.GetMovieById(movieId);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(500, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(500, response.GetProperty("error").GetProperty("code").GetInt32());
            Assert.Equal("An error occurred", response.GetProperty("error").GetProperty("message").GetString());
        }

        #endregion

        #region GetMovieImagesById Tests

        [Fact]
        public async Task GetMovieImagesById_ValidId_ReturnsImages()
        {
            // Arrange
            var movieId = 123;
            var expectedImages = new TmdbImageCollection();
            _mockTmdbService.Setup(x => x.GetMovieImagesById(movieId))
                           .ReturnsAsync(expectedImages);

            // Act
            var result = await _controller.GetMovieImagesById(movieId);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(200, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.True(response.GetProperty("data").ValueKind == JsonValueKind.Object);
        }

        [Fact]
        public async Task GetMovieImagesById_ImageNotFound_ReturnsNotFound()
        {
            // Arrange
            var movieId = 999;
            _mockTmdbService.Setup(x => x.GetMovieImagesById(movieId))
                           .ThrowsAsync(new NotFoundException("Images not found"));

            // Act
            var result = await _controller.GetMovieImagesById(movieId);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(404, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(404, response.GetProperty("error").GetProperty("code").GetInt32());
        }

        #endregion

        #region GetMovieRuntime Tests

        [Fact]
        public async Task GetMovieRuntime_ValidId_ReturnsRuntime()
        {
            // Arrange
            var movieId = 123;
            var expectedRuntime = 120;
            _mockTmdbService.Setup(x => x.GetMovieRuntime(movieId))
                           .ReturnsAsync(expectedRuntime);

            // Act
            var result = await _controller.GetMovieRuntime(movieId);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(200, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(expectedRuntime, response.GetProperty("data").GetInt32());
        }

        [Fact]
        public async Task GetMovieRuntime_MovieNotFound_ReturnsNotFound()
        {
            // Arrange
            var movieId = 999;
            _mockTmdbService.Setup(x => x.GetMovieRuntime(movieId))
                           .ThrowsAsync(new NotFoundException("Movie not found"));

            // Act
            var result = await _controller.GetMovieRuntime(movieId);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(404, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(404, response.GetProperty("error").GetProperty("code").GetInt32());
        }

        #endregion

        #region SearchMovies Tests

        [Fact]
        public async Task SearchMovies_ValidQuery_ReturnsResults()
        {
            // Arrange
            var query = "test";
            var expectedResults = new List<TmdbMovieResponse>
            {
                new TmdbMovieResponse { title = "Test Movie" }
            };
            _mockTmdbService.Setup(x => x.SearchMovies(query))
                           .ReturnsAsync((expectedResults, expectedResults.Count));

            // Act
            var result = await _controller.SearchMovies(query);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(200, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(1, response.GetProperty("data").GetProperty("totalResults").GetInt32());
            Assert.True(response.GetProperty("data").GetProperty("results").GetArrayLength() > 0);
        }

        [Fact]
        public async Task SearchMovies_EmptyQuery_ReturnsBadRequest()
        {
            // Arrange
            string query = "";

            // Act
            var result = await _controller.SearchMovies(query);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(400, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(400, response.GetProperty("error").GetProperty("code").GetInt32());
            Assert.Contains("required", response.GetProperty("error").GetProperty("message").GetString());
        }

        [Fact]
        public async Task SearchMovies_NoResults_ReturnsEmptyArray()
        {
            // Arrange
            var query = "nonexistent";
            _mockTmdbService.Setup(x => x.SearchMovies(query))
                           .ThrowsAsync(new NotFoundException("No results found"));

            // Act
            var result = await _controller.SearchMovies(query);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(200, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(0, response.GetProperty("data").GetProperty("totalResults").GetInt32());
            Assert.Equal(0, response.GetProperty("data").GetProperty("results").GetArrayLength());
        }

        [Fact]
        public async Task SearchMovies_ServiceError_Returns500()
        {
            // Arrange
            var query = "test";
            _mockTmdbService.Setup(x => x.SearchMovies(query))
                           .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.SearchMovies(query);
            var (statusCode, body) = await GetResultDetails(result);

            // Assert
            Assert.Equal(500, statusCode);
            var response = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(500, response.GetProperty("error").GetProperty("code").GetInt32());
            Assert.Equal("An unexpected error occurred", response.GetProperty("error").GetProperty("message").GetString());
        }

        #endregion
    }
}