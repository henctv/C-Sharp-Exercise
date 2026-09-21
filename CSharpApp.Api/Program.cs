using CSharpApp.Application.Products;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Settings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders().AddSerilog(logger);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDefaultConfiguration();
builder.Services.AddHttpConfiguration();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

builder.Services.AddHttpClient<IProductsService, ProductsService>((httpClient) =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["RestApiSettings:BaseUrl"]!);
});

builder.Services.AddHttpClient<ICategoriesService, CategoriesService>((httpClient) =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["RestApiSettings:BaseUrl"]!);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

var versionedEndpointRouteBuilder = app.NewVersionedApi();

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/Products", async (IProductsService productsService) =>
    {
        var products = await productsService.GetProducts();
        return products;
    })
    .WithName("GetProducts")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/Products/{id}", async (int id, IProductsService productsService) =>
    {
        var product = await productsService.GetProductById(id);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    })
    .WithName("GetProductById")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapPost("api/v{version:apiVersion}/Products", async (string version, Product product, IProductsService productsService) =>
    {
        // Implement the logic to add the product here
        return Results.Created($"api/v{version}/Products/{product.Id}", product);
    })
    .WithName("AddProduct")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/Categories", async (ICategoriesService categoriesService) =>
    {
        var categories = await categoriesService.GetCategories();
        return categories;
    })
    .WithName("GetCategories")
    .HasApiVersion(1.0);

app.Run();