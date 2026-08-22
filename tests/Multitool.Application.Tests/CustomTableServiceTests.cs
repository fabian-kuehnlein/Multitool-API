using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Models.CustomTable;
using Multitool.Application.Models.Info;
using Multitool.Application.Services;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class CustomTableServiceTests
{
    private readonly Mock<ICustomTableRepository> _repositoryMock;

    private List<Table> _getTableListResponse;
    private Table? _getTableResponse;
    private long _createTableResponse;
    private Table? _getTableRawResponse;
    private bool _tableExistsResponse;
    private Column? _getColumnResponse;
    private Row? _getRowResponse;
    private List<long> _getExistingRowIdsResponse;

    private static readonly long TABLE_ID = CustomTableTestData.DefaultTable.TableId;
    private static readonly long COLUMN_ID = CustomTableTestData.DefaultColumn.ColumnId;
    private static readonly long ROW_ID = CustomTableTestData.DefaultRow.RowId;

    private Table? _createdTable;
    private Table? _updatedTable;
    private Column? _updatedColumn;
    private bool? _updatedColumnTypeChanged;

    public CustomTableServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _repositoryMock = new Mock<ICustomTableRepository>();

        _getTableListResponse = new List<Table>();
        _getTableResponse = CustomTableTestData.DefaultTable;
        _createTableResponse = TABLE_ID;
        _getTableRawResponse = CustomTableTestData.DefaultTable;
        _tableExistsResponse = true;
        _getColumnResponse = CustomTableTestData.DefaultColumn;
        _getRowResponse = CustomTableTestData.DefaultRow;
        _getExistingRowIdsResponse = new List<long>();
    }

    private CustomTableService GetService()
    {
        _createdTable = null;
        _updatedTable = null;
        _updatedColumn = null;
        _updatedColumnTypeChanged = null;

        _repositoryMock.Reset();

        _repositoryMock.Setup(r => r.GetTableListAsync())
            .ReturnsAsync(_getTableListResponse);

        _repositoryMock.Setup(r => r.GetTableAsync(It.IsAny<long>()))
            .ReturnsAsync(_getTableResponse);

        _repositoryMock.Setup(r => r.CreateTableAsync(It.IsAny<Table>()))
            .Callback<Table>(t => _createdTable = t)
            .ReturnsAsync(_createTableResponse);

        _repositoryMock.Setup(r => r.GetTableRawAsync(It.IsAny<long>()))
            .ReturnsAsync(_getTableRawResponse);

        _repositoryMock.Setup(r => r.UpdateTableAsync(It.IsAny<Table>()))
            .Callback<Table>(t => _updatedTable = t)
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.DeleteTableAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.TableExistsAsync(It.IsAny<long>()))
            .ReturnsAsync(_tableExistsResponse);

        _repositoryMock.Setup(r => r.CreateColumnAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.GetColumnAsync(It.IsAny<long>()))
            .ReturnsAsync(_getColumnResponse);

        _repositoryMock.Setup(r => r.UpdateColumnAsync(It.IsAny<Column>(), It.IsAny<bool>()))
            .Callback<Column, bool>((c, typeChanged) =>
            {
                _updatedColumn = c;
                _updatedColumnTypeChanged = typeChanged;
            })
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.UpdateColumnOrderAsync(It.IsAny<List<Column>>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.DeleteColumnAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.CreateRowAsync(It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.UpdateRowOrderAsync(It.IsAny<Dictionary<long, int>>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.GetRowAsync(It.IsAny<long>()))
            .ReturnsAsync(_getRowResponse);

        _repositoryMock.Setup(r => r.UpsertCellAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CustomDataType>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.GetExistingRowIdsAsync(It.IsAny<long>(), It.IsAny<List<long>>()))
            .ReturnsAsync(_getExistingRowIdsResponse);

        _repositoryMock.Setup(r => r.DeleteRowsAsync(It.IsAny<long>(), It.IsAny<List<long>>()))
            .Returns(Task.CompletedTask);

        return new CustomTableService(_repositoryMock.Object);
    }

    // GetTableListAsync
    [Fact]
    public async Task GetTableListAsync_WhenTablesExist_ReturnsMappedOverviews()
    {
        // Arrange
        var tables = new List<Table> { CustomTableTestData.DefaultTable };
        _getTableListResponse = tables;
        var service = GetService();

        // Act
        var result = await service.GetTableListAsync();

        // Assert
        AssertEx.AreEqual(result, tables.Adapt<List<TableOverviewDto>>());

        _repositoryMock.Verify(r => r.GetTableListAsync(), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // GetTableAsync
    [Fact]
    public async Task GetTableAsync_WhenTableExists_ReturnsMappedDetail()
    {
        // Arrange
        var table = CustomTableTestData.DefaultTable;
        _getTableResponse = table;
        var service = GetService();

        // Act
        var result = await service.GetTableAsync(table.TableId);

        // Assert
        AssertEx.AreEqual(result, table.Adapt<TableDetail>());

        _repositoryMock.Verify(r => r.GetTableAsync(TABLE_ID), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getTableResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.GetTableAsync(TABLE_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetTableAsync(TABLE_ID), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // CreateTableAsync
    [Fact]
    public async Task CreateTableAsync_WhenDtoIsValid_CreatesTableWithColumnAndReturnsId()
    {
        // Arrange
        const long expectedId = 10L;
        var dto = CustomTableTestData.DefaultCreateTableDto;
        _createTableResponse = expectedId;
        var service = GetService();

        // Act
        var result = await service.CreateTableAsync(dto);

        // Assert
        AssertEx.AreEqual(result, expectedId);
        AssertEx.AreEqual(dto.Name, _createdTable!.Name);
        AssertEx.AreEqual(1, _createdTable.Columns.Count);
        AssertEx.AreEqual(dto.Column.Name, _createdTable.Columns[0].Name);
        AssertEx.AreEqual(dto.Column.DataType, _createdTable.Columns[0].DataType);
        AssertEx.CloseTo(_createdTable.CreatedAt, DateTime.Now, TimeSpan.FromSeconds(5));

        _repositoryMock.Verify(r => r.CreateTableAsync(It.Is<Table>(t =>
            t.Name == dto.Name &&
            t.Columns.Count == 1 &&
            t.Columns[0].Name == dto.Column.Name &&
            t.Columns[0].DataType == dto.Column.DataType
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // UpdateTableAsync
    [Fact]
    public async Task UpdateTableAsync_WhenTableExists_UpdatesName()
    {
        // Arrange
        var table = CustomTableTestData.DefaultTable;
        var dto = CustomTableTestData.DefaultUpdateTableDto;
        _getTableRawResponse = table;
        var service = GetService();

        // Act
        await service.UpdateTableAsync(table.TableId, dto);

        // Assert
        AssertEx.AreEqual(dto.Name, table.Name);
        AssertEx.AreEqual(_updatedTable, table);

        _repositoryMock.Verify(r => r.GetTableRawAsync(TABLE_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateTableAsync(It.Is<Table>(t =>
            t.TableId == TABLE_ID &&
            t.Name == dto.Name
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getTableRawResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateTableAsync(TABLE_ID, CustomTableTestData.DefaultUpdateTableDto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetTableRawAsync(TABLE_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateTableAsync(It.IsAny<Table>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // DeleteTableAsync
    [Fact]
    public async Task DeleteTableAsync_WhenTableExists_CallsRepositoryDelete()
    {
        // Arrange
        _tableExistsResponse = true;
        var service = GetService();

        // Act
        await service.DeleteTableAsync(TABLE_ID);

        // Assert
        _repositoryMock.Verify(r => r.TableExistsAsync(TABLE_ID), Times.Once);
        _repositoryMock.Verify(r => r.DeleteTableAsync(TABLE_ID), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _tableExistsResponse = false;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteTableAsync(TABLE_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.TableExistsAsync(TABLE_ID), Times.Once);
        _repositoryMock.Verify(r => r.DeleteTableAsync(It.IsAny<long>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // CreateColumnAsync
    [Fact]
    public async Task CreateColumnAsync_WhenTableExists_CallsRepository()
    {
        // Arrange
        _tableExistsResponse = true;
        var service = GetService();

        // Act
        await service.CreateColumnAsync(TABLE_ID);

        // Assert
        _repositoryMock.Verify(r => r.TableExistsAsync(TABLE_ID), Times.Once);
        _repositoryMock.Verify(r => r.CreateColumnAsync(TABLE_ID), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateColumnAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _tableExistsResponse = false;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.CreateColumnAsync(TABLE_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.TableExistsAsync(TABLE_ID), Times.Once);
        _repositoryMock.Verify(r => r.CreateColumnAsync(It.IsAny<long>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // UpdateColumnAsync
    [Fact]
    public async Task UpdateColumnAsync_WhenDataTypeChanged_PassesTypeChangedTrue()
    {
        // Arrange
        var existing = CustomTableTestData.DefaultColumn;
        var dto = CustomTableTestData.DefaultUpdateColumnDto;
        _getColumnResponse = existing;
        var service = GetService();

        // Act
        await service.UpdateColumnAsync(existing.ColumnId, dto);

        // Assert
        AssertEx.AreEqual(dto.Name, existing.Name);
        AssertEx.AreEqual(dto.DataType, existing.DataType);
        AssertEx.AreEqual(dto.ColOrder, existing.ColOrder);
        AssertEx.AreEqual(true, _updatedColumnTypeChanged);

        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateColumnAsync(It.Is<Column>(c =>
            c.ColumnId == COLUMN_ID &&
            c.Name == dto.Name &&
            c.DataType == dto.DataType &&
            c.ColOrder == dto.ColOrder
        ), true), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenDataTypeUnchanged_PassesTypeChangedFalse()
    {
        // Arrange
        var existing = CustomTableTestData.DefaultColumn;
        var dto = CustomTableTestData.DefaultUpdateColumnDto with { DataType = existing.DataType };
        _getColumnResponse = existing;
        var service = GetService();

        // Act
        await service.UpdateColumnAsync(existing.ColumnId, dto);

        // Assert
        AssertEx.AreEqual(false, _updatedColumnTypeChanged);

        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateColumnAsync(It.Is<Column>(c =>
            c.ColumnId == COLUMN_ID &&
            c.Name == dto.Name &&
            c.DataType == dto.DataType &&
            c.ColOrder == dto.ColOrder
        ), false), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenColumnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getColumnResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateColumnAsync(COLUMN_ID, CustomTableTestData.DefaultUpdateColumnDto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateColumnAsync(It.IsAny<Column>(), It.IsAny<bool>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // UpdateColumnOrderAsync
    [Fact]
    public async Task UpdateColumnOrderAsync_WhenColumnsProvided_CallsRepositoryWithMappedColumns()
    {
        // Arrange
        var list = new List<UpdateColumnOrderDto>
        {
            new(CustomTableTestData.DefaultColumn.ColumnId, 0),
            new(2, 1)
        };
        var service = GetService();

        // Act
        await service.UpdateColumnOrderAsync(list);

        // Assert
        _repositoryMock.Verify(r => r.UpdateColumnOrderAsync(It.Is<List<Column>>(columns =>
            columns.Count == 2 &&
            columns[0].ColumnId == 1 && columns[0].ColOrder == 0 &&
            columns[1].ColumnId == 2 && columns[1].ColOrder == 1
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // DeleteColumnAsync
    [Fact]
    public async Task DeleteColumnAsync_WhenColumnExists_CallsRepository()
    {
        // Arrange
        var column = CustomTableTestData.DefaultColumn;
        _getColumnResponse = column;
        var service = GetService();

        // Act
        await service.DeleteColumnAsync(column.TableId, column.ColumnId);

        // Assert
        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.DeleteColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnBelongsToAnotherTable_ThrowsNotFoundException()
    {
        // Arrange
        _getColumnResponse = CustomTableTestData.DefaultColumn;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteColumnAsync(999, COLUMN_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.DeleteColumnAsync(It.IsAny<long>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getColumnResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteColumnAsync(1, COLUMN_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.DeleteColumnAsync(It.IsAny<long>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // CreateRowAsync
    [Fact]
    public async Task CreateRowAsync_WhenTableExists_CallsRepository()
    {
        // Arrange
        _tableExistsResponse = true;
        var service = GetService();

        // Act
        await service.CreateRowAsync(TABLE_ID);

        // Assert
        _repositoryMock.Verify(r => r.TableExistsAsync(TABLE_ID), Times.Once);
        _repositoryMock.Verify(r => r.CreateRowAsync(TABLE_ID), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateRowAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _tableExistsResponse = false;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.CreateRowAsync(TABLE_ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.TableExistsAsync(TABLE_ID), Times.Once);
        _repositoryMock.Verify(r => r.CreateRowAsync(It.IsAny<long>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // DeleteRowsAsync
    [Fact]
    public async Task DeleteRowsAsync_WhenAllRowsExist_CallsRepositoryDelete()
    {
        // Arrange
        var rowIds = new List<long> { 1, 2 };
        _getExistingRowIdsResponse = rowIds;
        var service = GetService();

        // Act
        await service.DeleteRowsAsync(TABLE_ID, rowIds);

        // Assert
        _repositoryMock.Verify(r => r.GetExistingRowIdsAsync(TABLE_ID, rowIds), Times.Once);
        _repositoryMock.Verify(r => r.DeleteRowsAsync(TABLE_ID, rowIds), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteRowsAsync_WhenSomeRowsAreMissing_ThrowsNotFoundException()
    {
        // Arrange
        var rowIds = new List<long> { 1, 2 };
        _getExistingRowIdsResponse = new List<long> { 1 };
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteRowsAsync(TABLE_ID, rowIds);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetExistingRowIdsAsync(TABLE_ID, rowIds), Times.Once);
        _repositoryMock.Verify(r => r.DeleteRowsAsync(It.IsAny<long>(), It.IsAny<List<long>>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // UpdateRowOrderAsync
    [Fact]
    public async Task UpdateRowOrderAsync_WhenRowsProvided_CallsRepositoryWithRowOrderDictionary()
    {
        // Arrange
        var list = new List<RowOrderUpdateDto> { new(1, 0) };
        var service = GetService();

        // Act
        await service.UpdateRowOrderAsync(list);

        // Assert
        _repositoryMock.Verify(r => r.UpdateRowOrderAsync(It.Is<Dictionary<long, int>>(d =>
            d.Count == 1 && d.ContainsKey(1) && d[1] == 0
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // UpsertCellAsync
    [Fact]
    public async Task UpsertCellAsync_WhenRowAndColumnExist_CallsRepositoryWithColumnDataType()
    {
        // Arrange
        var row = CustomTableTestData.DefaultRow;
        var column = CustomTableTestData.DefaultColumn;
        _getRowResponse = row;
        _getColumnResponse = column;
        var service = GetService();

        // Act
        await service.UpsertCellAsync(row.RowId, column.ColumnId, "val");

        // Assert
        _repositoryMock.Verify(r => r.GetRowAsync(ROW_ID), Times.Once);
        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpsertCellAsync(ROW_ID, COLUMN_ID, column.DataType, "val"), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertCellAsync_WhenRowDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getRowResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpsertCellAsync(ROW_ID, COLUMN_ID, "val");

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetRowAsync(ROW_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpsertCellAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CustomDataType>(), It.IsAny<object?>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertCellAsync_WhenColumnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getColumnResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpsertCellAsync(ROW_ID, COLUMN_ID, "val");

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetRowAsync(ROW_ID), Times.Once);
        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpsertCellAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CustomDataType>(), It.IsAny<object?>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("not-a-number", CustomDataType.Int)]
    [InlineData("not-a-decimal", CustomDataType.Decimal)]
    [InlineData("not-a-date", CustomDataType.Date)]
    [InlineData("not-a-bool", CustomDataType.Bool)]
    public async Task UpsertCellAsync_WhenValueDoesNotMatchColumnDataType_ThrowsArgumentException(string value, CustomDataType dataType)
    {
        // Arrange
        var column = CustomTableTestData.DefaultColumn;
        column.DataType = dataType;
        _getColumnResponse = column;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpsertCellAsync(ROW_ID, COLUMN_ID, value);

        // Assert
        await AssertEx.Throws<ArgumentException>(act);

        _repositoryMock.Verify(r => r.GetRowAsync(ROW_ID), Times.Once);
        _repositoryMock.Verify(r => r.GetColumnAsync(COLUMN_ID), Times.Once);
        _repositoryMock.Verify(r => r.UpsertCellAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CustomDataType>(), It.IsAny<object?>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }
}
