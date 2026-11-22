using Multitool.Domain.Entities.Categories;

namespace Multitool.Application.Interfaces;

/// <summary>
/// Handles the per-board category whitelist for Kanban. The whitelist is part of the board detail read model;
/// members can validate card category ids against it, while changing it is reserved for the board owner.
/// </summary>
public interface IKanbanCategoryService
{
    /// <summary>
    /// Resolves client-supplied category ids against the board whitelist and returns the tracked categories.
    /// Throws when an id is outside the whitelist.
    /// </summary>
    Task<List<Category>> ValidateBoardCategoriesAsync(long boardId, IReadOnlyList<int> categoryIds);

    /// <summary>Adds a category to the board whitelist. Owner only.</summary>
    Task AddCategoryToBoardAsync(long boardId, int categoryId);

    /// <summary>Removes a category from the board whitelist. Owner only.</summary>
    Task RemoveCategoryFromBoardAsync(long boardId, int categoryId);
}
