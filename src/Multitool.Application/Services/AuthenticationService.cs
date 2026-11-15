using System.Security.Authentication;
using Multitool.Application.Interfaces;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class AuthenticationService(
    IUserRepository userRepository,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt) : IAuthenticationService
{
    public async Task<string> LoginAsync(string username, string password)
    {
        await Task.Delay(Random.Shared.Next(300, 600));

        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null)
            throw new InvalidCredentialException("Invalid credentials");

        if (!user.IsActive)
            throw new InvalidCredentialException("Invalid credentials");

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            throw new InvalidCredentialException("Invalid credentials");

        if (!hasher.Verify(password, user.PasswordHash))
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
            await userRepository.UpdateUserAsync(user);
            throw new InvalidCredentialException("Invalid credentials");
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        await userRepository.UpdateUserAsync(user);

        return jwt.GenerateToken(user);
    }
}
