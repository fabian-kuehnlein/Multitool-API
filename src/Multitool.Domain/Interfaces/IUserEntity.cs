namespace Multitool.Domain.Interfaces;

/// <summary>
/// Marks an entity as owned by exactly one user. Every implementation is automatically
/// scoped to the current user via a global query filter in the <c>AppDbContext</c>.
/// </summary>
public interface IUserEntity
{
    int UserId { get; set; }
}
