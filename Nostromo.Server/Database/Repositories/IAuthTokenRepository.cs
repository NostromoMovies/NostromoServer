// IAuthTokenRepository.cs
namespace Nostromo.Server.Database.Repositories
{
    public interface IAuthTokenRepository : IRepository<AuthToken>
    {
        AuthToken CreateToken(User user, string device);

        AuthToken GetByToken(string token);
    }
}
