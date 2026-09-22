using CSharpApp.Core.Dtos;
using FluentValidation;

namespace CSharpApp.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.NewVersionedApi("Products")
            .MapGroup("api/v{version:apiVersion}/products")
            .HasApiVersion(1.0)
            .WithTags("Products");

        group.MapGet("", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get all products")
            .Produces<IEnumerable<ProductDto>>();

        group.MapGet("{id:int}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get a product by id")
            .Produces<ProductDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateProduct)
            .WithName("AddProduct")
            .WithSummary("Create a new product")
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status502BadGateway);
    }

    public static async Task<IResult> GetProducts(IProductsService productsService)
    {
        var products = await productsService.GetProducts();
        return Results.Ok(products);
    }

    public static async Task<IResult> GetProductById(int id, IProductsService productsService)
    {
        var product = await productsService.GetProductById(id);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }

    public static async Task<IResult> CreateProduct(
        HttpContext httpContext,
        CreateProductDto createProduct,
        IValidator<CreateProductDto> validator,
        IProductsService productsService)
    {
        var validationResult = await validator.ValidateAsync(createProduct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var product = await productsService.CreateProduct(createProduct);
        var version = httpContext.GetRequestedApiVersion()?.ToString() ?? "1";
        return product switch
        {
            ProductDto => Results.Created($"api/v{version}/products/{product.Id}", product),
            null => Results.Problem("Failed to create product.", statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
