using Multitool.Application.Models.Kanban;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Entities.Kanban;

namespace Multitool.Tests.Shared;

public static class BoardTestData
{
    public static DateTime DefaultCreatedAt => new(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc);

    public static User DefaultMember => new()
    {
        Id = 1,
        Username = "user1",
        PasswordHash = "hash",
        Role = AuthTestData.DefaultUser.Role,
        IsActive = true,
        CreatedAt = DefaultCreatedAt
    };

    public static User OtherMember => new()
    {
        Id = 2,
        Username = "user2",
        PasswordHash = "hash",
        Role = AuthTestData.DefaultUser.Role,
        IsActive = true,
        CreatedAt = DefaultCreatedAt
    };

    public static Board DefaultBoard => new()
    {
        Id = 10,
        Name = "Board",
        Description = "Description",
        OwnerId = DefaultMember.Id,
        CreatedAt = DefaultCreatedAt,
        Members = [DefaultMember]
    };

    public static Board BoardWithTwoMembers => new()
    {
        Id = 10,
        Name = "Board",
        Description = "Description",
        OwnerId = DefaultMember.Id,
        CreatedAt = DefaultCreatedAt,
        Members = [DefaultMember, OtherMember]
    };

    public static Board BoardWithOtherUserOnly => new()
    {
        Id = 10,
        Name = "Board",
        Description = "Description",
        OwnerId = DefaultMember.Id,
        CreatedAt = DefaultCreatedAt,
        Members = [OtherMember]
    };

    public static Board OtherOwnerBoard => new()
    {
        Id = 10,
        Name = "Board",
        Description = "Description",
        OwnerId = OtherMember.Id,
        CreatedAt = DefaultCreatedAt,
        Members = [DefaultMember, OtherMember]
    };

    public static BoardColumn DefaultColumn => new()
    {
        Id = 20,
        BoardId = 10,
        Name = "Column",
        Position = 0
    };

    public static BoardColumn FirstColumn => new()
    {
        Id = 20,
        BoardId = 10,
        Name = "First",
        Position = 1
    };

    public static BoardColumn SecondColumn => new()
    {
        Id = 21,
        BoardId = 10,
        Name = "Second",
        Position = 4
    };

    public static BoardColumn DefaultColumnWithBoard
    {
        get
        {
            var column = DefaultColumn;
            column.Board = DefaultBoard;
            return column;
        }
    }

    public static Board DefaultBoardWithDetails => WithDetails(DefaultBoard);

    public static Board OtherOwnerBoardWithDetails => WithDetails(OtherOwnerBoard);

    public static List<BoardColumn> DefaultColumns =>
    [
        new() { Name = "Backlog", Position = 0 },
        new() { Name = "In Arbeit", Position = 1 },
        new() { Name = "Fertig", Position = 2 }
    ];

    public static List<BoardColumn> TodoDoneColumns =>
    [
        new() { Name = "Todo", Position = 0 },
        new() { Name = "Done", Position = 1 }
    ];

    public static Board CreateBoardWithTodoDoneColumns(int ownerId, string name = "Board") => new()
    {
        Name = name,
        Description = "Description",
        OwnerId = ownerId,
        CreatedAt = DefaultCreatedAt,
        Columns = TodoDoneColumns
    };

    public static List<BoardColumn> CreateNumberedColumns(int count) =>
        Enumerable.Range(0, count)
            .Select(i => new BoardColumn { Name = $"Col {i}", Position = i })
            .ToList();

    public static User CreateUser(string username) => new()
    {
        Username = username,
        PasswordHash = "hash",
        Role = AuthTestData.DefaultUser.Role,
        IsActive = true,
        CreatedAt = DefaultCreatedAt
    };

    public static BoardMemberModel DefaultBoardMemberModel => new(DefaultMember.Id, DefaultMember.Username);

    public static BoardOverviewModel DefaultBoardOverviewModel => new(
        DefaultBoard.Id,
        DefaultBoard.Name,
        DefaultBoard.CreatedAt,
        false,
        1);

    public static BoardModel DefaultBoardModel => new(
        DefaultBoard.Id,
        DefaultBoard.Name,
        DefaultBoard.Description,
        true,
        [new ColumnModel(DefaultColumn.Id, DefaultColumn.Name, DefaultColumn.Position, [CardTestData.DefaultCardModel])],
        [DefaultBoardMemberModel]);

    public static CreateBoardRequest DefaultCreateBoardRequest => new("New board", "New description", null);

    public static CreateBoardRequest DefaultCreateBoardRequestWithColumns => new("New board", null, ["Todo", "Done"]);

    public static UpdateBoardRequest DefaultUpdateBoardRequest => new("Renamed", "New description");

    private static Board WithDetails(Board board)
    {
        var column = DefaultColumnWithBoard;
        column.Board = board;

        var card = CardTestData.DefaultCard;
        card.Column = column;
        column.Cards.Add(card);
        board.Columns.Add(column);

        return board;
    }
}
