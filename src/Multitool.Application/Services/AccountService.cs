using System.Security.Authentication;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Account;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class AccountService(
    IUserRepository userRepository,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt,
    IUserContext userContext) : IAccountService
{
    public async Task VerifyAccountCredentialsAsync(VerifyAccountCredentialsRequest request)
    {
        var user = await GetAuthenticatedUserAsync();

        var usernameMatches = string.Equals(user.Username, request.Username, StringComparison.Ordinal);
        if (!usernameMatches || !hasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("The current credentials are incorrect.");
    }

    public async Task<string> SetupAccountCredentialsAsync(SetupAccountCredentialsRequest request)
    {
        var user = await GetAuthenticatedUserAsync();

        // This endpoint is only for the forced setup after a temporary password.
        // A regular change must verify the current password instead.
        if (!user.MustChangePassword)
            throw new InvalidOperationException("Credentials can only be set up while a password change is required.");

        if (hasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("The new password must differ from the temporary password.");

        await EnsureUsernameIsAvailableAsync(user, request.Username);

        user.Username = request.Username;
        user.PasswordHash = hasher.Hash(request.Password);
        user.MustChangePassword = false;
        ResetFailedLogin(user);

        await userRepository.UpdateUserAsync(user);

        return jwt.GenerateToken(user);
    }

    public async Task<string> ChangeAccountCredentialsAsync(ChangeAccountCredentialsRequest request)
    {
        var user = await GetAuthenticatedUserAsync();

        if (!hasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("The current password is incorrect.");

        var newUsername = request.NewUsername?.Trim();
        var usernameChanged = !string.IsNullOrWhiteSpace(newUsername) && newUsername != user.Username;
        var passwordChanged = !string.IsNullOrWhiteSpace(request.NewPassword);

        if (!usernameChanged && !passwordChanged)
            throw new InvalidOperationException("At least one new credential must be provided.");

        if (passwordChanged && hasher.Verify(request.NewPassword!, user.PasswordHash))
            throw new InvalidOperationException("The new password must differ from the current password.");

        if (usernameChanged)
        {
            await EnsureUsernameIsAvailableAsync(user, newUsername!);
            user.Username = newUsername!;
        }

        if (passwordChanged)
            user.PasswordHash = hasher.Hash(request.NewPassword!);

        ResetFailedLogin(user);
        await userRepository.UpdateUserAsync(user);

        return jwt.GenerateToken(user);
    }

    private async Task<User> GetAuthenticatedUserAsync()
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

        if (!user.IsActive)
            throw new InvalidCredentialException("Invalid credentials");

        return user;
    }

    private async Task EnsureUsernameIsAvailableAsync(User user, string username)
    {
        if (user.Username == username)
            return;

        var existing = await userRepository.GetUserByUsernameAsync(username);

        if (existing != null)
            throw new UserAlreadyExistsException("User with the given username already exists.");
    }

    private static void ResetFailedLogin(User user)
    {
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
    }
}
