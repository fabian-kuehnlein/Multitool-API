using Multitool.Api.Extensions;
using Multitool.Domain.Entities.Config;

namespace Multitool.Tests.Shared;

public static class AuthTestData
{
    public static User DefaultUser => new()
    {
        Id = 1,
        Username = "testuser",
        PasswordHash = "hashed-password"
    };

    public static RegisterRequest DefaultRegisterRequest => new("newuser", "password123");

    public static LoginRequest DefaultLoginRequest => new("testuser", "password123");

    public const string ValidAdminKey = "secret-admin-key";
}
