using System.Net.Http.Json;

namespace CSharpApp.Application.Products;

public class CategoriesService(
    HttpClient httpClient,
    ILogger<CategoriesService> logger
) : ICategoriesService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<CategoriesService> _logger = logger;

    public async Task<IEnumerable<Category>> GetCategories()
    {
        var response = await _httpClient.GetAsync("categories");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<Category>>();

        _logger.LogInformation("Fetched : {Count} number of categories", result?.Count() ?? 0);
        return result ?? [];
    }
}
