namespace CSharpApp.Core.Interfaces;

public interface IProductsService
{
    Task<Product?> GetProductById(int id);

    Task<IEnumerable<Product>> GetProducts();
}
