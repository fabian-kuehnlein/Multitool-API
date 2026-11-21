using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Config;
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
        var owner = new User
        {
            Username = "owner",
            PasswordHash = "hash",
            Role = AuthTestData.DefaultUser.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var member = new User
        {
            Username = "member",
            PasswordHash = "hash",
            Role = AuthTestData.DefaultUser.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var other = new User
        {
            Username = "other",
            PasswordHash = "hash",
            Role = AuthTestData.DefaultUser.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        Context.Users.AddRange(owner, member, other);
        Context.SaveChanges();

        _ownerId = owner.Id;
        _memberId = member.Id;
        _otherUserId = other.Id;
    }

    private Board CreateBoard(string name = "Board")
    {
        return new Board
        {
            Name = name,
            Description = "desc",
            OwnerId = _ownerId,
            CreatedAt = DateTime.UtcNow,
            Columns =
            [
                new BoardColumn { Name = "Todo", Position = 0 },
                new BoardColumn { Name = "Done", Position = 1 }
            ]
        };
    }

    private async Task<(Board Board, Card Card)> CreateBoardWithCardAsync()
    {
        var board = CreateBoard();
        board.Members.Add(await Context.Users.FindAsync(_ownerId) ?? throw new InvalidOperationException("Owner not found"));
        board.Members.Add(await Context.Users.FindAsync(_memberId) ?? throw new InvalidOperationException("Member not found"));
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        var column = board.Columns.OrderBy(c => c.Position).First();
        var card = new Card
        {
            ColumnId = column.Id,
            Title = "Card",
            Description = "d",
            Position = 0,
            CreatedByUserId = _ownerId,
            CreatedAt = DateTime.UtcNow
        };
        Context.Cards.Add(card);
        await Context.SaveChangesAsync();

        return (board, card);
    }

    // GetBoardOverviewAsync

    [Fact]
    public async Task GetBoardOverviewAsync_WhenUserIsMember_ReturnsOverviewBoards()
    {
        // Arrange
        var board = CreateBoard();
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
        AssertEx.AreEqual(board.CreatedAt, result[0].CreatedAt);
        AssertEx.AreEqual(2, result[0].Members.Count);
    }

    [Fact]
    public async Task GetBoardOverviewAsync_WhenUserIsNotMember_ReturnsEmptyList()
    {
        // Arrange
        var board = CreateBoard();
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

    // AddBoardMemberAsync / RemoveBoardMemberAsync

    [Fact]
    public async Task AddBoardMemberAsync_WhenUserAdded_AddsJoinRow()
    {
        // Arrange
        var board = CreateBoard();
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
        var board = CreateBoard();
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

    // UpdateColumnOrderAsync

    [Fact]
    public async Task UpdateColumnOrderAsync_WhenAllColumnsExist_UpdatesAllPositions()
    {
        // Arrange
        var board = CreateBoard();
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
        var board = CreateBoard();
        var otherBoard = CreateBoard("Other");
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
        var board = CreateBoard("New board");

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
        var board = CreateBoard("Delete");
        Context.Boards.Add(board);
        await Context.SaveChangesAsync();

        // Act
        await _sut.DeleteBoardAsync(board);

        // Assert
        var dbBoard = await Context.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == board.Id);
        Assert.Null(dbBoard);
    }
}
