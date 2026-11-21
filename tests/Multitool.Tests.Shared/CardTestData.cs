using Multitool.Application.Models.Kanban;
using Multitool.Domain.Entities.Kanban;

namespace Multitool.Tests.Shared;

public static class CardTestData
{
    public static Card DefaultCard => new()
    {
        Id = 30,
        ColumnId = BoardTestData.DefaultColumn.Id,
        Title = "Card",
        Description = "Card description",
        Position = 0,
        CreatedByUserId = BoardTestData.DefaultMember.Id,
        CreatedAt = BoardTestData.DefaultCreatedAt,
        Categories = [CategoryTestData.KanbanCategory]
    };

    public static Card CardWithColumn
    {
        get
        {
            var card = DefaultCard;
            card.Column = BoardTestData.DefaultColumnWithBoard;
            return card;
        }
    }

    public static CardModel DefaultCardModel => new(
        DefaultCard.Id,
        DefaultCard.ColumnId,
        DefaultCard.Title,
        DefaultCard.Description,
        DefaultCard.Position,
        DefaultCard.CreatedByUserId,
        DefaultCard.CreatedAt,
        DefaultCard.Categories.Select(c => c.Id).ToList());

    public static Card PersistableCard(long columnId, string title, int position, int createdByUserId) => new()
    {
        ColumnId = columnId,
        Title = title,
        Description = "Card description",
        Position = position,
        CreatedByUserId = createdByUserId,
        CreatedAt = BoardTestData.DefaultCreatedAt
    };
}
