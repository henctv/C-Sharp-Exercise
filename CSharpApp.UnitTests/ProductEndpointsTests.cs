using Asp.Versioning;
using CSharpApp.Api.Endpoints;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class ProductEndpointsTests
{
    [Fact]
    public async Task GetProducts_ShouldReturnResultsFromService()
    {
        // Arrange

        var productService = new Mock<IProductsService>();

        var products = new List<ProductDto>
            {
                new() { Id = 1, Title = "Product 1" },
                new() { Id = 2, Title = "Product 2" }
            };
        productService
            .Setup(service => service.GetProducts())
            .ReturnsAsync(products);

        // Act
        var productsResult = await ProductEndpoints.GetProducts(productService.Object);

        // Assert
        Assert.IsType<Ok<IEnumerable<ProductDto>>>(productsResult);

        var okResult = productsResult as Ok<IEnumerable<ProductDto>>;
        Assert.NotNull(okResult);

        Assert.Equal(products, okResult?.Value);

        productService.Verify(service => service.GetProducts(), Times.Once);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnResultFromService()
    {
        // Arrange
        var productService = new Mock<IProductsService>();
        var product = new ProductDto { Id = 1, Title = "Product 1" };
        productService
            .Setup(service => service.GetProductById(1))
            .ReturnsAsync(product);

        // Act
        var productResult = await ProductEndpoints.GetProductById(1, productService.Object);

        // Assert
        Assert.IsType<Ok<ProductDto>>(productResult);

        var okResult = productResult as Ok<ProductDto>;
        Assert.NotNull(okResult);
        Assert.Equal(product, okResult?.Value);

        productService.Verify(service => service.GetProductById(1), Times.Once);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFoundWhenProductDoesNotExist()
    {
        // Arrange
        var productService = new Mock<IProductsService>();
        productService
            .Setup(service => service.GetProductById(999))
            .ReturnsAsync((ProductDto)null);

        // Act
        var productResult = await ProductEndpoints.GetProductById(999, productService.Object);

        // Assert
        Assert.IsType<NotFound>(productResult);

        var notFoundResult = productResult as NotFound;
        Assert.NotNull(notFoundResult);

        productService.Verify(service => service.GetProductById(999), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnValidationProblem()
    {
        // Arrange
        var invalidProduct = new CreateProductDto(
            Title: "Product",
            Description: "Description",
            Price: 10m,
            CategoryId: 1,
            Images: []
        );

        var productService = new Mock<IProductsService>();
        var createProductValidator = new Mock<IValidator<CreateProductDto>>();
        createProductValidator
            .Setup(validator => validator.ValidateAsync(invalidProduct, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new("Title", "Title is required")
            }));
        var context = new DefaultHttpContext();

        // Act
        var result = await ProductEndpoints.CreateProduct(
            context,
            invalidProduct,
            createProductValidator.Object,
            productService.Object
        );

        // Assert
        Assert.IsType<ProblemHttpResult>(result);

        var validationProblemResult = result as ProblemHttpResult;
        Assert.NotNull(validationProblemResult);
        Assert.NotNull(validationProblemResult.ProblemDetails);

        createProductValidator.Verify(validator => validator.ValidateAsync(invalidProduct, It.IsAny<CancellationToken>()), Times.Once);
        productService.Verify(service => service.CreateProduct(It.IsAny<CreateProductDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnProblemWhenServiceFails()
    {
        // Arrange
        var product = new CreateProductDto(
            Title: "Product",
            Description: "Description",
            Price: 10m,
            CategoryId: 1,
            Images: []
        );

        var createProductValidator = new Mock<IValidator<CreateProductDto>>();
        createProductValidator
            .Setup(validator => validator.ValidateAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { }));

        var productService = new Mock<IProductsService>();
        productService
            .Setup(service => service.CreateProduct(It.IsAny<CreateProductDto>()))
            .ThrowsAsync(new Exception("Service failed"));

        var context = new DefaultHttpContext();
        // Act
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await ProductEndpoints.CreateProduct(
                context,
                product,
                createProductValidator.Object,
                productService.Object
            );
        });

        // Assert
        createProductValidator.Verify(validator => validator.ValidateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        productService.Verify(service => service.CreateProduct(It.IsAny<CreateProductDto>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnProblemWhenNullReturned()
    {
        // Arrange
        var product = new CreateProductDto(
            Title: "Product",
            Description: "Description",
            Price: 10m,
            CategoryId: 1,
            Images: []
        );

        var createProductValidator = new Mock<IValidator<CreateProductDto>>();
        createProductValidator
            .Setup(validator => validator.ValidateAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { }));

        var productService = new Mock<IProductsService>();
        productService
            .Setup(service => service.CreateProduct(It.IsAny<CreateProductDto>()))
            .ReturnsAsync((ProductDto)null);

        var context = new DefaultHttpContext();
        context.ApiVersioningFeature().RequestedApiVersion = new ApiVersion(1, 0);

        // Act
        var result = await ProductEndpoints.CreateProduct(
            context,
            product,
            createProductValidator.Object,
            productService.Object
        );

        // Assert
        Assert.IsType<ProblemHttpResult>(result);

        createProductValidator.Verify(validator => validator.ValidateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        productService.Verify(service => service.CreateProduct(It.IsAny<CreateProductDto>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnOk()
    {
        // Arrange
        var invalidProduct = new CreateProductDto(
            Title: "Product",
            Description: "Description",
            Price: 10m,
            CategoryId: 1,
            Images: []
        );

        var createProductValidator = new Mock<IValidator<CreateProductDto>>();
        createProductValidator
            .Setup(validator => validator.ValidateAsync(invalidProduct, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { }));

        var productService = new Mock<IProductsService>();
        productService
            .Setup(service => service.CreateProduct(It.IsAny<CreateProductDto>()))
            .ReturnsAsync(new ProductDto { });

        var context = new DefaultHttpContext();
        context.ApiVersioningFeature().RequestedApiVersion = new ApiVersion(1, 0);

        // Act
        var result = await ProductEndpoints.CreateProduct(
            context,
            invalidProduct,
            createProductValidator.Object,
            productService.Object
        );

        // Assert
        Assert.IsType<Created<ProductDto>>(result);

        createProductValidator.Verify(validator => validator.ValidateAsync(invalidProduct, It.IsAny<CancellationToken>()), Times.Once);
        productService.Verify(service => service.CreateProduct(It.IsAny<CreateProductDto>()), Times.Once);
    }
}
