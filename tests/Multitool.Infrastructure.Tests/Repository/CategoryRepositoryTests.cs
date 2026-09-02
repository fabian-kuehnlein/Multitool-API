using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Category;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class CategoryRepositoryTests : RepositoryTestBase
{
    private readonly CategoryRepository _sut;

    public CategoryRepositoryTests()
    {
        _sut = new CategoryRepository(Context);
    }

    private static Category CreateCategory(string name)
    {
        var category = CategoryTestData.DefaultCategory;
        category.Id = 0;
        category.Name = name;
        return category;
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ReturnsCategory()
    {
        // Arrange
        var category = CreateCategory("Test");
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(category.Id);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(category.Name, result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    // GetCategoriesAsync

    [Fact]
    public async Task GetCategoriesAsync_WhenCategoriesExist_ReturnsAllSortedById()
    {
        // Arrange
        var c1 = CreateCategory("Second");
        var c2 = CreateCategory("First");
        Context.Categories.AddRange(c1, c2);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCategoriesAsync();

        // Assert
        AssertEx.AreEqual(2, result.Count);
        Assert.True(result[0].Id < result[1].Id);
    }

    [Fact]
    public async Task GetCategoriesAsync_WhenNoCategories_ReturnsEmptyList()
    {
        // Arrange

        // Act
        var result = await _sut.GetCategoriesAsync();

        // Assert
        Assert.Empty(result);
    }

    // CreateCategoryAsync

    [Fact]
    public async Task CreateCategoryAsync_WhenCategoryIsValid_AddsCategoryToDatabase()
    {
        // Arrange
        var category = CreateCategory("New Category");

        // Act
        var id = await _sut.CreateCategoryAsync(category);

        // Assert
        Assert.True(id > 0);
        var dbCategory = await Context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        Assert.NotNull(dbCategory);
        AssertEx.AreEqual("New Category", dbCategory!.Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenCategoryIsValid_ReturnsPositiveId()
    {
        // Arrange
        var category = CreateCategory("Another");

        // Act
        var id = await _sut.CreateCategoryAsync(category);

        // Assert
        Assert.True(id > 0);
    }

    // UpdateCategoryAsync

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryExists_UpdatesName()
    {
        // Arrange
        var category = CreateCategory("Old");
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        category.Name = "Updated";

        // Act
        await _sut.UpdateCategoryAsync(category);

        // Assert
        var dbCategory = await Context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == category.Id);
        Assert.NotNull(dbCategory);
        AssertEx.AreEqual("Updated", dbCategory!.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryExists_UpdatesColor()
    {
        // Arrange
        var category = CreateCategory("Cat");
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        category.Color = "#00FF00";

        // Act
        await _sut.UpdateCategoryAsync(category);

        // Assert
        var dbCategory = await Context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == category.Id);
        Assert.NotNull(dbCategory);
        AssertEx.AreEqual("#00FF00", dbCategory!.Color);
    }
}
