using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using User = Nostromo.Server.Database.User;
using Microsoft.AspNetCore.Http;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthTokenRepository _authTokenRepository;
        private readonly IUserRepository _userRepository;

        public AuthController(
            HttpClient httpClient,
            IDatabaseService databaseService,
            ILogger<AuthController> logger,
            IUserRepository userRepository,
            IAuthTokenRepository authTokenRepository)
        {
            _httpClient = httpClient;
            _databaseService = databaseService;
            _logger = logger;
            _userRepository = userRepository;
            _authTokenRepository = authTokenRepository;
        }

        [HttpPost("register")]
        public async Task<IResult> Register([FromBody] RegisterRequest registerRequest)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(registerRequest.username) ||
                string.IsNullOrWhiteSpace(registerRequest.password))
            {
                throw new ArgumentException("Username and password are required");
            }

            // Check if user already exists
            var existingUser = await _userRepository.FindByUsernameAsync(
                registerRequest.username);
            if (existingUser != null)
            {
                throw new ArgumentException("Username already exists");
            }

            string salt = PasswordHelper.GenerateSalt();
            string hashedPassword = PasswordHelper.HashPassword(registerRequest.password, salt);

            var user = new User
            {
                Username = registerRequest.username,
                PasswordHash = hashedPassword,
                Salt = salt,
                IsAdmin = registerRequest.isAdmin
            };
            await _userRepository.AddAsync(user);

            _logger.LogInformation("User registered successfully: {Username}", user.Username);
            return ApiResults.Success(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IResult> Login([FromBody] LoginRequest loginRequest)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(loginRequest.username) ||
                string.IsNullOrWhiteSpace(loginRequest.password))
            {
                throw new ArgumentException("Username and password are required");
            }

            var user = await _userRepository.FindByUsernameAsync(loginRequest.username);

            if (user == null)
            {
                // Use the same message as password failure to avoid username enumeration
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            string passwordHash = user.PasswordHash;
            string salt = user.Salt;

            if (!PasswordHelper.VerifyPassword(loginRequest.password, passwordHash, salt))
            {
                throw new ArgumentException("Invalid username or password");
            }

            _logger.LogInformation("User logged in successfully: {Username}", user.Username);
            var token = _authTokenRepository.CreateToken(user, loginRequest.device);

            return ApiResults.Success(new
            {
                message = "User logged in successfully",
                token = token
            });
        }
    }

    public class RegisterRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public bool isAdmin { get; set; }
    }

    public class LoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string device { get; set; }
    }
}