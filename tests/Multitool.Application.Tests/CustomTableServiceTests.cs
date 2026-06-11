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

    [Fact]
    public async Task GetTableListAsync_ReturnsMappedOverviews()
    {
        var tables = new List<Table> { CustomTableTestData.DefaultTable };
        _repositoryMock.Setup(r => r.GetTableListAsync()).ReturnsAsync(tables);

        var result = await _sut.GetTableListAsync();

        result.Should().HaveCount(1);
        result[0].TableId.Should().Be(CustomTableTestData.DefaultTable.TableId);
    }

    [Fact]
    public async Task GetTableAsync_WhenTableExists_ReturnsMappedDetail()
    {
        var table = CustomTableTestData.DefaultTable;
        _repositoryMock.Setup(r => r.GetTableAsync(table.TableId)).ReturnsAsync(table);

        var result = await _sut.GetTableAsync(table.TableId);

        result.TableId.Should().Be(table.TableId);
    }

    [Fact]
    public async Task GetTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetTableAsync(It.IsAny<long>())).ReturnsAsync((Table?)null);

        var act = () => _sut.GetTableAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateTableAsync_CallsRepository_WithCorrectData()
    {
        var dto = CustomTableTestData.DefaultCreateTableDto;
        _repositoryMock.Setup(r => r.CreateTableAsync(It.IsAny<Table>())).ReturnsAsync(10L);

        var result = await _sut.CreateTableAsync(dto);

        result.Should().Be(10L);
        _repositoryMock.Verify(r => r.CreateTableAsync(It.Is<Table>(t => t.Name == dto.Name)), Times.Once);
    }

    [Fact]
    public async Task UpdateTableAsync_WhenTableExists_UpdatesName()
    {
        var table = CustomTableTestData.DefaultTable;
        var dto = CustomTableTestData.DefaultUpdateTableDto;
        _repositoryMock.Setup(r => r.GetTableRawAsync(table.TableId)).ReturnsAsync(table);

        await _sut.UpdateTableAsync(table.TableId, dto);

        table.Name.Should().Be(dto.Name);
        _repositoryMock.Verify(r => r.UpdateTableAsync(table), Times.Once);
    }

    [Fact]
    public async Task DeleteTableAsync_WhenTableExists_CallsDelete()
    {
        _repositoryMock.Setup(r => r.TableExistsAsync(1)).ReturnsAsync(true);

        await _sut.DeleteTableAsync(1);

        _repositoryMock.Verify(r => r.DeleteTableAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenColumnExists_UpdatesProperties()
    {
        var existing = CustomTableTestData.DefaultColumn;
        var dto = CustomTableTestData.DefaultUpdateColumnDto;
        _repositoryMock.Setup(r => r.GetColumnAsync(existing.ColumnId)).ReturnsAsync(existing);

        await _sut.UpdateColumnAsync(existing.ColumnId, dto);

        existing.Name.Should().Be(dto.Name);
        existing.DataType.Should().Be(dto.DataType);
        _repositoryMock.Verify(r => r.UpdateColumnAsync(existing, true), Times.Once);
    }

    [Fact]
    public async Task DeleteRowsAsync_WhenAllRowsExist_CallsDelete()
    {
        var rowIds = new List<long> { 1, 2 };
        _repositoryMock.Setup(r => r.GetExistingRowIdsAsync(1, rowIds)).ReturnsAsync(rowIds);

        await _sut.DeleteRowsAsync(1, rowIds);

        _repositoryMock.Verify(r => r.DeleteRowsAsync(1, rowIds), Times.Once);
    }

    [Fact]
    public async Task DeleteRowsAsync_WhenSomeRowsMissing_ThrowsNotFoundException()
    {
        var rowIds = new List<long> { 1, 2 };
        _repositoryMock.Setup(r => r.GetExistingRowIdsAsync(1, rowIds)).ReturnsAsync(new List<long> { 1 });

        var act = () => _sut.DeleteRowsAsync(1, rowIds);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetTableRawAsync(It.IsAny<long>())).ReturnsAsync((Table?)null);

        var act = () => _sut.UpdateTableAsync(1, CustomTableTestData.DefaultUpdateTableDto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteTableAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.TableExistsAsync(It.IsAny<long>())).ReturnsAsync(false);

        var act = () => _sut.DeleteTableAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateColumnAsync_WhenTableExists_CallsRepository()
    {
        _repositoryMock.Setup(r => r.TableExistsAsync(1)).ReturnsAsync(true);

        await _sut.CreateColumnAsync(1);

        _repositoryMock.Verify(r => r.CreateColumnAsync(1), Times.Once);
    }

    [Fact]
    public async Task CreateColumnAsync_WhenTableDoesNotExist_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.TableExistsAsync(1)).ReturnsAsync(false);

        var act = () => _sut.CreateColumnAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenColumnDoesNotExist_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetColumnAsync(It.IsAny<long>())).ReturnsAsync((Column?)null);

        var act = () => _sut.UpdateColumnAsync(1, CustomTableTestData.DefaultUpdateColumnDto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateColumnOrderAsync_CallsRepository()
    {
        var list = new List<UpdateColumnOrderDto> { new(1, 0) };

        await _sut.UpdateColumnOrderAsync(list);

        _repositoryMock.Verify(r => r.UpdateColumnOrderAsync(It.IsAny<List<Column>>()), Times.Once);
    }

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnExists_CallsRepository()
    {
        var col = CustomTableTestData.DefaultColumn;
        _repositoryMock.Setup(r => r.GetColumnAsync(col.ColumnId)).ReturnsAsync(col);

        await _sut.DeleteColumnAsync(col.TableId, col.ColumnId);

        _repositoryMock.Verify(r => r.DeleteColumnAsync(col.ColumnId), Times.Once);
    }

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnTableMismatch_ThrowsNotFoundException()
    {
        var col = CustomTableTestData.DefaultColumn;
        _repositoryMock.Setup(r => r.GetColumnAsync(col.ColumnId)).ReturnsAsync(col);

        var act = () => _sut.DeleteColumnAsync(999, col.ColumnId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateRowAsync_WhenTableExists_CallsRepository()
    {
        _repositoryMock.Setup(r => r.TableExistsAsync(1)).ReturnsAsync(true);

        await _sut.CreateRowAsync(1);

        _repositoryMock.Verify(r => r.CreateRowAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateRowOrderAsync_CallsRepository()
    {
        var list = new List<RowOrderUpdateDto> { new(1, 0) };

        await _sut.UpdateRowOrderAsync(list);

        _repositoryMock.Verify(r => r.UpdateRowOrderAsync(It.Is<Dictionary<long, int>>(d => 
            d.Count == 1 && d.ContainsKey(1) && d[1] == 0)), Times.Once);
    }

    [Fact]
    public async Task UpsertCellAsync_WhenRowMissing_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetRowAsync(1)).ReturnsAsync((Row?)null);

        var act = () => _sut.UpsertCellAsync(1, 1, "val");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpsertCellAsync_WhenColumnMissing_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetRowAsync(1)).ReturnsAsync(new Row());
        _repositoryMock.Setup(r => r.GetColumnAsync(1)).ReturnsAsync((Column?)null);

        var act = () => _sut.UpsertCellAsync(1, 1, "val");

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
