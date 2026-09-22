namespace CSharpApp.Core.Interfaces;

public interface IProductsService
{
    Task<ProductDto?> GetProductById(int id);

    Task<IEnumerable<ProductDto>> GetProducts();

    Task<ProductDto?> CreateProduct(CreateProductDto createProduct);
}
