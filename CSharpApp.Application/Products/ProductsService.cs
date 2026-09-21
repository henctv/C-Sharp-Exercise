using System.Net.Http.Json;

namespace CSharpApp.Application.Products;

public class ProductsService(
    HttpClient httpClient,
    ILogger<ProductsService> logger
) : IProductsService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<ProductsService> _logger = logger;

    public async Task<Product?> GetProductById(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"products/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Product?>();

            _logger.LogInformation("Fetched product with ID: {Id}, Result: {@Result}", id, result);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Product with ID: {Id} not found", id);

            return null;
        }
    }

    public async Task<IEnumerable<Product>> GetProducts()
    {
        var response = await _httpClient.GetAsync("products");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();

        _logger.LogInformation("Fetched : {Count} number of products", result?.Count() ?? 0);
        return result ?? [];
    }
}
