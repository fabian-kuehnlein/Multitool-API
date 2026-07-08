using FluentAssertions;
using Moq;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;

namespace Multitool.Application.Tests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _sut = new CategoryService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsAllCategories()
    {
        var categories = new List<Category> { CalendarTestData.DefaultCategory };
        _repositoryMock.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(categories);

        var result = await _sut.GetCategoriesAsync();

        result.Should().BeEquivalentTo(categories);
    }

    [Fact]
    public async Task GetCategoriesAsync_WhenNoCategoriesFound_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(new List<Category>());

        var act = () => _sut.GetCategoriesAsync();

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("No categories found");
    }
}
