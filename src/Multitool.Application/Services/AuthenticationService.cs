using System.Security.Authentication;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Authentification;

namespace Multitool.Application.Services;

public class AuthenticationService(IUserRepository userRepository, IPasswordHasher hasher, IJwtTokenGenerator jwt) : IAuthenticationService
{
    public async Task RegisterAsync(string username, string password)
    {
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
        var user = await userRepository.GetByUsernameAsync(username);
        
        if (user == null || !hasher.Verify(password, user.PasswordHash))
            throw new InvalidCredentialException("Invalid credentials");

        return jwt.GenerateToken(user);
    }
}
