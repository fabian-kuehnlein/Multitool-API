using Multitool.Domain.Enums;

namespace Multitool.Domain.Entities.Config;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public int AccessFailedCount { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
