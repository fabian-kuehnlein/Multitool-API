using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Kanban;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class CardRepositoryTests : RepositoryTestBase
{
    private readonly CardRepository _sut;
    private int _ownerId;
    private int _memberId;

    public CardRepositoryTests()
    {
        _sut = new CardRepository(Context);
        SetupUsers();
    }

    private void SetupUsers()
    {
        var owner = BoardTestData.CreateUser("owner");
        var member = BoardTestData.CreateUser("member");

        Context.Users.AddRange(owner, member);
        Context.SaveChanges();

        _ownerId = owner.Id;
        _memberId = member.Id;
    }

    private async Task<Board> CreateBoardWithColumnsAsync(int columnCount = 2, int? ownerId = null)
    {
        var boardOwnerId = ownerId ?? _ownerId;
        var owner = await Context.Users.FindAsync(boardOwnerId) ?? throw new InvalidOperationException();
        var member = await Context.Users.FindAsync(_memberId) ?? throw new InvalidOperationException();

        var board = BoardTestData.CreateBoardWithTodoDoneColumns(boardOwnerId);
        board.Columns.Clear();
        board.Columns.AddRange(BoardTestData.CreateNumberedColumns(columnCount));
        board.Members.Add(owner);
        board.Members.Add(member);

        Context.Boards.Add(board);
        await Context.SaveChangesAsync();
        return board;
    }

    // GetCardForOperationAsync

    [Fact]
    public async Task GetCardForOperationAsync_WhenCardExists_ReturnsCardWithBoardAndMembers()
    {
        // Arrange
        var board = await CreateBoardWithColumnsAsync();
        var column = board.Columns.First();
        var card = CardTestData.PersistableCard(column.Id, "Card", 0, _ownerId);
        Context.Cards.Add(card);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCardForOperationAsync(card.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.Column.Board);
        Assert.Equal(2, result.Column.Board.Members.Count);
    }

    [Fact]
    public async Task GetCardForOperationAsync_WhenCategoryBelongsToBoardOwner_ReturnsCategoriesDespiteUserFilter()
    {
        // Arrange
        var board = await CreateBoardWithColumnsAsync(ownerId: _memberId);
        var column = board.Columns.First();
        var category = CategoryTestData.CreateKanbanCategory("Owner category");
        category.UserId = _memberId;
        Context.Categories.Add(category);

        var card = CardTestData.PersistableCard(column.Id, "Card", 0, _memberId);
        card.Categories.Add(category);
        Context.Cards.Add(card);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCardForOperationAsync(card.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!.Categories);
        AssertEx.AreEqual(category.Id, result.Categories[0].Id);
    }

    // MoveCardAsync

    [Fact]
    public async Task MoveCardAsync_WhenMovingToAnotherColumn_ClosesGapAndReordersTarget()
    {
        // Arrange
        var board = await CreateBoardWithColumnsAsync();
        var colA = board.Columns.OrderBy(c => c.Position).First();
        var colB = board.Columns.OrderBy(c => c.Position).Last();

        var a1 = CardTestData.PersistableCard(colA.Id, "A1", 0, _ownerId);
        var a2 = CardTestData.PersistableCard(colA.Id, "A2", 1, _ownerId);
        var a3 = CardTestData.PersistableCard(colA.Id, "A3", 2, _ownerId);
        var b1 = CardTestData.PersistableCard(colB.Id, "B1", 0, _ownerId);
        var b2 = CardTestData.PersistableCard(colB.Id, "B2", 1, _ownerId);
        Context.Cards.AddRange(a1, a2, a3, b1, b2);
        await Context.SaveChangesAsync();

        var trackedA2 = await Context.Cards.FirstAsync(c => c.Id == a2.Id);

        // Act
        await _sut.MoveCardAsync(trackedA2, colB.Id, 1);

        // Assert
        var colACards = await Context.Cards.AsNoTracking()
            .Where(c => c.ColumnId == colA.Id)
            .OrderBy(c => c.Position)
            .ToListAsync();
        var colBCards = await Context.Cards.AsNoTracking()
            .Where(c => c.ColumnId == colB.Id)
            .OrderBy(c => c.Position)
            .ToListAsync();

        AssertEx.AreEqual(new[] { a1.Id, a3.Id }, colACards.Select(c => c.Id).ToArray());
        AssertEx.AreEqual(new[] { 0, 1 }, colACards.Select(c => c.Position).ToArray());

        AssertEx.AreEqual(new[] { b1.Id, a2.Id, b2.Id }, colBCards.Select(c => c.Id).ToArray());
        AssertEx.AreEqual(new[] { 0, 1, 2 }, colBCards.Select(c => c.Position).ToArray());
    }

    [Fact]
    public async Task MoveCardAsync_WhenPositionOutOfRange_ClampsPosition()
    {
        // Arrange
        var board = await CreateBoardWithColumnsAsync();
        var colA = board.Columns.OrderBy(c => c.Position).First();
        var colB = board.Columns.OrderBy(c => c.Position).Last();

        var a1 = CardTestData.PersistableCard(colA.Id, "A1", 0, _ownerId);
        var b1 = CardTestData.PersistableCard(colB.Id, "B1", 0, _ownerId);
        Context.Cards.AddRange(a1, b1);
        await Context.SaveChangesAsync();

        var trackedA1 = await Context.Cards.FirstAsync(c => c.Id == a1.Id);

        // Act
        await _sut.MoveCardAsync(trackedA1, colB.Id, 99);

        // Assert
        var colBCards = await Context.Cards.AsNoTracking()
            .Where(c => c.ColumnId == colB.Id)
            .OrderBy(c => c.Position)
            .ToListAsync();

        AssertEx.AreEqual(new[] { b1.Id, a1.Id }, colBCards.Select(c => c.Id).ToArray());
        AssertEx.AreEqual(new[] { 0, 1 }, colBCards.Select(c => c.Position).ToArray());
    }

    // Join write operations (card categories)

    [Fact]
    public async Task CreateCardAsync_WhenCategoriesProvided_WritesJoinRows()
    {
        // Arrange
        var board = await CreateBoardWithColumnsAsync();
        var column = board.Columns.First();
        var category = CategoryTestData.CreateKanbanCategory("Kanban");
        category.UserId = _ownerId;
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var trackedCategory = await Context.Categories.FirstAsync(c => c.Id == category.Id);
        var card = CardTestData.PersistableCard(column.Id, "Card", 0, _ownerId);

        // Act
        await _sut.CreateCardAsync(card, [trackedCategory]);

        // Assert
        var reloaded = await Context.Cards
            .Include(c => c.Categories)
            .AsNoTracking()
            .FirstAsync(c => c.Id == card.Id);
        Assert.Single(reloaded.Categories);
        AssertEx.AreEqual(category.Id, reloaded.Categories[0].Id);
    }

    [Fact]
    public async Task ReplaceCardCategoriesAsync_WhenCalled_ClearsAndAddsJoinRows()
    {
        // Arrange
        var board = await CreateBoardWithColumnsAsync();
        var column = board.Columns.First();
        var cat1 = CategoryTestData.CreateKanbanCategory("Cat1");
        var cat2 = CategoryTestData.CreateKanbanCategory("Cat2");
        var cat3 = CategoryTestData.CreateKanbanCategory("Cat3");
        cat1.UserId = _ownerId;
        cat2.UserId = _ownerId;
        cat3.UserId = _ownerId;
        Context.Categories.AddRange(cat1, cat2, cat3);
        await Context.SaveChangesAsync();

        var trackedCat1 = await Context.Categories.FirstAsync(c => c.Id == cat1.Id);
        var trackedCat2 = await Context.Categories.FirstAsync(c => c.Id == cat2.Id);
        var trackedCat3 = await Context.Categories.FirstAsync(c => c.Id == cat3.Id);

        var card = CardTestData.PersistableCard(column.Id, "Card", 0, _ownerId);
        await _sut.CreateCardAsync(card, [trackedCat1, trackedCat2]);

        var trackedCard = await Context.Cards.Include(c => c.Categories).FirstAsync(c => c.Id == card.Id);
        trackedCat3 = await Context.Categories.FirstAsync(c => c.Id == cat3.Id);

        // Act
        await _sut.ReplaceCardCategoriesAsync(trackedCard, [trackedCat3]);

        // Assert
        var reloaded = await Context.Cards
            .Include(c => c.Categories)
            .AsNoTracking()
            .FirstAsync(c => c.Id == card.Id);
        Assert.Single(reloaded.Categories);
        AssertEx.AreEqual(cat3.Id, reloaded.Categories[0].Id);
    }
}
