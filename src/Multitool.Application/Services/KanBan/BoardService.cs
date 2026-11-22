using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Kanban;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class BoardService(
    IBoardRepository boardRepository,
    IUserRepository userRepository,
    IBoardAccessService boardAccessService,
    IUserContext userContext) : IBoardService
{
    public async Task<List<BoardOverviewModel>> GetBoardsOverviewAsync()
    {
        var boards = await boardRepository.GetBoardOverviewAsync(userContext.UserId);
        return boards.Adapt<List<BoardOverviewModel>>();
    }

    public async Task<BoardModel> GetBoardByIdAsync(long boardId)
    {
        var userId = userContext.UserId;

        // The detail projection already contains the membership predicate, so no separate guard is needed.
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId, userId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        var model = board.Adapt<BoardModel>();
        return model with { IsOwner = board.OwnerId == userId };
    }

    public async Task<long> CreateBoardAsync(CreateBoardRequest request)
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        var board = request.Adapt<Board>();
        board.OwnerId = user.Id;
        board.Columns = CreateDefaultColumns();

        // Access is membership-only, so the owner must also be the first member.
        board.Members.Add(user);

        return await boardRepository.CreateBoardAsync(board);
    }

    public async Task UpdateBoardAsync(long boardId, UpdateBoardRequest request)
    {
        // Board metadata is owner-only. The guard throws 403 for members and 404 for non-members.
        await boardAccessService.EnsureAccessAsync(BoardAccess.Owner, boardId);

        var board = await boardRepository.GetBoardByIdAsync(boardId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        board.Name = request.Name;
        board.Description = request.Description;

        await boardRepository.UpdateBoardAsync(board);
    }

    public async Task DeleteBoardAsync(long boardId)
    {
        // Deleting a board is owner-only, same status semantics as the metadata update.
        await boardAccessService.EnsureAccessAsync(BoardAccess.Owner, boardId);

        var board = await boardRepository.GetBoardByIdAsync(boardId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        await boardRepository.DeleteBoardAsync(board);
    }

    public async Task CreateColumnAsync(long boardId, string name)
    {
        // Column management is member-scoped; the board is addressed directly here.
        await boardAccessService.EnsureAccessAsync(BoardAccess.Member, boardId);

        var board = await boardRepository.GetBoardWithDetailsAsync(boardId, userContext.UserId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        var position = board.Columns.Count == 0 ? 0 : board.Columns.Max(c => c.Position) + 1;
        var column = new BoardColumn
        {
            BoardId = boardId,
            Name = name,
            Position = position
        };

        await boardRepository.CreateColumnAsync(column);
    }

    public async Task UpdateColumnAsync(long columnId, string name)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Column, columnId);

        var column = await boardRepository.GetColumnByIdAsync(columnId);
        if (column is null)
        {
            throw new NotFoundException($"Column with ID {columnId} not found.");
        }

        column.Name = name;

        await boardRepository.UpdateColumnAsync(column);
    }

    public async Task UpdateColumnOrderAsync(long boardId, IReadOnlyDictionary<long, int> positions)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Member, boardId);

        // The repository loads all affected columns in one query and saves them in one go.
        // A false result means at least one id is not on this board; nothing was saved then.
        var updated = await boardRepository.UpdateColumnOrderAsync(boardId, positions);
        if (!updated)
        {
            throw new NotFoundException($"One or more columns do not belong to board {boardId}.");
        }
    }

    public async Task DeleteColumnAsync(long columnId)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Column, columnId);

        var column = await boardRepository.GetColumnByIdAsync(columnId);
        if (column is null)
        {
            throw new NotFoundException($"Column with ID {columnId} not found.");
        }

        await boardRepository.DeleteColumnAsync(column);
    }

    public async Task<List<BoardMemberModel>> GetBoardMembersAsync(long boardId)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Member, boardId);

        // Members are required for the response; the lean GetBoardByIdAsync does not include them.
        var board = await boardRepository.GetBoardForAccessCheckAsync(boardId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        return board.Members.Adapt<List<BoardMemberModel>>();
    }

    public async Task AddBoardMemberAsync(long boardId, string username)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Owner, boardId);

        // Members must be loaded because EF needs the skip navigation for the insert.
        var board = await boardRepository.GetBoardForAccessCheckAsync(boardId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user is null)
        {
            throw new NotFoundException($"User with username {username} not found.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException($"User with username {username} is not active.");
        }

        await boardRepository.AddBoardMemberAsync(board, user);
    }

    public async Task RemoveBoardMemberAsync(long boardId, int userId)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Owner, boardId);

        var board = await boardRepository.GetBoardForAccessCheckAsync(boardId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        var user = await userRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            throw new NotFoundException($"User with ID {userId} not found.");
        }

        await boardRepository.RemoveBoardMemberAsync(board, user);
    }

    private static readonly IReadOnlyList<string> DefaultColumnNames = ["Backlog", "In Arbeit", "Fertig"];

    private static List<BoardColumn> CreateDefaultColumns()
    {
        return DefaultColumnNames
            .Select((name, index) => new BoardColumn { Name = name, Position = index })
            .ToList();
    }
}
