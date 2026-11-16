using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.User;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class UserService(
    IUserRepository userRepository,
    ICategoryRepository categoryRepository,
    IPasswordHasher hasher,
    IUserContext userContext) : IUserService
{
    public async Task<List<UserModel>> GetUsersAsync()
    {
        var users = await userRepository.GetUsersAsync();
        return users.Adapt<List<UserModel>>();
    }

    public async Task<int> CreateUserAsync(CreateUserRequest request)
    {
        var existing = await userRepository.GetUserByUsernameAsync(request.Username);

        if (existing != null)
            throw new UserAlreadyExistsException("User with the given username already exists.");

        var user = request.Adapt<User>();
        user.PasswordHash = hasher.Hash(request.Password);

        var userId = await userRepository.CreateUserAsync(user);

        await CreateDefaultEntities(userId);

        return userId;
    }

    public async Task ResetPasswordAsync(int id, ResetPasswordRequest request)
    {
        var user = await userRepository.GetUserByIdAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found.");

        user.PasswordHash = hasher.Hash(request.Password);
        user.MustChangePassword = true;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;

        await userRepository.UpdateUserAsync(user);
    }

    public async Task SetUserActiveAsync(int id, SetUserActiveRequest request)
    {
        var user = await userRepository.GetUserByIdAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found.");

        if (!request.IsActive && user.Id == userContext.UserId)
            throw new InvalidOperationException("You cannot deactivate your own account.");

        user.IsActive = request.IsActive;

        if (!request.IsActive)
        {
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
        }

        await userRepository.UpdateUserAsync(user);
    }

    private async Task CreateDefaultEntities(int userId)
    {   
        // Default Category     
        await categoryRepository.CreateCategoryAsync(new Category
        {
           UserId = userId,
           Name = "Privat",
           Color = "#3b82f6",
           ApplicableModules = Enum.GetValues<AppModule>().ToList()
        });
    }
}

