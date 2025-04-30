using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Nostromo.Server.API.Controllers;
using Nostromo.Server.API.Middleware;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using System.Text.Json;
using Xunit;

namespace Nostromo.Tests.Controllers
{
    public class MoviesControllerTests
    {
        private readonly Mock<IMovieRepository> _mockMovieRepository;
        private readonly MoviesController _controller;
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<ILogger<ApiExceptionMiddleware>> _middlewareLogger;

        public MoviesControllerTests()
        {
            _mockMovieRepository = new Mock<IMovieRepository>();
            _middlewareLogger = new Mock<ILogger<ApiExceptionMiddleware>>();
            _controller = new MoviesController(_mockMovieRepository.Object);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(_mockMovieRepository.Object);
            _serviceProvider = services.BuildServiceProvider();
        }

        private async Task<(int StatusCode, JsonDocument Response)> GetResultDetails(Func<Task<IResult>> action)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };
            httpContext.Response.Body = new MemoryStream();

            var middleware = new ApiExceptionMiddleware(
                next: async (context) => await (await action()).ExecuteAsync(context),
                logger: _middlewareLogger.Object
            );

            await middleware.InvokeAsync(httpContext);

            httpContext.Response.Body.Position = 0;
            using var reader = new StreamReader(httpContext.Response.Body);
            var body = await reader.ReadToEndAsync();

            // Handle empty responses
            if (string.IsNullOrWhiteSpace(body))
                return (httpContext.Response.StatusCode, null);

            var response = JsonDocument.Parse(body);
            return (httpContext.Response.StatusCode, response);
        }

        [Fact]
        public async Task GetMovies_ReturnsMovieCollection()
        {
            // Arrange
            var expectedMovies = new List<TMDBMovie>
            {
                new TMDBMovie { MovieID = 1, Title = "Test Movie 1" },
                new TMDBMovie { MovieID = 2, Title = "Test Movie 2" }
            };

            _mockMovieRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(expectedMovies);

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.GetMovies());

            // Assert
            Assert.Equal(200, statusCode);
            Assert.NotNull(response);

            var rootElement = response.RootElement;
            Assert.True(rootElement.TryGetProperty("apiVersion", out var apiVersion));
            Assert.Equal("1.0", apiVersion.GetString());

            Assert.True(rootElement.TryGetProperty("data", out var data));
            Assert.Equal(2, data.GetProperty("totalItems").GetInt32());

            var items = data.GetProperty("items");
            Assert.Equal(2, items.GetArrayLength());
        }

        [Fact]
        public async Task GetMovies_WhenEmpty_ReturnsEmptyCollection()
        {
            // Arrange
            _mockMovieRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<TMDBMovie>());

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.GetMovies());

            // Assert
            Assert.Equal(200, statusCode);
            Assert.NotNull(response);

            var rootElement = response.RootElement;
            Assert.True(rootElement.TryGetProperty("apiVersion", out var apiVersion));
            Assert.Equal("1.0", apiVersion.GetString());

            Assert.True(rootElement.TryGetProperty("data", out var data));
            Assert.Equal(0, data.GetProperty("totalItems").GetInt32());
            Assert.Equal(0, data.GetProperty("items").GetArrayLength());
        }

        [Fact]
        public async Task GetPoster_WhenExists_ReturnsFile()
        {
            // Arrange
            int movieId = 1;
            string expectedPath = "/path/to/poster.jpg";
            _mockMovieRepository.Setup(repo => repo.GetPosterPathAsync(movieId))
                .ReturnsAsync((true, expectedPath));

            // Act
            var result = await _controller.GetPoster(movieId);

            // Assert
            var fileResult = Assert.IsType<PhysicalFileHttpResult>(result);
            Assert.Equal("image/jpeg", fileResult.ContentType);
            Assert.Equal(expectedPath, fileResult.FileName);
        }

        [Fact]
        public async Task GetPoster_WhenNotExists_ReturnsNotFound()
        {
            // Arrange
            int movieId = 1;
            _mockMovieRepository.Setup(repo => repo.GetPosterPathAsync(movieId))
                .ReturnsAsync((false, string.Empty));

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.GetPoster(movieId));

            // Assert
            Assert.Equal(404, statusCode);
            Assert.NotNull(response);

            var rootElement = response.RootElement;
            Assert.True(rootElement.TryGetProperty("apiVersion", out var apiVersion));
            Assert.Equal("1.0", apiVersion.GetString());

            Assert.True(rootElement.TryGetProperty("error", out var error));
            Assert.Equal(404, error.GetProperty("code").GetInt32());
            Assert.Equal("Poster not found", error.GetProperty("message").GetString());
        }
    }
}