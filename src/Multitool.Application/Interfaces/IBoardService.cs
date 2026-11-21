using Multitool.Application.Models.Kanban;

namespace Multitool.Application.Interfaces;

public interface IBoardService
{
    /// <summary>Returns the boards the current user is a member of.</summary>
    Task<List<BoardOverviewModel>> GetBoardsAsync();

    /// <summary>
    /// Returns the full board for a member. The repository detail query already enforces membership.
    /// </summary>
    Task<BoardModel> GetBoardByIdAsync(long boardId);

    /// <summary>Creates a board with the current user as owner and first member.</summary>
    Task<long> CreateBoardAsync(CreateBoardRequest request);

    /// <summary>Updates board metadata. Owner only.</summary>
    Task UpdateBoardAsync(long boardId, UpdateBoardRequest request);

    /// <summary>Deletes a board. Owner only.</summary>
    Task DeleteBoardAsync(long boardId);

    /// <summary>Creates a column on an accessible board. Members allowed.</summary>
    Task CreateColumnAsync(long boardId, string name);

    /// <summary>Renames an accessible column. Members allowed.</summary>
    Task UpdateColumnAsync(long columnId, string name);

    /// <summary>
    /// Applies a new column order in one repository call. Members allowed.
    /// The dictionary maps column ids to their new positions.
    /// </summary>
    Task UpdateColumnOrderAsync(long boardId, IReadOnlyDictionary<long, int> positions);

    /// <summary>Deletes an accessible column. Members allowed.</summary>
    Task DeleteColumnAsync(long columnId);

    /// <summary>Lists the members of an accessible board. Members allowed.</summary>
    Task<List<BoardMemberModel>> GetBoardMembersAsync(long boardId);

    /// <summary>Adds a member to the board. Owner only.</summary>
    Task AddBoardMemberAsync(long boardId, string username);

    /// <summary>Removes a member from the board. Owner only.</summary>
    Task RemoveBoardMemberAsync(long boardId, int userId);
}
