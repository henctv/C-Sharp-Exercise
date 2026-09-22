using System.Text.Json;
using CSharpApp.Application.Products;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

public class CategoryServiceTests
{
    private readonly ICategoriesService _categoryService;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public CategoryServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };
        _categoryService = new CategoriesService(
            logger: NullLogger<CategoriesService>.Instance,
            httpClient: httpClient,
            resourcePath: "categories"
        );
    }

    [Fact]
    public async Task GetCategories_ShouldThrow()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            });

        // Act
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await _categoryService.GetCategories();
        });
    }

    [Fact]
    public async Task GetCategories_ShouldReturnCategories()
    {
        // Arrange
        var categories = new List<CategoryDto>
        {
            new CategoryDto { Id = 1, Name = "Category 1" },
            new CategoryDto { Id = 2, Name = "Category 2" }
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(categories))
            });

        // Act
        var result = await _categoryService.GetCategories();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categories.Count, result.Count());
    }
}
