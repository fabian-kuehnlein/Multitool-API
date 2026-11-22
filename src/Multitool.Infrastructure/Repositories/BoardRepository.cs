using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

/// <summary>
/// Repository for kanban boards and columns.
/// Method categories:
/// - Overview projection: AsNoTracking + Select for list/sidebar views, read-only.
/// - Detail projection: AsNoTracking + Select for API detail views, read-only.
/// - Tracked entity: regular EF tracking for business operations and updates.
/// - Access/ownership query: tracked entity with required includes for permission, membership, and ownership checks.
/// </summary>
public class BoardRepository(AppDbContext db) : IBoardRepository
{
    /// <summary>
    /// Overview projection for the board card grid.
    /// Loads only data required by the overview (Id, Name, Description, CreatedAt, member ids for MemberCount).
    /// Never tracked and never persisted.
    /// </summary>
    public async Task<List<Board>> GetBoardOverviewAsync(int userId)
    {
        return await db.Boards
            .AsNoTracking()
            .Where(b => b.Members.Any(u => u.Id == userId))
            .Select(b => new Board
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                CreatedAt = b.CreatedAt,
                Members = b.Members.Select(u => new User { Id = u.Id }).ToList()
            })
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Detail projection for a single board view.
    /// Loads the full read model (members, whitelisted categories, columns, cards, non-deleted card category ids)
    /// for an accessible board.
    /// Never tracked and never persisted. Uses IgnoreQueryFilters only because of the Category global filter;
    /// access isolation remains enforced by the explicit membership predicate.
    /// </summary>
    public async Task<Board?> GetBoardWithDetailsAsync(long boardId, int userId)
    {
        // IgnoreQueryFilters is required only because Category has a global query filter;
        // access isolation remains enforced by the explicit membership predicate.
        return await db.Boards
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(b => b.Id == boardId && b.Members.Any(u => u.Id == userId))
            .Select(b => new Board
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                OwnerId = b.OwnerId,
                CreatedAt = b.CreatedAt,
                Members = b.Members
                    .Select(u => new User { Id = u.Id, Username = u.Username })
                    .ToList(),
                Categories = b.Categories
                    .Where(c => c.UserId == b.OwnerId && !c.IsDeleted && c.ApplicableModules.Contains(AppModule.Kanban))
                    .Select(c => new Category
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        Name = c.Name,
                        Color = c.Color,
                        ApplicableModules = c.ApplicableModules,
                        IsDeleted = c.IsDeleted
                    })
                    .ToList(),
                Columns = b.Columns
                    .OrderBy(c => c.Position)
                    .Select(c => new BoardColumn
                    {
                        Id = c.Id,
                        BoardId = c.BoardId,
                        Name = c.Name,
                        Position = c.Position,
                        Cards = c.Cards
                            .OrderBy(card => card.Position)
                            .Select(card => new Card
                            {
                                Id = card.Id,
                                ColumnId = card.ColumnId,
                                Title = card.Title,
                                Description = card.Description,
                                Position = card.Position,
                                CreatedByUserId = card.CreatedByUserId,
                                CreatedAt = card.CreatedAt,
                                Categories = card.Categories
                                    .Where(cat => !cat.IsDeleted)
                                    .Select(cat => new Category
                                    {
                                        Id = cat.Id,
                                        Name = string.Empty,
                                        Color = string.Empty
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Access/ownership query used primarily by BoardAccessService.
    /// Loads a tracked board including Members so access, ownership, and member-management decisions can be made
    /// (e.g. can view/edit/share/remove members). Categories are included as well so the tracked skip navigation
    /// can be changed by the owner without loading the board again. Not a read projection: it returns a tracked
    /// entity because the same loaded graph is required for follow-up skip-navigation changes.
    /// </summary>
    public async Task<Board?> GetBoardForAccessCheckAsync(long boardId)
    {
        return await db.Boards
            .Include(b => b.Members)
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == boardId);
    }

    /// <summary>
    /// Default tracked entity load for a single board.
    /// Use for general business operations like editing or deleting a board when members are not required.
    /// Use GetBoardForAccessCheckAsync instead when membership/ownership checks or member mutations are needed.
    /// </summary>
    public async Task<Board?> GetBoardByIdAsync(long boardId)
    {
        return await db.Boards.FirstOrDefaultAsync(b => b.Id == boardId);
    }

    /// <summary>
    /// Loads the tracked categories that are whitelisted for a board and still eligible for Kanban.
    /// Used only by KanbanCategoryService.ValidateBoardCategoriesAsync; the board detail projection
    /// exposes the same whitelist as a read model. IgnoreQueryFilters is required because the whitelist
    /// belongs to the board owner, who may differ from the current user. Membership is checked by the
    /// calling service before this lookup. The categories stay tracked for card category joins.
    /// </summary>
    public async Task<List<Category>> GetBoardCategoriesForValidationAsync(long boardId)
    {
        // IgnoreQueryFilters is required only because Category has a global query filter for its owner.
        var board = await db.Boards
            .IgnoreQueryFilters()
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board is null)
        {
            return [];
        }

        return board.Categories
            .Where(c => c.UserId == board.OwnerId && !c.IsDeleted && c.ApplicableModules.Contains(AppModule.Kanban))
            .ToList();
    }

    /// <summary>
    /// Adds a category to the tracked board whitelist. The composite primary key of board_categories
    /// makes repeated calls harmless.
    /// </summary>
    public async Task AddBoardCategoryAsync(Board board, Category category)
    {
        if (board.Categories.Any(c => c.Id == category.Id))
            return;

        board.Categories.Add(category);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a category from the tracked board whitelist without touching card category joins.
    /// </summary>
    public async Task RemoveBoardCategoryAsync(Board board, Category category)
    {
        var existing = board.Categories.FirstOrDefault(c => c.Id == category.Id);
        if (existing is null)
            return;

        board.Categories.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task<long> CreateBoardAsync(Board board)
    {
        db.Boards.Add(board);
        await db.SaveChangesAsync();

        return board.Id;
    }

    public async Task UpdateBoardAsync(Board board)
    {
        db.Boards.Update(board);
        await db.SaveChangesAsync();
    }

    public async Task DeleteBoardAsync(Board board)
    {
        db.Boards.Remove(board);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Access/ownership query for column-scoped operations (access checks, edit, delete).
    /// Loads Column with Board and Board.Members to evaluate board membership via the parent board.
    /// Board.Columns is intentionally not included because current column use cases do not require sibling columns.
    /// </summary>
    public async Task<BoardColumn?> GetColumnByIdAsync(long columnId)
    {
        return await db.BoardColumns
            .Include(c => c.Board)
                .ThenInclude(b => b.Members)
            .FirstOrDefaultAsync(c => c.Id == columnId);
    }

    public async Task<long> CreateColumnAsync(BoardColumn column)
    {
        db.BoardColumns.Add(column);
        await db.SaveChangesAsync();

        return column.Id;
    }

    public async Task UpdateColumnAsync(BoardColumn column)
    {
        db.BoardColumns.Update(column);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Bulk order update: one query loads all affected tracked columns, all positions are
    /// assigned in memory and committed with a single SaveChangesAsync. Nothing is saved
    /// when an id does not belong to the board, so no partial order can be persisted.
    /// </summary>
    public async Task<bool> UpdateColumnOrderAsync(long boardId, IReadOnlyDictionary<long, int> positions)
    {
        var ids = positions.Keys.ToList();
        var columns = await db.BoardColumns
            .Where(c => c.BoardId == boardId && ids.Contains(c.Id))
            .ToListAsync();

        if (columns.Count != ids.Count)
        {
            return false;
        }

        foreach (var column in columns)
        {
            column.Position = positions[column.Id];
        }

        await db.SaveChangesAsync();
        return true;
    }

    public async Task DeleteColumnAsync(BoardColumn column)
    {
        db.BoardColumns.Remove(column);
        await db.SaveChangesAsync();
    }

    public async Task AddBoardMemberAsync(Board board, User user)
    {
        if (board.Members.Any(u => u.Id == user.Id))
            return;

        board.Members.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task RemoveBoardMemberAsync(Board board, User user)
    {
        var existing = board.Members.FirstOrDefault(u => u.Id == user.Id);
        if (existing is null)
            return;

        board.Members.Remove(existing);
        await db.SaveChangesAsync();
    }
}
