using FluentAssertions;
using Moq;
using Multitool.Application.Models.CustomTable;
using Multitool.Application.Services;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;

namespace Multitool.Application.Tests;

public class CustomTableServiceTests
{
    private readonly Mock<ICustomTableRepository> _repositoryMock;
    private readonly CustomTableService _sut;

    public CustomTableServiceTests()
    {
        _repositoryMock = new Mock<ICustomTableRepository>();
        _sut = new CustomTableService(_repositoryMock.Object);
    }

    // GetTableListAsync

    [Fact]
    public async Task GetTableListAsync_WhenTablesExist_ReturnsMappedOverviews()
    {
        // Arrange
        var tables = new List<Table> { CustomTableTestData.DefaultTable };
        _repositoryMock.Setup(r => r.GetTableListAsync()).ReturnsAsync(tables);

        // Act
        var result = await _sut.GetTableListAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].TableId.Should().Be(CustomTableTestData.DefaultTable.TableId);
    }

    // GetTableAsync

    [Fact]
    public async Task GetTableAsync_WhenTableExists_ReturnsMappedDetail()
    {
        // Arrange
        var table = CustomTableTestData.DefaultTable;
        _repositoryMock.Setup(r => r.GetTableAsync(table.TableId)).ReturnsAsync(table);

        // Act
        var result = await _sut.GetTableAsync(table.TableId);

        // Assert
        result.TableId.Should().Be(table.TableId);
    }

    [Fact]
    public async Task GetTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetTableAsync(It.IsAny<long>())).ReturnsAsync((Table?)null);

        // Act
        var act = () => _sut.GetTableAsync(1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // CreateTableAsync

    [Fact]
    public async Task CreateTableAsync_WhenDtoIsValid_ReturnsIdAndCreatesTableWithColumn()
    {
        // Arrange
        var dto = CustomTableTestData.DefaultCreateTableDto;
        _repositoryMock.Setup(r => r.CreateTableAsync(It.IsAny<Table>())).ReturnsAsync(10L);

        // Act
        var result = await _sut.CreateTableAsync(dto);

        // Assert
        result.Should().Be(10L);
        _repositoryMock.Verify(r => r.CreateTableAsync(It.Is<Table>(t => t.Name == dto.Name)), Times.Once);
    }

    // UpdateTableAsync

    [Fact]
    public async Task UpdateTableAsync_WhenTableExists_UpdatesName()
    {
        // Arrange
        var table = CustomTableTestData.DefaultTable;
        var dto = CustomTableTestData.DefaultUpdateTableDto;
        _repositoryMock.Setup(r => r.GetTableRawAsync(table.TableId)).ReturnsAsync(table);

        // Act
        await _sut.UpdateTableAsync(table.TableId, dto);

        // Assert
        table.Name.Should().Be(dto.Name);
        _repositoryMock.Verify(r => r.UpdateTableAsync(table), Times.Once);
    }

    [Fact]
    public async Task UpdateTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetTableRawAsync(It.IsAny<long>())).ReturnsAsync((Table?)null);

        // Act
        var act = () => _sut.UpdateTableAsync(1, CustomTableTestData.DefaultUpdateTableDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // DeleteTableAsync

    [Fact]
    public async Task DeleteTableAsync_WhenTableExists_CallsDelete()
    {
        // Arrange
        _repositoryMock.Setup(r => r.TableExistsAsync(1)).ReturnsAsync(true);

        // Act
        await _sut.DeleteTableAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.DeleteTableAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.TableExistsAsync(It.IsAny<long>())).ReturnsAsync(false);

        // Act
        var act = () => _sut.DeleteTableAsync(1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // CreateColumnAsync

    [Fact]
    public async Task CreateColumnAsync_WhenTableExists_CallsRepository()
    {
        // Arrange
        _repositoryMock.Setup(r => r.TableExistsAsync(1)).ReturnsAsync(true);

        // Act
        await _sut.CreateColumnAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.CreateColumnAsync(1), Times.Once);
    }

    [Fact]
    public async Task CreateColumnAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.TableExistsAsync(1)).ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateColumnAsync(1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // UpdateColumnAsync

    [Fact]
    public async Task UpdateColumnAsync_WhenColumnExists_UpdatesProperties()
    {
        // Arrange
        var existing = CustomTableTestData.DefaultColumn;
        var dto = CustomTableTestData.DefaultUpdateColumnDto;
        _repositoryMock.Setup(r => r.GetColumnAsync(existing.ColumnId)).ReturnsAsync(existing);

        // Act
        await _sut.UpdateColumnAsync(existing.ColumnId, dto);

        // Assert
        existing.Name.Should().Be(dto.Name);
        existing.DataType.Should().Be(dto.DataType);
        _repositoryMock.Verify(r => r.UpdateColumnAsync(existing, true), Times.Once);
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenDataTypeUnchanged_PassesTypeChangedFalse()
    {
        // Arrange
        var existing = CustomTableTestData.DefaultColumn;
        var dto = CustomTableTestData.DefaultUpdateColumnDto with { DataType = existing.DataType };
        _repositoryMock.Setup(r => r.GetColumnAsync(existing.ColumnId)).ReturnsAsync(existing);

        // Act
        await _sut.UpdateColumnAsync(existing.ColumnId, dto);

        // Assert
        _repositoryMock.Verify(r => r.UpdateColumnAsync(existing, false), Times.Once);
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenColumnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetColumnAsync(It.IsAny<long>())).ReturnsAsync((Column?)null);

        // Act
        var act = () => _sut.UpdateColumnAsync(1, CustomTableTestData.DefaultUpdateColumnDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // UpdateColumnOrderAsync

    [Fact]
    public async Task UpdateColumnOrderAsync_WhenColumnsProvided_CallsRepositoryWithMappedColumns()
    {
        // Arrange
        var list = new List<UpdateColumnOrderDto> { new(1, 0) };

        // Act
        await _sut.UpdateColumnOrderAsync(list);

        // Assert
        _repositoryMock.Verify(r => r.UpdateColumnOrderAsync(It.IsAny<List<Column>>()), Times.Once);
    }

    // DeleteColumnAsync

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnExists_CallsRepository()
    {
        // Arrange
        var col = CustomTableTestData.DefaultColumn;
        _repositoryMock.Setup(r => r.GetColumnAsync(col.ColumnId)).ReturnsAsync(col);

        // Act
        await _sut.DeleteColumnAsync(col.TableId, col.ColumnId);

        // Assert
        _repositoryMock.Verify(r => r.DeleteColumnAsync(col.ColumnId), Times.Once);
    }

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnTableMismatch_ThrowsNotFoundException()
    {
        // Arrange
        var col = CustomTableTestData.DefaultColumn;
        _repositoryMock.Setup(r => r.GetColumnAsync(col.ColumnId)).ReturnsAsync(col);

        // Act
        var act = () => _sut.DeleteColumnAsync(999, col.ColumnId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // CreateRowAsync

    [Fact]
    public async Task CreateRowAsync_WhenTableExists_CallsRepository()
    {
        // Arrange
        _repositoryMock.Setup(r => r.TableExistsAsync(1)).ReturnsAsync(true);

        // Act
        await _sut.CreateRowAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.CreateRowAsync(1), Times.Once);
    }

    // DeleteRowsAsync

    [Fact]
    public async Task DeleteRowsAsync_WhenAllRowsExist_CallsDelete()
    {
        // Arrange
        var rowIds = new List<long> { 1, 2 };
        _repositoryMock.Setup(r => r.GetExistingRowIdsAsync(1, rowIds)).ReturnsAsync(rowIds);

        // Act
        await _sut.DeleteRowsAsync(1, rowIds);

        // Assert
        _repositoryMock.Verify(r => r.DeleteRowsAsync(1, rowIds), Times.Once);
    }

    [Fact]
    public async Task DeleteRowsAsync_WhenSomeRowsMissing_ThrowsNotFoundException()
    {
        // Arrange
        var rowIds = new List<long> { 1, 2 };
        _repositoryMock.Setup(r => r.GetExistingRowIdsAsync(1, rowIds)).ReturnsAsync(new List<long> { 1 });

        // Act
        var act = () => _sut.DeleteRowsAsync(1, rowIds);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // UpdateRowOrderAsync

    [Fact]
    public async Task UpdateRowOrderAsync_WhenRowsProvided_CallsRepositoryWithRowOrderDictionary()
    {
        // Arrange
        var list = new List<RowOrderUpdateDto> { new(1, 0) };

        // Act
        await _sut.UpdateRowOrderAsync(list);

        // Assert
        _repositoryMock.Verify(r => r.UpdateRowOrderAsync(It.Is<Dictionary<long, int>>(d =>
            d.Count == 1 && d.ContainsKey(1) && d[1] == 0)), Times.Once);
    }

    // UpsertCellAsync

    [Fact]
    public async Task UpsertCellAsync_WhenRowAndColumnExist_CallsRepositoryWithColumnDataType()
    {
        // Arrange
        var row = CustomTableTestData.DefaultRow;
        var column = CustomTableTestData.DefaultColumn;
        _repositoryMock.Setup(r => r.GetRowAsync(row.RowId)).ReturnsAsync(row);
        _repositoryMock.Setup(r => r.GetColumnAsync(column.ColumnId)).ReturnsAsync(column);

        // Act
        await _sut.UpsertCellAsync(row.RowId, column.ColumnId, "val");

        // Assert
        _repositoryMock.Verify(r => r.UpsertCellAsync(row.RowId, column.ColumnId, column.DataType, "val"), Times.Once);
    }

    [Fact]
    public async Task UpsertCellAsync_WhenRowMissing_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetRowAsync(1)).ReturnsAsync((Row?)null);

        // Act
        var act = () => _sut.UpsertCellAsync(1, 1, "val");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpsertCellAsync_WhenColumnMissing_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetRowAsync(1)).ReturnsAsync(new Row());
        _repositoryMock.Setup(r => r.GetColumnAsync(1)).ReturnsAsync((Column?)null);

        // Act
        var act = () => _sut.UpsertCellAsync(1, 1, "val");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
