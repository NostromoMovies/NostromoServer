// AuthTokenRepository.cs
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Nostromo.Server.Database.Repositories
{
    public class AuthTokenRepository : Repository<AuthToken>, IAuthTokenRepository
    {
        public AuthTokenRepository(NostromoDbContext context) : base(context)
        {
        }

        public AuthToken CreateToken(User user, string device)
        {

            var token = new AuthToken
            {
                Token = GenerateToken(),
                UserId = user.UserID,
                DeviceName = device
            };

            _context.AuthTokens.Add(token);
            _context.SaveChanges();

            return token;
        }

        public AuthToken GetByToken(string token)
        {
            return _context.AuthTokens
                .Include(t => t.User)
                .FirstOrDefault(t => t.Token == token);
        }

        private string GenerateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }
    }
}
