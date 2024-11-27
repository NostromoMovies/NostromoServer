using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using Nostromo.Models;
using Nostromo.Server.Services;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<AuthController> _logger;
        private readonly AuthTokenRepository authTokenRepository;
        private readonly UserRepository userRepository;

        public AuthController(
            HttpClient httpClient,
            IDatabaseService databaseService,
            ILogger<AuthController> logger)
        {
            _httpClient = httpClient;
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(registerRequest.username) ||
                    string.IsNullOrWhiteSpace(registerRequest.password))
                {
                    return BadRequest(new { Message = "Username and password are required" });
                }

                // Check if user already exists
                var existingUser = await _databaseService.FindUserByUsernameAsync(registerRequest.username);
                if (existingUser != null)
                {
                    return Conflict(new { Message = "Username already exists" });
                }

                string salt = PasswordHelper.GenerateSalt();
                string hashedPassword = PasswordHelper.HashPassword(registerRequest.password, salt);

                var user = new Users
                {
                    username = registerRequest.username,
                    passwordHash = hashedPassword,
                    first_name = registerRequest.first_Name,
                    last_name = registerRequest.last_Name,
                    salt = salt
                };

                await _databaseService.CreateUserAsync(user);

                _logger.LogInformation("User registered successfully: {Username}", user.username);
                return Ok(new { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Username}", registerRequest.username);
                return StatusCode(500, new { Message = "An error occurred while registering the user" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(loginRequest.username) ||
                    string.IsNullOrWhiteSpace(loginRequest.password))
                {
                    return BadRequest(new { Message = "Username and password are required" });
                }

                var user = await _databaseService.FindUserByUsernameAsync(loginRequest.username);

                if (user == null)
                {
                    // Use the same message as password failure to avoid username enumeration
                    return Unauthorized(new { Message = "Invalid username or password" });
                }

                string passwordHash = user.PasswordHash; // Note: Using the correct property name from User entity
                string salt = user.Salt;

                if (!PasswordHelper.VerifyPassword(loginRequest.password, passwordHash, salt))
                {
                    return Unauthorized(new { Message = "Invalid username or password" });
                }

                _logger.LogInformation("User logged in successfully: {Username}", user.Username);
                return Ok(new
                {
                    Message = "User logged in successfully",
                    User = new
                    {
                        Username = user.Username,
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", loginRequest.username);
                return StatusCode(500, new { Message = "An error occurred during login" });
            }
        }
    }

    public class RegisterRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string first_Name { get; set; }
        public string last_Name { get; set; }
    }

    public class LoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}