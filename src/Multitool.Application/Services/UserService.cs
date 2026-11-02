using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.User;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IPasswordHasher hasher,
    IUserContext userContext) : IUserService
{
    public async Task<List<UserDto>> GetUsersAsync()
    {
        var users = await userRepository.GetUsersAsync();
        return users.Adapt<List<UserDto>>();
    }

    public async Task<int> CreateUserAsync(CreateUserDto dto)
    {
        var existing = await userRepository.GetUserByUsernameAsync(dto.Username);

        if (existing != null)
            throw new UserAlreadyExistsException("User with the given username already exists.");

        var user = dto.Adapt<User>();
        user.PasswordHash = hasher.Hash(dto.Password);

        return await userRepository.CreateUserAsync(user);
    }

    public async Task ResetPasswordAsync(int id, ResetPasswordDto dto)
    {
        var user = await userRepository.GetUserByIdAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found.");

        user.PasswordHash = hasher.Hash(dto.Password);
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;

        await userRepository.UpdateUserAsync(user);
    }

    public async Task SetUserActiveAsync(int id, SetUserActiveDto dto)
    {
        var user = await userRepository.GetUserByIdAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found.");

        if (!dto.IsActive && user.Id == userContext.UserId)
            throw new InvalidOperationException("You cannot deactivate your own account.");

        user.IsActive = dto.IsActive;

        if (!dto.IsActive)
        {
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
        }

        await userRepository.UpdateUserAsync(user);
    }
}
