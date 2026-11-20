using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

/// <summary>
/// Repository for kanban cards.
/// Method categories:
/// - Tracked entity: regular EF tracking for business operations and updates.
/// - Access/ownership/context query: tracked entity with required includes for authorization and card-scoped operations.
/// </summary>
public class CardRepository(AppDbContext db) : ICardRepository
{
    /// <summary>
    /// Access/ownership/context query for card-scoped operations addressed by card id.
    /// Loads a tracked card including Column.Board.Members, Column.Board.Columns, and Categories.
    /// Use when the caller only knows the card id (e.g. /cards/{id} endpoints): Board.Members is required to
    /// authorize access via the parent board, Board.Columns is required to validate move targets within the same
    /// board, and Categories is required because card operations read/mutate card categories (e.g. replace requires
    /// a loaded collection). Not replaceable by GetBoardWithDetailsAsync because the board id is unknown up front
    /// and a tracked, focused card context is needed instead of an untracked full-board read projection.
    /// </summary>
    public async Task<Card?> GetCardForOperationAsync(long cardId)
    {
        return await db.Cards
            .Include(c => c.Column)
                .ThenInclude(col => col.Board)
                    .ThenInclude(b => b.Members)
            .Include(c => c.Column)
                .ThenInclude(col => col.Board)
                    .ThenInclude(b => b.Columns)
            .Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Id == cardId);
    }

    /// <summary>
    /// Default tracked entity load for a single card.
    /// Use for simple card operations when the required access/context data is already available.
    /// </summary>
    public async Task<Card?> GetCardByIdAsync(long cardId)
    {
        return await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
    }

    public async Task<long> CreateCardAsync(Card card, IReadOnlyList<Category> categories)
    {
        card.Categories.Clear();
        foreach (var category in categories)
            card.Categories.Add(category);

        db.Cards.Add(card);
        await db.SaveChangesAsync();

        return card.Id;
    }

    public async Task UpdateCardAsync(Card card)
    {
        db.Cards.Update(card);
        await db.SaveChangesAsync();
    }

    public async Task DeleteCardAsync(Card card)
    {
        db.Cards.Remove(card);
        await db.SaveChangesAsync();
    }

    public async Task MoveCardAsync(Card card, long targetColumnId, int position)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var sourceColumnId = card.ColumnId;
        var sourceCards = await db.Cards
            .Where(c => c.ColumnId == sourceColumnId && c.Id != card.Id)
            .OrderBy(c => c.Position)
            .ToListAsync();

        for (var i = 0; i < sourceCards.Count; i++)
            sourceCards[i].Position = i;

        var targetCards = sourceColumnId == targetColumnId
            ? sourceCards
            : await db.Cards
                .Where(c => c.ColumnId == targetColumnId)
                .OrderBy(c => c.Position)
                .ToListAsync();

        var clampedPosition = Math.Clamp(position, 0, targetCards.Count);
        card.ColumnId = targetColumnId;
        targetCards.Insert(clampedPosition, card);

        for (var i = 0; i < targetCards.Count; i++)
            targetCards[i].Position = i;

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task ReplaceCardCategoriesAsync(Card card, IReadOnlyList<Category> categories)
    {
        card.Categories.Clear();
        foreach (var category in categories)
            card.Categories.Add(category);

        await db.SaveChangesAsync();
    }
}
