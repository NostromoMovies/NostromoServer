// IUserRepository.cs
using System.Threading.Tasks;

namespace Nostromo.Server.Database.Repositories;

public interface IUserRepository
{
    Task<User> FindByUsernameAsync(string username);
    Task<User> GetByIdAsync(int id);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}