using Multitool.Domain.Entities.Config;

namespace Multitool.Infrastructure.Authentification;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}