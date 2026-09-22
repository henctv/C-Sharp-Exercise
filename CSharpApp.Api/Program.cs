using Asp.Versioning;
using CSharpApp.Api.Endpoints;
using CSharpApp.Api.Middleware;
using CSharpApp.Application.Products;
using CSharpApp.Core.Settings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders().AddSerilog(logger);

builder.Services.AddOpenApi();
builder.Services.AddDefaultConfiguration();
builder.Services.AddHttpConfiguration();
builder.Services.AddProblemDetails();
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services
    .AddHttpClient(nameof(ProductsService), (serviceProvider, httpClient) =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;
        httpClient.BaseAddress = new Uri(settings.BaseUrl!);
    })
    .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
    .AddResiliencePolicies()
    .AddTypedClient<IProductsService>((httpClient, serviceProvider) =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;

        return new ProductsService(
            serviceProvider.GetRequiredService<ILogger<ProductsService>>(),
            httpClient,
            settings.Products!);
    });

builder.Services
    .AddHttpClient(nameof(CategoriesService), (serviceProvider, httpClient) =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;
        httpClient.BaseAddress = new Uri(settings.BaseUrl!);
    })
    .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
    .AddResiliencePolicies()
    .AddTypedClient<ICategoriesService>((httpClient, serviceProvider) =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;

        return new CategoriesService(
            serviceProvider.GetRequiredService<ILogger<CategoriesService>>(),
            httpClient,
            settings.Categories!);
    });

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CSharpApp API v1");
    });
}

app.MapProductEndpoints();
app.MapCategoryEndpoints();

app.Run();