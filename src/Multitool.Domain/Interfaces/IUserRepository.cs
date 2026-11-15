using Multitool.Domain.Entities.Config;

namespace Multitool.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<List<User>> GetUsersAsync();
    Task<int> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
}
