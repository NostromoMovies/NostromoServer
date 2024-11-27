// IAuthTokenRepository.cs
namespace Nostromo.Server.Database.Repositories
{
    public interface IAuthTokenRepository : IRepository<AuthToken>
    {
        AuthToken CreateToken(User user);

        AuthToken GetByToken(string token);
    }
}
