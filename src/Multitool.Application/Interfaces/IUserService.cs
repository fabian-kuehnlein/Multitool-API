using Multitool.Application.Models.User;

namespace Multitool.Application.Interfaces;

public interface IUserService
{
    Task<List<UserModel>> GetUsersAsync();
    Task<int> CreateUserAsync(CreateUserRequest request);
    Task ResetPasswordAsync(int id, ResetPasswordRequest request);
    Task SetUserActiveAsync(int id, SetUserActiveRequest request);
}
