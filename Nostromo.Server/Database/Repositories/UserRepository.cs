namespace Nostromo.Server.Database.Repositories;

using Microsoft.EntityFrameworkCore;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(NostromoDbContext context) : base(context)
    {
    }

    public async Task<User> FindByUsernameAsync(string username)
    {
        return await Query()
            .FirstOrDefaultAsync(u => u.Username == username);
    }
    // example
    // Note: no need to implement CRUD operations - they come from Repository<User>

    public override async Task<User> AddAsync(User user)
    {
        // If you need special behavior when creating users
        // For example, setting CreatedAt
        user.CreatedAt = DateTime.UtcNow;
        return await base.AddAsync(user);
    }
    
}