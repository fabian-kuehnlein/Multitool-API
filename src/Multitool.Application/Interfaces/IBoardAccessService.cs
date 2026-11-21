using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;

namespace Multitool.Application.Interfaces;

/// <summary>
/// Single entry point for all board/column authorization decisions.
/// These are pure guards: they throw on violation and return nothing, so callers cannot accidentally
/// skip the check. The caller loads the entity it wants to mutate afterwards.
/// </summary>
public interface IBoardAccessService
{
    /// <summary>
    /// Ensures the requested <paramref name="access"/> policy for the given <paramref name="id"/>.
    /// Depending on the policy, the id is either a board id or a column id.
    /// Non-members and missing entities produce
    /// <see cref="Multitool.Domain.Exceptions.NotFoundException"/> to avoid leaking board existence;
    /// a member without ownership produces <see cref="Multitool.Domain.Exceptions.ForbiddenException"/>.
    /// </summary>
    Task EnsureAccessAsync(BoardAccess access, long id);

    /// <summary>
    /// Same guarantees as <see cref="EnsureAccessAsync(BoardAccess, long)"/> for an already tracked board.
    /// Only <see cref="BoardAccess.Member"/> and <see cref="BoardAccess.Owner"/> are valid here.
    /// No additional repository call is made.
    /// </summary>
    Task EnsureAccessAsync(BoardAccess access, Board board);

    /// <summary>
    /// Same guarantees as <see cref="EnsureAccessAsync(BoardAccess, long)"/> with
    /// <see cref="BoardAccess.Column"/> for an already tracked column.
    /// No additional repository call is made.
    /// </summary>
    Task EnsureAccessAsync(BoardAccess access, BoardColumn column);
}
