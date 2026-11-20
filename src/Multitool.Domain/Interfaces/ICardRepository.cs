using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Entities.Kanban;

namespace Multitool.Domain.Interfaces;

public interface ICardRepository
{
    Task<Card?> GetCardForOperationAsync(long cardId);
    Task<Card?> GetCardByIdAsync(long cardId);
    Task<long> CreateCardAsync(Card card, IReadOnlyList<Category> categories);
    Task UpdateCardAsync(Card card);
    Task DeleteCardAsync(Card card);
    Task MoveCardAsync(Card card, long targetColumnId, int position);
    Task ReplaceCardCategoriesAsync(Card card, IReadOnlyList<Category> categories);
}
