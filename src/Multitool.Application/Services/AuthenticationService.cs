using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Authentification;

namespace Multitool.Application.Services;

public class AuthenticationService(IUserRepository userRepository, IPasswordHasher hasher, IJwtTokenGenerator jwt) : IAuthenticationService
{
    public async Task RegisterAsync(string username, string password)
    {
        var existing = await userRepository.GetByUsernameAsync(username);
        if (existing != null)
            throw new Exception("User already exists");

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
            throw new Exception("Invalid credentials");

        return jwt.GenerateToken(user);
    }
}
