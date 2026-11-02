using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Enums;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class AppDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly Mock<IUserContext> _userContextMock;

    private static readonly int USER_ID = AuthTestData.DefaultUser.Id;
    private static readonly int OTHER_USER_ID = AuthTestData.DefaultUser.Id + 1;

    public AppDbContextTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _userContextMock = new Mock<IUserContext>();
        _userContextMock.SetupGet(c => c.UserId).Returns(USER_ID);

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    private AppDbContext CreateContext() => new(_options, _userContextMock.Object);

    public void Dispose()
    {
        _connection.Dispose();
    }

    // SaveChangesAsync

    [Fact]
    public async Task SaveChangesAsync_WhenNewUserEntitiesHaveNoUserId_AssignsCurrentUserId()
    {
        // Arrange
        await using var context = CreateContext();
        var category = new Category
        {
            Name = "Work",
            Color = "#FF0000",
            ApplicableModules = [AppModule.Todo]
        };
        var workDay = new WorkDay
        {
            Date = new DateTime(2026, 1, 1),
            BreakMinutes = 0,
            WorkMinutes = 480,
            OvertimeMinutes = 0,
            IsHomeOffice = false,
            Status = DayStatus.Normal,
            IsLocked = false
        };
        context.Categories.Add(category);
        context.WorkDays.Add(workDay);

        // Act
        await context.SaveChangesAsync();

        // Assert
        AssertEx.AreEqual(USER_ID, category.UserId);
        AssertEx.AreEqual(USER_ID, workDay.UserId);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNewUserEntityHasUserId_KeepsUserId()
    {
        // Arrange
        await using var context = CreateContext();
        var category = new Category
        {
            UserId = OTHER_USER_ID,
            Name = "Other",
            Color = "#00FF00",
            ApplicableModules = [AppModule.Todo]
        };
        context.Categories.Add(category);

        // Act
        await context.SaveChangesAsync();

        // Assert
        AssertEx.AreEqual(OTHER_USER_ID, category.UserId);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNoUserIsAuthenticated_DoesNotAssignUserId()
    {
        // Arrange
        _userContextMock.SetupGet(c => c.UserId).Returns(0);
        await using var context = CreateContext();
        var category = new Category
        {
            Name = "Anonymous",
            Color = "#0000FF",
            ApplicableModules = [AppModule.Todo]
        };
        context.Categories.Add(category);

        // Act
        await context.SaveChangesAsync();

        // Assert
        AssertEx.AreEqual(0, category.UserId);
    }

    // SaveChanges

    [Fact]
    public void SaveChanges_WhenNewUserEntityHasNoUserId_AssignsCurrentUserId()
    {
        // Arrange
        using var context = CreateContext();
        var category = new Category
        {
            Name = "Work",
            Color = "#FF0000",
            ApplicableModules = [AppModule.Todo]
        };
        context.Categories.Add(category);

        // Act
        context.SaveChanges();

        // Assert
        AssertEx.AreEqual(USER_ID, category.UserId);
    }

    // Categories

    [Fact]
    public async Task Categories_WhenOtherUserDataExists_ReturnsOnlyCurrentUserData()
    {
        // Arrange
        await using var context = CreateContext();
        var ownCategory = new Category
        {
            UserId = USER_ID,
            Name = "Own",
            Color = "#FF0000",
            ApplicableModules = [AppModule.Todo]
        };
        var otherCategory = new Category
        {
            UserId = OTHER_USER_ID,
            Name = "Other",
            Color = "#00FF00",
            ApplicableModules = [AppModule.Todo]
        };
        context.Categories.AddRange(ownCategory, otherCategory);
        await context.SaveChangesAsync();

        // Act
        var result = await context.Categories.AsNoTracking().ToListAsync();

        // Assert
        Assert.Single(result);
        AssertEx.AreEqual(ownCategory.Id, result[0].Id);
    }

    // Todos

    [Fact]
    public async Task Todos_WhenOtherUserDataExists_ReturnsOnlyCurrentUserData()
    {
        // Arrange
        await using var context = CreateContext();
        var ownCategory = new Category
        {
            UserId = USER_ID,
            Name = "Own",
            Color = "#FF0000",
            ApplicableModules = [AppModule.Todo]
        };
        var otherCategory = new Category
        {
            UserId = OTHER_USER_ID,
            Name = "Other",
            Color = "#00FF00",
            ApplicableModules = [AppModule.Todo]
        };
        context.Categories.AddRange(ownCategory, otherCategory);
        await context.SaveChangesAsync();

        var ownTodo = new Todo
        {
            UserId = USER_ID,
            Title = "Own",
            CategoryId = ownCategory.Id,
            IsDone = false,
            CreationDateTime = DateTime.Now
        };
        var otherTodo = new Todo
        {
            UserId = OTHER_USER_ID,
            Title = "Other",
            CategoryId = otherCategory.Id,
            IsDone = false,
            CreationDateTime = DateTime.Now
        };
        context.Todos.AddRange(ownTodo, otherTodo);
        await context.SaveChangesAsync();

        // Act
        var result = await context.Todos.AsNoTracking().ToListAsync();

        // Assert
        Assert.Single(result);
        AssertEx.AreEqual(ownTodo.Id, result[0].Id);
    }

    // Custom tables and their children

    [Fact]
    public async Task CustomTablesAndChildren_WhenOtherUserDataExists_ReturnsOnlyCurrentUserData()
    {
        // Arrange
        await using var context = CreateContext();
        var ownTable = new Table
        {
            UserId = USER_ID,
            Name = "Own",
            CreatedAt = DateTime.Now,
            Columns = [new Column { Name = "Own Column", DataType = CustomDataType.String, ColOrder = 0 }],
            Rows = [new Row { CreatedAt = DateTime.Now, RowOrder = 0 }]
        };
        var otherTable = new Table
        {
            UserId = OTHER_USER_ID,
            Name = "Other",
            CreatedAt = DateTime.Now,
            Columns = [new Column { Name = "Other Column", DataType = CustomDataType.String, ColOrder = 0 }],
            Rows = [new Row { CreatedAt = DateTime.Now, RowOrder = 0 }]
        };
        context.CustomTables.AddRange(ownTable, otherTable);
        await context.SaveChangesAsync();

        context.CustomCells.AddRange(
            new Cell { RowId = ownTable.Rows[0].RowId, ColumnId = ownTable.Columns[0].ColumnId, ValString = "own" },
            new Cell { RowId = otherTable.Rows[0].RowId, ColumnId = otherTable.Columns[0].ColumnId, ValString = "other" });
        await context.SaveChangesAsync();

        // Act
        var tables = await context.CustomTables.AsNoTracking().ToListAsync();
        var columns = await context.CustomColumns.AsNoTracking().ToListAsync();
        var rows = await context.CustomRows.AsNoTracking().ToListAsync();
        var cells = await context.CustomCells.AsNoTracking().ToListAsync();

        // Assert
        Assert.Single(tables);
        AssertEx.AreEqual(ownTable.TableId, tables[0].TableId);

        Assert.Single(columns);
        AssertEx.AreEqual(ownTable.Columns[0].ColumnId, columns[0].ColumnId);

        Assert.Single(rows);
        AssertEx.AreEqual(ownTable.Rows[0].RowId, rows[0].RowId);

        Assert.Single(cells);
        AssertEx.AreEqual("own", cells[0].ValString);
    }
}
