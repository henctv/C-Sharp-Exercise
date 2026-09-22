using System.Net.Http.Json;

namespace CSharpApp.Application.Products;

public class CategoriesService(
    ILogger<CategoriesService> logger,
    HttpClient httpClient,
    string resourcePath
) : ICategoriesService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<CategoriesService> _logger = logger;
    private readonly string _resourcePath = resourcePath;

    public async Task<IEnumerable<CategoryDto>> GetCategories()
    {
        var response = await _httpClient.GetAsync(_resourcePath);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryDto>>();

        _logger.LogInformation("Fetched : {Count} number of categories", result?.Count() ?? 0);
        return result ?? [];
    }
}
