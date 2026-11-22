using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Kanban;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public sealed class CardService(
    IBoardRepository boardRepository,
    ICardRepository cardRepository,
    IBoardAccessService boardAccessService,
    IKanbanCategoryService kanbanCategoryService,
    IUserContext userContext) : ICardService
{
    public async Task<CardModel> GetCardByIdAsync(long cardId)
    {
        var card = await GetAccessibleCardAsync(cardId);
        return card.Adapt<CardModel>();
    }

    public async Task<long> CreateCardAsync(CreateCardRequest request)
    {
        // The caller only knows the column id, so the column guard resolves the board context.
        await boardAccessService.EnsureAccessAsync(BoardAccess.Column, request.ColumnId);

        var column = await boardRepository.GetColumnByIdAsync(request.ColumnId);
        if (column is null)
        {
            throw new NotFoundException($"Column with ID {request.ColumnId} not found.");
        }

        var boardId = column.BoardId;
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId, userContext.UserId);
        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found.");
        }

        var targetColumn = board.Columns.FirstOrDefault(c => c.Id == request.ColumnId);
        if (targetColumn is null)
        {
            throw new NotFoundException($"Column with ID {request.ColumnId} not found.");
        }

        var categories = await kanbanCategoryService.ValidateBoardCategoriesAsync(
            boardId, request.CategoryIds ?? []);

        var card = request.Adapt<Card>();
        card.ColumnId = request.ColumnId;
        card.Position = targetColumn.Cards.Count == 0 ? 0 : targetColumn.Cards.Max(c => c.Position) + 1;
        card.CreatedByUserId = userContext.UserId;
        card.Categories.Clear();

        return await cardRepository.CreateCardAsync(card, categories);
    }

    public async Task UpdateCardAsync(long cardId, UpdateCardRequest request)
    {
        var card = await GetAccessibleCardAsync(cardId);

        card.Title = request.Title;
        card.Description = request.Description;

        await cardRepository.UpdateCardAsync(card);
    }

    public async Task DeleteCardAsync(long cardId)
    {
        var card = await GetAccessibleCardAsync(cardId);
        await cardRepository.DeleteCardAsync(card);
    }

    public async Task MoveCardAsync(long cardId, MoveCardRequest request)
    {
        var card = await GetAccessibleCardAsync(cardId);

        var targetColumn = card.Column.Board.Columns.FirstOrDefault(c => c.Id == request.TargetColumnId);
        if (targetColumn is null)
        {
            throw new NotFoundException($"Column with ID {request.TargetColumnId} not found.");
        }

        await cardRepository.MoveCardAsync(card, request.TargetColumnId, request.Position);
    }

    public async Task SetCardCategoriesAsync(long cardId, SetCardCategoriesRequest request)
    {
        var card = await GetAccessibleCardAsync(cardId);

        var categories = await kanbanCategoryService.ValidateBoardCategoriesAsync(
            card.Column.BoardId, request.CategoryIds);

        await cardRepository.ReplaceCardCategoriesAsync(card, categories);
    }

    private async Task<Card> GetAccessibleCardAsync(long cardId)
    {
        var card = await cardRepository.GetCardForOperationAsync(cardId);
        if (card is null)
        {
            throw new NotFoundException($"Card with ID {cardId} not found.");
        }

        await boardAccessService.EnsureAccessAsync(BoardAccess.Member, card.Column.Board);
        return card;
    }
}
