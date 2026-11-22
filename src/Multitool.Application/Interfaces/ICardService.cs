using Multitool.Application.Models.Kanban;

namespace Multitool.Application.Interfaces;

public interface ICardService
{
    /// <summary>Returns a card for a member of the parent board.</summary>
    Task<CardModel> GetCardByIdAsync(long cardId);

    /// <summary>Creates a card at the end of the target column. Category ids must be whitelisted.</summary>
    Task<long> CreateCardAsync(CreateCardRequest request);

    /// <summary>Updates title and description. Category ids are written via SetCardCategoriesAsync.</summary>
    Task UpdateCardAsync(long cardId, UpdateCardRequest request);

    /// <summary>Deletes a card. Members allowed.</summary>
    Task DeleteCardAsync(long cardId);

    /// <summary>
    /// Moves a card inside its board and clamps the position in the repository.
    /// The target column must belong to the card's board.
    /// </summary>
    Task MoveCardAsync(long cardId, MoveCardRequest request);

    /// <summary>Replaces the card categories with whitelisted categories of the parent board.</summary>
    Task SetCardCategoriesAsync(long cardId, SetCardCategoriesRequest request);
}
