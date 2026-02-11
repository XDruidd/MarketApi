using Market.Models.Market;
using Market.Models.Market.FromBody;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Market.Services.Interface;

public interface IProductServices
{
    Task<List<ProductDto>> GetProducts(int pageNumber = 1, int pageSize = 10);

    Task<int> GetProductsCountPage(int pageSize = 10);
    
    Task<UpdateProductDto2> PostProduct(ProductFromBody product);
    
    Task<bool> DeleteProduct(int productId);
    Task<bool> PathProduct(int productId, UpdateProductDto update);
}