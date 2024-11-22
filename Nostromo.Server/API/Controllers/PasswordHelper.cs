using Microsoft.AspNetCore.Identity.Data;
using Nostromo.Models;
using System;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;



namespace Nostromo.Server.API.Controllers
{
   public static class PasswordHelper
    {
        public static string GenerateSalt(int size = 32)
        {
            
            var salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);

        }

        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedByte = Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(combinedByte);
                return Convert.ToBase64String(hashBytes);
            }
        }
       public static bool VerifyPassword(string password, string encoded_password,string salt)
        {
            string hashedPassword = HashPassword(password, salt);
            if (hashedPassword == encoded_password)
            {
                return true; 
            }
            return false;
        }
    }

}
