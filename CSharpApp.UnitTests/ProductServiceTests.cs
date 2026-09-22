using System.Text.Json;
using CSharpApp.Application.Products;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

public class ProductServiceTests
{
    private readonly IProductsService _productService;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public ProductServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };
        _productService = new ProductsService(
            logger: NullLogger<ProductsService>.Instance,
            httpClient: httpClient,
            resourcePath: "products"
        );
    }

    [Fact]
    public async Task GetProductById_ShouldInvokeRequestAsync()
    {
        // Arrange
        var product = new ProductDto
        {
            Id = 1,
            Title = "Product 1"
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
                Content = new StringContent(JsonSerializer.Serialize(product))
            });

        // Act
        var result = await _productService.GetProductById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Title, result.Title);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNull_WhenProductNotFound()
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
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        // Act
        var result = await _productService.GetProductById(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductById_ShouldThrow()
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
            await _productService.GetProductById(999);
        });
    }

    [Fact]
    public async Task GetProducts_ShouldThrow()
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
            await _productService.GetProducts();
        });
    }

    [Fact]
    public async Task GetProducts_ShouldReturnProducts()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Id = 1, Title = "Product 1" },
            new ProductDto { Id = 2, Title = "Product 2" }
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
                Content = new StringContent(JsonSerializer.Serialize(products))
            });

        // Act
        var result = await _productService.GetProducts();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(products.Count, result.Count());
    }

    [Fact]
    public async Task CreateProduct_ShouldThrow()
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
            await _productService.CreateProduct(new CreateProductDto(
                Title: "Product 1",
                Price: 9.99m,
                Description: "Description 1",
                CategoryId: 1,
                Images: ["http://example.com/image.jpg"]
            ));
        });
    }

    [Fact]
    public async Task CreateProduct_ReturnProduct()
    {
        // Arrange
        ProductDto product = new()
        {
            Id = 1,
            Title = "Product 1"
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
                Content = new StringContent(JsonSerializer.Serialize(product))
            });

        // Act
        var result = await _productService.CreateProduct(new CreateProductDto(
            Title: "Product 1",
            Price: 9.99m,
            Description: "Description 1",
            CategoryId: 1,
            Images: ["http://example.com/image.jpg"]
        ));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Title, result.Title);
    }
}
