using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Nostromo.Models;

namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseService _databaseService;

        public AuthController(HttpClient httpClient, DatabaseService databaseService)
        {
            _httpClient = httpClient;
            _databaseService = databaseService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest registerRequest)
        {
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

            _databaseService.InsertUserLogin(user);
            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var user = new Users();
            user = _databaseService.FindUserByUsername(loginRequest.username);
            string passwordHash = user.passwordHash.ToString();
            string salt = user.salt.ToString();
            if (user == null || ! PasswordHelper.VerifyPassword(loginRequest.password, passwordHash,salt))
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            return Ok(new { Message = "User logged in successfully" });
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
