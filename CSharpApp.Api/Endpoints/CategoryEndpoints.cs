using CSharpApp.Core.Dtos;

namespace CSharpApp.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.NewVersionedApi("Categories")
            .MapGroup("api/v{version:apiVersion}/categories")
            .HasApiVersion(1.0)
            .WithTags("Categories");

        group.MapGet("", GetCategories)
            .WithName("GetCategories")
            .WithSummary("Get all categories")
            .Produces<IEnumerable<CategoryDto>>();
    }

    public static async Task<IResult> GetCategories(ICategoriesService categoriesService)
    {
        var categories = await categoriesService.GetCategories();

        return Results.Ok(categories);
    }
}
