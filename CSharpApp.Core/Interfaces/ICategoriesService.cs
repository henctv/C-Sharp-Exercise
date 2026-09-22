namespace CSharpApp.Core.Interfaces;

public interface ICategoriesService
{
    Task<IEnumerable<CategoryDto>> GetCategories();
}