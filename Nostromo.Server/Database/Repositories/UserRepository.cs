using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Database;
using Microsoft.EntityFrameworkCore;

namespace Nostromo.Server.Database.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NostromoDbContext _context;

    public UserRepository(NostromoDbContext context)
    {
        _context = context;
    }

    public async Task<User> FindByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}