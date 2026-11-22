using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Entities.Kanban;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class BoardRepositoryTests : RepositoryTestBase
{
    private readonly BoardRepository _sut;
    private int _ownerId;
    private int _memberId;
    private int _otherUserId;

    public BoardRepositoryTests()
    {
        _sut = new BoardRepository(Context);
        SetupUsers();
    }

    private void SetupUsers()
    {
        var owner = BoardTestData.CreateUser("owner");
        var member = BoardTestData.CreateUser("member");
        var other = BoardTestData.CreateUser("other");

        Context.Users.AddRange(owner, member, other);
        Context.SaveChanges();

        _ownerId = owner.Id;
        _memberId = member.Id;
        _otherUserId = other.Id;
    }

    private async Task<(Board Board, Card Card)> CreateBoardWithCardAsync()
    {
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId);
        board.Members.Add(await Context.Users.FindAsync(_ownerId) ?? throw new InvalidOperationException("Owner not found"));
        board.Members.Add(await Context.Users.FindAsync(_memberId) ?? throw new InvalidOperationException("Member not found"));
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        var column = board.Columns.OrderBy(c => c.Position).First();
        var card = CardTestData.PersistableCard(column.Id, "Card", 0, _ownerId);
        card.Description = "d";
        Context.Cards.Add(card);
        await Context.SaveChangesAsync();

        return (board, card);
    }

    private async Task<(Board Board, Category Eligible, Category Second, Category NonKanban, Category Deleted)>
        CreateBoardWithCategoriesAsync(int ownerId, bool whitelistCategories = true)
    {
        var owner = await Context.Users.FindAsync(ownerId) ?? throw new InvalidOperationException("Owner not found");
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(ownerId);
        board.Members.Add(owner);

        var eligible = CategoryTestData.CreateKanbanCategory("Eligible");
        var second = CategoryTestData.CreateKanbanCategory("Second");
        var nonKanban = CategoryTestData.DefaultCategory;
        nonKanban.Id = 0;
        var deleted = CategoryTestData.DeletedKanbanCategory;
        deleted.Id = 0;
        eligible.UserId = ownerId;
        second.UserId = ownerId;
        nonKanban.UserId = ownerId;
        deleted.UserId = ownerId;

        Context.Categories.AddRange(eligible, second, nonKanban, deleted);
        if (whitelistCategories)
        {
            board.Categories.AddRange([eligible, second, nonKanban, deleted]);
        }

        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        return (board, eligible, second, nonKanban, deleted);
    }

    // GetBoardOverviewAsync

    [Fact]
    public async Task GetBoardOverviewAsync_WhenUserIsMember_ReturnsOverviewBoards()
    {
        // Arrange
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId);
        board.Members.Add(await Context.Users.FindAsync(_ownerId) ?? throw new InvalidOperationException());
        board.Members.Add(await Context.Users.FindAsync(_memberId) ?? throw new InvalidOperationException());
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetBoardOverviewAsync(_memberId);

        // Assert
        Assert.Single(result);
        AssertEx.AreEqual(board.Id, result[0].Id);
        AssertEx.AreEqual(board.Name, result[0].Name);
        AssertEx.AreEqual(board.Description, result[0].Description);
        AssertEx.AreEqual(board.CreatedAt, result[0].CreatedAt);
        AssertEx.AreEqual(2, result[0].Members.Count);
    }

    [Fact]
    public async Task GetBoardOverviewAsync_WhenUserIsNotMember_ReturnsEmptyList()
    {
        // Arrange
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId);
        board.Members.Add(await Context.Users.FindAsync(_ownerId) ?? throw new InvalidOperationException());
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetBoardOverviewAsync(_otherUserId);

        // Assert
        Assert.Empty(result);
    }

    // GetBoardWithDetailsAsync

    [Fact]
    public async Task GetBoardWithDetailsAsync_WhenUserIsMember_ReturnsBoardWithDetails()
    {
        // Arrange
        var (board, card) = await CreateBoardWithCardAsync();

        // Act
        var result = await _sut.GetBoardWithDetailsAsync(board.Id, _memberId);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(board.Id, result!.Id);
        Assert.Equal(2, result.Members.Count);
        Assert.Equal(2, result.Columns.Count);
        Assert.Single(result.Columns.OrderBy(c => c.Position).First().Cards);
    }

    [Fact]
    public async Task GetBoardWithDetailsAsync_WhenUserIsNotMember_ReturnsNull()
    {
        // Arrange
        var (board, _) = await CreateBoardWithCardAsync();

        // Act
        var result = await _sut.GetBoardWithDetailsAsync(board.Id, _otherUserId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoardWithDetailsAsync_WhenUserIsMember_ReturnsOnlyEligibleWhitelistedCategories()
    {
        // Arrange
        var (board, eligible, second, nonKanban, deleted) = await CreateBoardWithCategoriesAsync(_ownerId);
        var member = await Context.Users.FindAsync(_memberId) ?? throw new InvalidOperationException("Member not found");
        board.Members.Add(member);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetBoardWithDetailsAsync(board.Id, _memberId);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(2, result!.Categories.Count);
        Assert.Contains(result.Categories, c => c.Id == eligible.Id);
        Assert.Contains(result.Categories, c => c.Id == second.Id);
        Assert.DoesNotContain(result.Categories, c => c.Id == nonKanban.Id);
        Assert.DoesNotContain(result.Categories, c => c.Id == deleted.Id);
        AssertEx.AreEqual(eligible.Name, result.Categories.Single(c => c.Id == eligible.Id).Name);
        AssertEx.AreEqual(eligible.Color, result.Categories.Single(c => c.Id == eligible.Id).Color);
    }

    // GetBoardCategoriesForValidationAsync

    [Fact]
    public async Task GetBoardCategoriesForValidationAsync_WhenOwnerIsAnotherUser_ReturnsOnlyEligibleWhitelistedCategories()
    {
        // Arrange
        var (board, eligible, second, nonKanban, deleted) = await CreateBoardWithCategoriesAsync(_otherUserId);
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetBoardCategoriesForValidationAsync(board.Id);

        // Assert
        AssertEx.AreEqual(2, result.Count);
        Assert.Contains(result, c => c.Id == eligible.Id);
        Assert.Contains(result, c => c.Id == second.Id);
        Assert.DoesNotContain(result, c => c.Id == nonKanban.Id);
        Assert.DoesNotContain(result, c => c.Id == deleted.Id);
        Assert.All(result, c => Assert.Equal(EntityState.Unchanged, Context.Entry(c).State));
    }

    [Fact]
    public async Task GetBoardCategoriesForValidationAsync_WhenBoardDoesNotExist_ReturnsEmptyList()
    {
        // Arrange

        // Act
        var result = await _sut.GetBoardCategoriesForValidationAsync(999);

        // Assert
        Assert.Empty(result);
    }

    // GetBoardForAccessCheckAsync

    [Fact]
    public async Task GetBoardForAccessCheckAsync_WhenBoardHasWhitelist_LoadsTrackedCategories()
    {
        // Arrange
        var (board, eligible, second, nonKanban, deleted) = await CreateBoardWithCategoriesAsync(_ownerId);
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetBoardForAccessCheckAsync(board.Id);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(4, result!.Categories.Count);
        Assert.Contains(result.Categories, c => c.Id == eligible.Id);
        Assert.Contains(result.Categories, c => c.Id == second.Id);
        Assert.Contains(result.Categories, c => c.Id == nonKanban.Id);
        Assert.Contains(result.Categories, c => c.Id == deleted.Id);
        Assert.All(result.Categories, c => Assert.Equal(EntityState.Unchanged, Context.Entry(c).State));
    }

    // AddBoardMemberAsync / RemoveBoardMemberAsync

    [Fact]
    public async Task AddBoardMemberAsync_WhenUserAdded_AddsJoinRow()
    {
        // Arrange
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId);
        board.Members.Add(await Context.Users.FindAsync(_ownerId) ?? throw new InvalidOperationException());
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        var trackedBoard = await Context.Boards.Include(b => b.Members).FirstAsync(b => b.Id == board.Id);
        var user = await Context.Users.FindAsync(_memberId) ?? throw new InvalidOperationException();

        // Act
        await _sut.AddBoardMemberAsync(trackedBoard, user);

        // Assert
        var reloaded = await Context.Boards
            .Include(b => b.Members)
            .AsNoTracking()
            .FirstAsync(b => b.Id == board.Id);
        Assert.Contains(reloaded.Members, u => u.Id == _memberId);
    }

    [Fact]
    public async Task RemoveBoardMemberAsync_WhenUserRemoved_RemovesJoinRow()
    {
        // Arrange
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId);
        board.Members.Add(await Context.Users.FindAsync(_ownerId) ?? throw new InvalidOperationException());
        board.Members.Add(await Context.Users.FindAsync(_memberId) ?? throw new InvalidOperationException());
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        var trackedBoard = await Context.Boards.Include(b => b.Members).FirstAsync(b => b.Id == board.Id);
        var user = trackedBoard.Members.First(u => u.Id == _memberId);

        // Act
        await _sut.RemoveBoardMemberAsync(trackedBoard, user);

        // Assert
        var reloaded = await Context.Boards
            .Include(b => b.Members)
            .AsNoTracking()
            .FirstAsync(b => b.Id == board.Id);
        Assert.DoesNotContain(reloaded.Members, u => u.Id == _memberId);
    }

    // AddBoardCategoryAsync / RemoveBoardCategoryAsync

    [Fact]
    public async Task AddBoardCategoryAsync_WhenCategoryIsNotWhitelisted_WritesJoinRow()
    {
        // Arrange
        var (board, eligible, _, _, _) = await CreateBoardWithCategoriesAsync(_ownerId, whitelistCategories: false);
        var trackedBoard = await Context.Boards
            .Include(b => b.Categories)
            .FirstAsync(b => b.Id == board.Id);
        var trackedCategory = await Context.Categories.FirstAsync(c => c.Id == eligible.Id);

        // Act
        await _sut.AddBoardCategoryAsync(trackedBoard, trackedCategory);

        // Assert
        Context.ChangeTracker.Clear();
        var reloaded = await Context.Boards
            .Include(b => b.Categories)
            .AsNoTracking()
            .FirstAsync(b => b.Id == board.Id);
        Assert.Single(reloaded.Categories);
        AssertEx.AreEqual(eligible.Id, reloaded.Categories[0].Id);
    }

    [Fact]
    public async Task RemoveBoardCategoryAsync_WhenCategoryIsWhitelisted_RemovesJoinRowAndKeepsCardTags()
    {
        // Arrange
        var (board, eligible, second, nonKanban, deleted) = await CreateBoardWithCategoriesAsync(_ownerId);
        var column = board.Columns.First();
        var card = CardTestData.PersistableCard(column.Id, "Tagged card", 0, _ownerId);
        card.Categories.Add(eligible);
        Context.Cards.Add(card);
        await Context.SaveChangesAsync();

        var trackedBoard = await Context.Boards
            .Include(b => b.Categories)
            .FirstAsync(b => b.Id == board.Id);
        var trackedCategory = await Context.Categories.FirstAsync(c => c.Id == eligible.Id);

        // Act
        await _sut.RemoveBoardCategoryAsync(trackedBoard, trackedCategory);

        // Assert
        Context.ChangeTracker.Clear();
        var reloadedBoard = await Context.Boards
            .Include(b => b.Categories)
            .AsNoTracking()
            .FirstAsync(b => b.Id == board.Id);
        AssertEx.AreEqual(3, reloadedBoard.Categories.Count);
        Assert.DoesNotContain(reloadedBoard.Categories, c => c.Id == eligible.Id);
        Assert.Contains(reloadedBoard.Categories, c => c.Id == second.Id);
        Assert.Contains(reloadedBoard.Categories, c => c.Id == nonKanban.Id);
        Assert.Contains(reloadedBoard.Categories, c => c.Id == deleted.Id);

        var reloadedCard = await Context.Cards
            .Include(c => c.Categories)
            .AsNoTracking()
            .FirstAsync(c => c.Id == card.Id);
        Assert.Single(reloadedCard.Categories);
        AssertEx.AreEqual(eligible.Id, reloadedCard.Categories[0].Id);
    }

    // UpdateColumnOrderAsync

    [Fact]
    public async Task UpdateColumnOrderAsync_WhenAllColumnsExist_UpdatesAllPositions()
    {
        // Arrange
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId);
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();
        var first = board.Columns[0];
        var second = board.Columns[1];
        var positions = new Dictionary<long, int>
        {
            [first.Id] = 2,
            [second.Id] = 0
        };

        // Act
        var result = await _sut.UpdateColumnOrderAsync(board.Id, positions);

        // Assert
        Assert.True(result);
        Context.ChangeTracker.Clear();
        var saved = await Context.BoardColumns.AsNoTracking()
            .Where(c => c.BoardId == board.Id)
            .ToListAsync();
        AssertEx.AreEqual(2, saved.Count);
        AssertEx.AreEqual(2, saved.Single(c => c.Id == first.Id).Position);
        AssertEx.AreEqual(0, saved.Single(c => c.Id == second.Id).Position);
    }

    [Fact]
    public async Task UpdateColumnOrderAsync_WhenColumnBelongsToAnotherBoard_ReturnsFalseWithoutSaving()
    {
        // Arrange
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId);
        var otherBoard = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId, "Other");
        Context.Boards.AddRange(board, otherBoard);
        await Context.SaveChangesAsync();
        var ownColumn = board.Columns[0];
        var foreignColumn = otherBoard.Columns[0];
        var positions = new Dictionary<long, int>
        {
            [ownColumn.Id] = 9,
            [foreignColumn.Id] = 9
        };

        // Act
        var result = await _sut.UpdateColumnOrderAsync(board.Id, positions);

        // Assert
        Assert.False(result);
        Context.ChangeTracker.Clear();
        var savedOwn = await Context.BoardColumns.AsNoTracking().FirstAsync(c => c.Id == ownColumn.Id);
        var savedForeign = await Context.BoardColumns.AsNoTracking().FirstAsync(c => c.Id == foreignColumn.Id);
        AssertEx.AreEqual(0, savedOwn.Position);
        AssertEx.AreEqual(0, savedForeign.Position);
    }

    // CRUD basics

    [Fact]
    public async Task CreateBoardAsync_WhenBoardIsValid_AddsBoard()
    {
        // Arrange
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId, "New board");

        // Act
        var id = await _sut.CreateBoardAsync(board);

        // Assert
        Assert.True(id > 0);
        var dbBoard = await Context.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        Assert.NotNull(dbBoard);
        AssertEx.AreEqual("New board", dbBoard!.Name);
    }

    [Fact]
    public async Task DeleteBoardAsync_WhenBoardExists_RemovesBoard()
    {
        // Arrange
        var board = BoardTestData.CreateBoardWithTodoDoneColumns(_ownerId, "Delete");
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        // Act
        await _sut.DeleteBoardAsync(board);

        // Assert
        var dbBoard = await Context.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == board.Id);
        Assert.Null(dbBoard);
    }
}
