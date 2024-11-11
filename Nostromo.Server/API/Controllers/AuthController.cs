using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
namespace Nostromo.Server.API.Controllers
{

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
         public IActions Register([FromBody] LoginRequest loginRequest)
        {
            var salt = PasswordHelper.GenerateSalt();
            var hashedPassword = _databaseService.HashPassword(loginRequest.Password, salt);

            var user = new User { Username = loginRequest.Username, Password = hashedPassword, First_name = loginRequest.First_Name, Last_Name = loginRequest.Last_Name }
            _databaseService.InsertUser(user);
            return Ok(new { Message = "User registered successfully" })
        }
        [HttpPost("login")]

        public IActions Login([FromBody] LoginRequest loginRequest)
        {
          var user _databaseService.FindUserByUsername(loginRequest.Username);
          if (user == null || !_databaseService.VerifyPassword(loginRequest.Password,user.Password))
            {
                return Unauthorized(new { Message = "Invalid username or password" });

            }
            return Ok(new { Message = | "User logged in successfully" });


        }

    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }

    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }


    }