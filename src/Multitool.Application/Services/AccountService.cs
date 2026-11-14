using System.Security.Authentication;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Account;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class AccountService(
    IUserRepository userRepository,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt,
    IUserContext userContext) : IAccountService
{
    public async Task<string> UpdateAccountCredentialsAsync(UpdateAccountCredentialsDto dto)
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

        if (!user.IsActive)
            throw new InvalidCredentialException("Invalid credentials");

        // This endpoint is only for the forced setup after a temporary password.
        // A general password change must verify the current password instead.
        if (!user.MustChangePassword)
            throw new InvalidOperationException("Credentials can only be changed while a password change is required.");

        if (hasher.Verify(dto.Password, user.PasswordHash))
            throw new InvalidOperationException("The new password must differ from the temporary password.");

        if (user.Username != dto.Username)
        {
            var existing = await userRepository.GetUserByUsernameAsync(dto.Username);

            if (existing != null)
                throw new UserAlreadyExistsException("User with the given username already exists.");
        }

        user.Username = dto.Username;
        user.PasswordHash = hasher.Hash(dto.Password);
        user.MustChangePassword = false;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;

        await userRepository.UpdateUserAsync(user);

        return jwt.GenerateToken(user);
    }
}
