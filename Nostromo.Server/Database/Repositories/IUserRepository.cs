namespace Nostromo.Server.Database.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User> FindByUsernameAsync(string username);
    // No need to declare CRUD methods - they come from IRepository<User>
}