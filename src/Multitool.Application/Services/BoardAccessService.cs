using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

/// <summary>
/// Authorization guards for write paths whose repository methods load tracked entities without
/// a user filter. Read projections filter by membership inside their own query and do not use this service.
/// The guards only decide; the calling service loads the entity it mutates.
/// </summary>
public sealed class BoardAccessService(
    IBoardRepository boardRepository,
    IUserContext userContext) : IBoardAccessService
{
    public Task EnsureAccessAsync(BoardAccess access, long id)
        => access switch
        {
            BoardAccess.Member or BoardAccess.Owner =>
                LoadAndCheckBoardAsync(id, requireOwner: access == BoardAccess.Owner),
            BoardAccess.Column => LoadAndCheckColumnAsync(id),
            _ => throw new ArgumentOutOfRangeException(nameof(access), access, null)
        };

    public Task EnsureAccessAsync(BoardAccess access, Board board)
    {
        var userId = GetUserIdForBoardPolicy(access);

        EnsureBoardMembership(board, userId);
        if (access == BoardAccess.Owner)
        {
            EnsureBoardOwnership(board, userId);
        }

        return Task.CompletedTask;
    }

    public Task EnsureAccessAsync(BoardAccess access, BoardColumn column)
    {
        if (access != BoardAccess.Column)
        {
            throw new ArgumentException(
                $"Access policy {access} requires a board context, not a column.",
                nameof(access));
        }

        EnsureBoardMembership(column.Board, userContext.UserId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Loads the tracked board including Members once for the authorization decision.
    /// Tracking is required because member mutations change the Members skip navigation.
    /// </summary>
    private async Task LoadAndCheckBoardAsync(long boardId, bool requireOwner)
    {
        var userId = userContext.UserId;
        var board = await boardRepository.GetBoardForAccessCheckAsync(boardId);

        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        EnsureBoardMembership(board, userId);
        if (requireOwner)
        {
            EnsureBoardOwnership(board, userId);
        }
    }

    private async Task LoadAndCheckColumnAsync(long columnId)
    {
        var column = await boardRepository.GetColumnByIdAsync(columnId);
        if (column is null)
        {
            throw new NotFoundException($"Column with ID {columnId} not found.");
        }

        EnsureBoardMembership(column.Board, userContext.UserId);
    }

    private int GetUserIdForBoardPolicy(BoardAccess access)
    {
        if (access is not (BoardAccess.Member or BoardAccess.Owner))
        {
            throw new ArgumentException(
                $"Access policy {access} requires a column context, not a board.",
                nameof(access));
        }

        return userContext.UserId;
    }

    private static void EnsureBoardMembership(Board board, int userId)
    {
        if (!board.Members.Any(u => u.Id == userId))
        {
            throw new NotFoundException($"Board with ID {board.Id} not found.");
        }
    }

    private static void EnsureBoardOwnership(Board board, int userId)
    {
        if (board.OwnerId != userId)
        {
            throw new ForbiddenException($"User {userId} is not the owner of board {board.Id}.");
        }
    }
}
