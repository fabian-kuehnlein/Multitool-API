using Multitool.Api.Extensions;
using Multitool.Application.Models.User;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Enums;

namespace Multitool.Tests.Shared;

public static class AuthTestData
{
    public static User DefaultUser => new()
    {
        Id = 1,
        Username = "testuser",
        PasswordHash = "hashed-password",
        Role = UserRole.Admin,
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    public static UserDto DefaultUserDto => new(
        DefaultUser.Id,
        DefaultUser.Username,
        DefaultUser.Role,
        DefaultUser.IsActive,
        DefaultUser.CreatedAt);

    public static CreateUserDto DefaultCreateUserDto => new("newuser", "password123", UserRole.User);

    public static ResetPasswordDto DefaultResetPasswordDto => new("new-password123");

    public static SetUserActiveDto DefaultSetUserActiveDto => new(false);

    public static LoginRequest DefaultLoginRequest => new("testuser", "password123");
}
