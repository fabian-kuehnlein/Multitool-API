using Multitool.Domain.Entities.Config;

namespace Multitool.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}