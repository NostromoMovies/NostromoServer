using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Nostromo.Models;
using Nostromo.Server.API.Controllers;
using Nostromo.Server.API.Middleware;
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
        private readonly Mock<ILogger<ApiExceptionMiddleware>> _middlewareLogger;
        private readonly TmdbController _controller;
        private readonly IServiceProvider _serviceProvider;

        public TmdbControllerTests()
        {
            _mockTmdbService = new Mock<ITmdbService>();
            _mockLogger = new Mock<ILogger<TmdbController>>();
            _middlewareLogger = new Mock<ILogger<ApiExceptionMiddleware>>();
            _controller = new TmdbController(_mockTmdbService.Object, _mockLogger.Object);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(_mockTmdbService.Object);
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
            var response = JsonDocument.Parse(body);

            return (httpContext.Response.StatusCode, response);
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
            var (statusCode, response) = await GetResultDetails(() => _controller.GetMovieById(movieId));

            // Assert
            Assert.Equal(200, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            Assert.Equal(movieId, response.RootElement.GetProperty("data").GetProperty("id").GetInt32());
            Assert.Equal("Test Movie", response.RootElement.GetProperty("data").GetProperty("title").GetString());
        }

        [Fact]
        public async Task GetMovieById_MovieNotFound_ReturnsNotFound()
        {
            // Arrange
            var movieId = 999;
            _mockTmdbService.Setup(x => x.GetMovieById(movieId))
                           .ThrowsAsync(new NotFoundException($"Movie {movieId} not found"));

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.GetMovieById(movieId));

            // Assert
            Assert.Equal(404, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(404, error.GetProperty("code").GetInt32());
            Assert.Contains("not found", error.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetMovieById_ServiceError_Returns500()
        {
            // Arrange
            var movieId = 123;
            _mockTmdbService.Setup(x => x.GetMovieById(movieId))
                           .ThrowsAsync(new Exception("Service error"));

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.GetMovieById(movieId));

            // Assert
            Assert.Equal(500, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(500, error.GetProperty("code").GetInt32());
            Assert.Equal("An unexpected error occurred", error.GetProperty("message").GetString());
        }

        #endregion

        // #region GetMovieImagesById Tests
        //
        // [Fact]
        // public async Task GetMediaImagesById_ValidId_ReturnsImages()
        // {
        //     // Arrange
        //     var movieId = 123;
        //     var expectedImages = new TmdbImageCollection();
        //     _mockTmdbService.Setup(x => x.GetMovieImagesById(movieId))
        //                    .ReturnsAsync(expectedImages);
        //
        //     // Act
        //     var (statusCode, response) = await GetResultDetails(() => _controller.GetMovieImagesById(movieId));
        //
        //     // Assert
        //     Assert.Equal(200, statusCode);
        //     Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
        //     Assert.True(response.RootElement.GetProperty("data").ValueKind == JsonValueKind.Object);
        // }
        //
        // [Fact]
        // public async Task GetMovieImagesById_ImageNotFound_ReturnsNotFound()
        // {
        //     // Arrange
        //     var movieId = 999;
        //     _mockTmdbService.Setup(x => x.GetMovieImagesById(movieId))
        //                    .ThrowsAsync(new NotFoundException("Images not found"));
        //
        //     // Act
        //     var (statusCode, response) = await GetResultDetails(() => _controller.GetMovieImagesById(movieId));
        //
        //     // Assert
        //     Assert.Equal(404, statusCode);
        //     Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
        //     Assert.Equal(404, response.RootElement.GetProperty("error").GetProperty("code").GetInt32());
        // }
        //
        // #endregion

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
            var (statusCode, response) = await GetResultDetails(() => _controller.GetMovieRuntime(movieId));

            // Assert
            Assert.Equal(200, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            Assert.Equal(expectedRuntime, response.RootElement.GetProperty("data").GetInt32());
        }

        [Fact]
        public async Task GetMovieRuntime_MovieNotFound_ReturnsNotFound()
        {
            // Arrange
            var movieId = 999;
            _mockTmdbService.Setup(x => x.GetMovieRuntime(movieId))
                           .ThrowsAsync(new NotFoundException("Movie not found"));

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.GetMovieRuntime(movieId));

            // Assert
            Assert.Equal(404, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(404, error.GetProperty("code").GetInt32());
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
            var (statusCode, response) = await GetResultDetails(() => _controller.SearchMovies(query));

            // Assert
            Assert.Equal(200, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var data = response.RootElement.GetProperty("data");
            Assert.Equal(1, data.GetProperty("totalItems").GetInt32());
            Assert.True(data.GetProperty("items").GetArrayLength() > 0);
        }

        [Fact]
        public async Task SearchMovies_EmptyQuery_ReturnsBadRequest()
        {
            // Arrange
            string query = "";

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.SearchMovies(query));

            // Assert
            Assert.Equal(400, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(400, error.GetProperty("code").GetInt32());
            Assert.Contains("required", error.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SearchMovies_NoResults_ReturnsEmptyCollection()
        {
            // Arrange
            var query = "fggfgffgfgfgfgffggffg";
            _mockTmdbService.Setup(x => x.SearchMovies(query))
                           .ReturnsAsync((Array.Empty<TmdbMovieResponse>(), 0));

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.SearchMovies(query));

            // Assert
            Assert.Equal(200, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var data = response.RootElement.GetProperty("data");
            Assert.Equal(0, data.GetProperty("totalItems").GetInt32());
            Assert.Equal(0, data.GetProperty("items").GetArrayLength());
        }

        [Fact]
        public async Task SearchMovies_ServiceError_Returns500()
        {
            // Arrange
            var query = "test";
            _mockTmdbService.Setup(x => x.SearchMovies(query))
                           .ThrowsAsync(new Exception("Service error"));

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.SearchMovies(query));

            // Assert
            Assert.Equal(500, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(500, error.GetProperty("code").GetInt32());
            Assert.Equal("An unexpected error occurred", error.GetProperty("message").GetString());
        }

        #endregion
    }
}