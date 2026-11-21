namespace Multitool.Domain.Enums;

/// <summary>
/// Authorization policy passed to <c>IBoardAccessService.EnsureAccessAsync</c>.
/// The policy also tells the service how to interpret the given id.
/// </summary>
public enum BoardAccess
{
    /// <summary>The id is a board id; the current user must be a member of that board.</summary>
    Member,

    /// <summary>The id is a board id; membership is checked first, then ownership.</summary>
    Owner,

    /// <summary>
    /// The id is a column id, not a board id. The service resolves the owning board
    /// via the column and checks membership on that board.
    /// </summary>
    Column
}
