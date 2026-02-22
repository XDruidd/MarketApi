using Market.Models.Market;
using Market.Models.Market.FromBody;
using Market.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Market.Services;

public class ProductServices : IProductServices
{
    private readonly AppDbContext _dbContext;

    public ProductServices(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<ProductDto>> GetProducts(int pageNumber = 1, int pageSize = 10)
    {
        var products = await _dbContext.Products
            .Where(p => p.IsActive == true && p.IsDeleted == false)
            .Select(product => new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                QuantityInStock = product.QuantityInStock,
                ImgPatch = product.ImgPath
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return products;
    }

    public async Task<int> GetProductsCountPage(int pageSize = 10)
    {
        var count = await _dbContext
            .Products
            .Where(p => p.IsActive == true && p.IsDeleted == false)
            .CountAsync();
        var totalPages = (int)Math.Ceiling((double)count / pageSize);

        return totalPages;
    }

    private async Task<string> UploudImg(ProductFromBodyDto product)
    {
        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads"
        );
        
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);
        
        var fileName = Guid.NewGuid() + 
                       Path.GetExtension(product.Image.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        { 
            await product.Image.CopyToAsync(stream);
        }
        
        var imageUrl = "/Product/image/" + fileName;
        return imageUrl;
    }
    public async Task<UpdateProductDto2> PostProduct(ProductFromBodyDto product)
    {


        var imageUrl = await UploudImg(product);
        
        var newProduct = await _dbContext.Products.AddAsync(new Product {
            Name = product.Name,
            Price = product.Price,
            QuantityInStock = product.QuantityInStock,
            IsActive = product.IsActive,
            ImgPath = imageUrl
        });
        await _dbContext.SaveChangesAsync();
        
        return new UpdateProductDto2
        {
            Id = newProduct.Entity.Id,
            Name = newProduct.Entity.Name,
            Price = newProduct.Entity.Price,
            ImgPatch = newProduct.Entity.ImgPath,
            QuantityInStock = newProduct.Entity.QuantityInStock,
            IsActive = newProduct.Entity.IsActive
        };
    }

    private async Task<Product?> GetProduct(int productId)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
    }
    
    public async Task<bool> DeleteProduct(int productId)
    {
        Product? product = await GetProduct(productId);
        if (product == null || product.IsDeleted) return false;
        
        product.IsDeleted = true;
        //видаля товар з корзин при видалинні
        await DellOrder(productId);
        
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PathProduct(int productId, UpdateProductDto update)
    {
        Product? product = await GetProduct(productId);
        if (product == null || product.IsDeleted) return false;
        
        if (update.Name != null) { product.Name = update.Name;}
        if (update.Price.HasValue) { product.Price = update.Price.Value;}
        if (update.QuantityInStock.HasValue) {product.QuantityInStock = update.QuantityInStock.Value;}

        if (update.IsActive.HasValue)
        {
            product.IsActive = update.IsActive.Value;
            //удаление при фолсе с всех товаров
            if (!product.IsActive) { await DellOrder(productId); }
        }
        
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task DellOrder(int productId)
    {
        var orderProducts = await _dbContext.OrderProducts
            .Include(op => op.Order)
            .Where(op => op.ProductId == productId)
            .ToListAsync();
            
        foreach (var op in orderProducts)
        {
            op.Order.TotalPrice -= op.Count * op.Price;
        }
            
        _dbContext.OrderProducts.RemoveRange(orderProducts);
    }
}
