using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Nostromo.Server.API.Controllers;
using Nostromo.Server.API.Middleware;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Services;
using System.Text.Json;
using Xunit;

namespace Nostromo.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IAuthTokenRepository> _mockAuthTokenRepository;
        private readonly Mock<IDatabaseService> _mockDatabaseService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<ILogger<ApiExceptionMiddleware>> _middlewareLogger;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly AuthController _controller;
        private readonly IServiceProvider _serviceProvider;

        public AuthControllerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthTokenRepository = new Mock<IAuthTokenRepository>();
            _mockDatabaseService = new Mock<IDatabaseService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _middlewareLogger = new Mock<ILogger<ApiExceptionMiddleware>>();
            _mockHttpClient = new Mock<HttpClient>();

            _controller = new AuthController(
                _mockHttpClient.Object,
                _mockDatabaseService.Object,
                _mockLogger.Object,
                _mockUserRepository.Object,
                _mockAuthTokenRepository.Object);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(_mockUserRepository.Object);
            services.AddSingleton(_mockAuthTokenRepository.Object);
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

        #region Register Tests

        [Fact]
        public async Task Register_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new RegisterRequest
            {
                username = "testuser",
                password = "testpass",
                isAdmin = false
            };

            _mockUserRepository.Setup(x => x.FindByUsernameAsync(request.username))
                             .ReturnsAsync((User)null);

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.Register(request));

            // Assert
            Assert.Equal(200, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            Assert.Equal("User registered successfully",
                response.RootElement.GetProperty("data").GetProperty("message").GetString());
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Register_EmptyUsername_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                username = "",
                password = "testpass"
            };

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.Register(request));

            // Assert
            Assert.Equal(400, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(400, error.GetProperty("code").GetInt32());
            Assert.Contains("required", error.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Register_ExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                username = "existing",
                password = "testpass"
            };

            _mockUserRepository.Setup(x => x.FindByUsernameAsync(request.username))
                             .ReturnsAsync(new User { Username = "existing" });

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.Register(request));

            // Assert
            Assert.Equal(400, statusCode);
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(400, error.GetProperty("code").GetInt32());
            Assert.Contains("already exists", error.GetProperty("message").GetString());
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                username = "testuser",
                password = "testpass",
                device = "testdevice"
            };

            var user = new User
            {
                Username = request.username,
                Salt = "testsalt",
                PasswordHash = PasswordHelper.HashPassword(request.password, "testsalt")
            };

            var expectedToken = new AuthToken
            {
                AuthId = 1,
                UserId = 1,
                Token = "test-token",
                DeviceName = request.device,
                User = user
            };

            _mockUserRepository.Setup(x => x.FindByUsernameAsync(request.username))
                              .ReturnsAsync(user);
            _mockAuthTokenRepository.Setup(x => x.CreateToken(user, request.device))
                                  .Returns(expectedToken);

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.Login(request));

            // Assert
            Assert.Equal(200, statusCode);
            Assert.Equal("1.0", response.RootElement.GetProperty("apiVersion").GetString());
            var data = response.RootElement.GetProperty("data");
            Assert.Equal("User logged in successfully", data.GetProperty("message").GetString());
            Assert.True(data.TryGetProperty("token", out _));
        }

        [Fact]
        public async Task Login_InvalidUsername_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                username = "nonexistent",
                password = "testpass",
                device = "testdevice"
            };

            _mockUserRepository.Setup(x => x.FindByUsernameAsync(request.username))
                             .ReturnsAsync((User)null);

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.Login(request));

            // Assert
            Assert.Equal(401, statusCode);
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(401, error.GetProperty("code").GetInt32());
            Assert.Contains("Invalid username or password", error.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                username = "testuser",
                password = "wrongpass",
                device = "testdevice"
            };

            var user = new User
            {
                Username = request.username,
                Salt = "testsalt",
                PasswordHash = PasswordHelper.HashPassword("correctpass", "testsalt")
            };

            _mockUserRepository.Setup(x => x.FindByUsernameAsync(request.username))
                             .ReturnsAsync(user);

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.Login(request));

            // Assert
            Assert.Equal(400, statusCode);
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(400, error.GetProperty("code").GetInt32());
            Assert.Contains("Invalid username or password", error.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                username = "",
                password = "",
                device = "testdevice"
            };

            // Act
            var (statusCode, response) = await GetResultDetails(() => _controller.Login(request));

            // Assert
            Assert.Equal(400, statusCode);
            var error = response.RootElement.GetProperty("error");
            Assert.Equal(400, error.GetProperty("code").GetInt32());
            Assert.Contains("required", error.GetProperty("message").GetString());
        }

        #endregion
    }
}