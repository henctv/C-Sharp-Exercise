using CSharpApp.Api.Endpoints;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

public class CategoryEndpointsTests
{
    [Fact]
    public async Task GetCategories_ShouldReturnOk()
    {
        // Arrange
        var categories = new List<CategoryDto>
        {
            new CategoryDto { Id = 1, Name = "Category 1" },
            new CategoryDto { Id = 2, Name = "Category 2" }
        };

        var categoryService = new Mock<ICategoriesService>();
        categoryService
            .Setup(service => service.GetCategories())
            .ReturnsAsync(categories);

        // Act
        var result = await CategoryEndpoints.GetCategories(categoryService.Object);

        // Assert
        Assert.IsType<Ok<IEnumerable<CategoryDto>>>(result);

        var okResult = Assert.IsType<Ok<IEnumerable<CategoryDto>>>(result);
        Assert.Equal(categories, okResult.Value);
    }
}