using System.Net.Http.Json;

namespace CSharpApp.Application.Products;

public class ProductsService(
    ILogger<ProductsService> logger,
    HttpClient httpClient,
    string resourcePath
) : IProductsService
{
    private readonly ILogger<ProductsService> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _resourcePath = resourcePath;

    public async Task<ProductDto?> GetProductById(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_resourcePath}/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ProductDto?>();

            _logger.LogInformation("Fetched product with ID: {Id}, Result: {@Result}", id, result);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Product with ID: {Id} not found", id);

            return null;
        }
    }

    public async Task<IEnumerable<ProductDto>> GetProducts()
    {
        var response = await _httpClient.GetAsync(_resourcePath);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();

        _logger.LogInformation("Fetched : {Count} number of products", result?.Count() ?? 0);
        return result ?? [];
    }

    public async Task<ProductDto?> CreateProduct(CreateProductDto createProduct)
    {
        var response = await _httpClient.PostAsJsonAsync(_resourcePath, createProduct);

        var content = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProductDto?>();

        _logger.LogInformation("Created product with Result: {@Result}", result);
        return result;
    }
}
