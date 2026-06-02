using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await db.Users.SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task AddAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }
}
