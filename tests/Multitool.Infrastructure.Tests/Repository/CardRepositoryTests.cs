using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Entities.Kanban;
using Multitool.Domain.Enums;
using Multitool.Infrastructure.Repositories;
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
        var owner = new User
        {
            Username = "owner",
            PasswordHash = "hash",
            Role = Multitool.Tests.Shared.AuthTestData.DefaultUser.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var member = new User
        {
            Username = "member",
            PasswordHash = "hash",
            Role = Multitool.Tests.Shared.AuthTestData.DefaultUser.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        Context.Users.AddRange(owner, member);
        Context.SaveChanges();

        _ownerId = owner.Id;
        _memberId = member.Id;
    }

    private async Task<Board> CreateBoardWithColumnsAsync(int columnCount = 2)
    {
        var owner = await Context.Users.FindAsync(_ownerId) ?? throw new InvalidOperationException();
        var member = await Context.Users.FindAsync(_memberId) ?? throw new InvalidOperationException();

        var board = new Board
        {
            Name = "Board",
            OwnerId = _ownerId,
            CreatedAt = DateTime.UtcNow,
            Columns = Enumerable.Range(0, columnCount)
                .Select(i => new BoardColumn { Name = $"Col {i}", Position = i })
                .ToList(),
            Members = [owner, member]
        };

        Context.Boards.Add(board);
        await Context.SaveChangesAsync();
        return board;
    }

    private async Task<Card> CreateCardAsync(long columnId, string title, int position, List<Category>? categories = null)
    {
        var card = new Card
        {
            ColumnId = columnId,
            Title = title,
            Position = position,
            CreatedByUserId = _ownerId,
            CreatedAt = DateTime.UtcNow
        };

        if (categories is null || categories.Count == 0)
        {
            Context.Cards.Add(card);
            await Context.SaveChangesAsync();
            return card;
        }

        await _sut.CreateCardAsync(card, categories);
        return card;
    }

    private Category CreateCategory(string name)
    {
        return new Category
        {
            UserId = _ownerId,
            Name = name,
            Color = "#FF0000",
            ApplicableModules = [AppModule.Kanban],
            IsDeleted = false
        };
    }

    // GetCardForOperationAsync

    [Fact]
    public async Task GetCardForOperationAsync_WhenCardExists_ReturnsCardWithBoardAndMembers()
    {
        // Arrange
        var board = await CreateBoardWithColumnsAsync();
        var column = board.Columns.First();
        var card = await CreateCardAsync(column.Id, "Card", 0);

        // Act
        var result = await _sut.GetCardForOperationAsync(card.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.Column.Board);
        Assert.Equal(2, result.Column.Board.Members.Count);
    }

    // MoveCardAsync

    [Fact]
    public async Task MoveCardAsync_WhenMovingToAnotherColumn_ClosesGapAndReordersTarget()
    {
        // Arrange
        var board = await CreateBoardWithColumnsAsync();
        var colA = board.Columns.OrderBy(c => c.Position).First();
        var colB = board.Columns.OrderBy(c => c.Position).Last();

        var a1 = await CreateCardAsync(colA.Id, "A1", 0);
        var a2 = await CreateCardAsync(colA.Id, "A2", 1);
        var a3 = await CreateCardAsync(colA.Id, "A3", 2);
        var b1 = await CreateCardAsync(colB.Id, "B1", 0);
        var b2 = await CreateCardAsync(colB.Id, "B2", 1);

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

        var a1 = await CreateCardAsync(colA.Id, "A1", 0);
        var b1 = await CreateCardAsync(colB.Id, "B1", 0);

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
        var category = CreateCategory("Kanban");
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var trackedCategory = await Context.Categories.FirstAsync(c => c.Id == category.Id);
        var card = new Card
        {
            ColumnId = column.Id,
            Title = "Card",
            Position = 0,
            CreatedByUserId = _ownerId,
            CreatedAt = DateTime.UtcNow
        };

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
        var cat1 = CreateCategory("Cat1");
        var cat2 = CreateCategory("Cat2");
        var cat3 = CreateCategory("Cat3");
        Context.Categories.AddRange(cat1, cat2, cat3);
        await Context.SaveChangesAsync();

        var trackedCat1 = await Context.Categories.FirstAsync(c => c.Id == cat1.Id);
        var trackedCat2 = await Context.Categories.FirstAsync(c => c.Id == cat2.Id);
        var trackedCat3 = await Context.Categories.FirstAsync(c => c.Id == cat3.Id);

        var card = new Card
        {
            ColumnId = column.Id,
            Title = "Card",
            Position = 0,
            CreatedByUserId = _ownerId,
            CreatedAt = DateTime.UtcNow
        };
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
