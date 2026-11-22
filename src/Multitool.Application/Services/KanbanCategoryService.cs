using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public sealed class KanbanCategoryService(
    IBoardRepository boardRepository,
    ICategoryRepository categoryRepository,
    IBoardAccessService boardAccessService) : IKanbanCategoryService
{
    public async Task<List<Category>> ValidateBoardCategoriesAsync(long boardId, IReadOnlyList<int> categoryIds)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Member, boardId);

        var allowed = await boardRepository.GetBoardCategoriesForValidationAsync(boardId);
        var categories = new List<Category>();

        foreach (var categoryId in categoryIds.Distinct())
        {
            var category = allowed.FirstOrDefault(c => c.Id == categoryId);
            if (category is null)
            {
                throw new CategoryNotAvailableForModuleException(
                    $"Category with ID {categoryId} is not available for board {boardId}.");
            }

            categories.Add(category);
        }

        return categories;
    }

    public async Task AddCategoryToBoardAsync(long boardId, int categoryId)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Owner, boardId);

        var board = await GetBoardForOperationAsync(boardId);
        var category = await GetAvailableCategoryAsync(board.OwnerId, categoryId);

        await boardRepository.AddBoardCategoryAsync(board, category);
    }

    public async Task RemoveCategoryFromBoardAsync(long boardId, int categoryId)
    {
        await boardAccessService.EnsureAccessAsync(BoardAccess.Owner, boardId);

        var board = await GetBoardForOperationAsync(boardId);
        var category = await GetAvailableCategoryAsync(board.OwnerId, categoryId);

        await boardRepository.RemoveBoardCategoryAsync(board, category);
    }

    private async Task<Board> GetBoardForOperationAsync(long boardId)
    {
        // The board is loaded with its tracked whitelist because Add/Remove mutate the skip navigation.
        var board = await boardRepository.GetBoardForAccessCheckAsync(boardId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        return board;
    }

    private async Task<Category> GetAvailableCategoryAsync(int ownerId, int categoryId)
    {
        var categories = await categoryRepository.GetKanbanCategoriesForOwnerAsync(ownerId);
        var category = categories.FirstOrDefault(c => c.Id == categoryId);
        if (category is null)
        {
            throw new CategoryNotAvailableForModuleException(
                $"Category with ID {categoryId} is not available for the kanban module.");
        }

        return category;
    }
}
