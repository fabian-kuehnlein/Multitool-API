using Multitool.Domain.Entities.Config;
using Multitool.Domain.Entities.Kanban;

namespace Multitool.Domain.Interfaces;

public interface IBoardRepository
{
    Task<List<Board>> GetBoardOverviewAsync(int userId);
    Task<Board?> GetBoardWithDetailsAsync(long boardId, int userId);
    Task<Board?> GetBoardForAccessCheckAsync(long boardId);
    Task<Board?> GetBoardByIdAsync(long boardId);
    Task<long> CreateBoardAsync(Board board);
    Task UpdateBoardAsync(Board board);
    Task DeleteBoardAsync(Board board);
    Task<BoardColumn?> GetColumnByIdAsync(long columnId);
    Task<long> CreateColumnAsync(BoardColumn column);
    Task UpdateColumnAsync(BoardColumn column);
    Task UpdateColumnOrderAsync(List<BoardColumn> columns);
    Task DeleteColumnAsync(BoardColumn column);
    Task AddBoardMemberAsync(Board board, User user);
    Task RemoveBoardMemberAsync(Board board, User user);
}
