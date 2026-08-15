using FluentAssertions;
using Mapster;
using Moq;
using Multitool.Application.Models;
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

    // GetCategoriesAsync

    [Fact]
    public async Task GetCategoriesAsync_WhenCategoriesExist_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category> { CalendarTestData.DefaultCategory };
        _repositoryMock.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _sut.GetCategoriesAsync();

        // Assert
        result.Should().BeEquivalentTo(categories.Adapt<List<CategoryDto>>());
    }

    [Fact]
    public async Task GetCategoriesAsync_WhenNoCategoriesFound_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(new List<Category>());

        // Act
        var act = () => _sut.GetCategoriesAsync();

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("No categories found");
    }
}
