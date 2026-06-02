using System.Security.Authentication;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class AuthenticationService(IUserRepository userRepository, IPasswordHasher hasher, IJwtTokenGenerator jwt, IAdminKeyProvider keyProvider) : IAuthenticationService
{
    public async Task RegisterAsync(string username, string password, string adminKey)
    {
        if (!IsAdminKeyValid(adminKey))
            throw new InvalidCredentialException("Invalid admin key");

        var existing = await userRepository.GetByUsernameAsync(username);
        if (existing != null)
            throw new UserAlreadyExistsException(username);

        var user = new User
        {
            Username = username,
            PasswordHash = hasher.Hash(password)
        };

        await userRepository.AddAsync(user);
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        await Task.Delay(Random.Shared.Next(300, 600));

        var user = await userRepository.GetByUsernameAsync(username);

        if (user == null)
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
            await userRepository.UpdateAsync(user);
            throw new InvalidCredentialException("Invalid credentials");
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        await userRepository.UpdateAsync(user);

        return jwt.GenerateToken(user);
    }

    private bool IsAdminKeyValid(string key)
    {
        return key == keyProvider.GetAdminKey();
    }
}
