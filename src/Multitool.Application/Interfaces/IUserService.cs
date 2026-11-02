using Multitool.Application.Models.User;

namespace Multitool.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetUsersAsync();
    Task<int> CreateUserAsync(CreateUserDto dto);
    Task ResetPasswordAsync(int id, ResetPasswordDto dto);
    Task SetUserActiveAsync(int id, SetUserActiveDto dto);
}
