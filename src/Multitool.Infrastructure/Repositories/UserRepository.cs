using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await db.Users.SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await db.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<int> CreateUserAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.Id;
    }

    public async Task UpdateUserAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }
}
