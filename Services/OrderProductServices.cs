using System.Runtime.InteropServices.JavaScript;
using Market.Models;
using Market.Models.Market;
using Market.Models.Market.FromBody;
using Market.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Market.Services;

public class OrderProductServices : IOrderProductServices
{
    private readonly ILogger<OrderProductServices> _logger;
    private readonly AppDbContext _dbContext;

    public OrderProductServices(ILogger<OrderProductServices> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task<StatusResult> PostOrder(OrderProductDto orderProduct, string userId)
    {

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId);
        var product = await _dbContext.Products.FindAsync(orderProduct.ProductId);
        
        if (order == null || product == null)
        {
            return new StatusResult(ReturnStatusCode.BadRequest,"No order found");
        }
        if (order.Status)
        {
            return new StatusResult(ReturnStatusCode.Conflict,"Order already completed");
        }
        if (product.IsDeleted)
        {
            return new StatusResult(ReturnStatusCode.Deleted, "Product deleted");
        }
        

        var orderPId = await _dbContext.OrderProducts.FirstOrDefaultAsync(op => op.ProductId == orderProduct.ProductId && op.OrderId == order.Id);
        if (orderPId != null)
        {
            /*if ((orderPId.Count + orderProduct.Count) > product.QuantityInStock)
            {
                return new StatusResult(ReturnStatusCode.Conflict, "Not enough stock");
            }

            orderPId.Count += orderProduct.Count;
            order.TotalPrice += orderProduct.Count * product.Price;
            await _dbContext.SaveChangesAsync();*/
            return new StatusResult(ReturnStatusCode.Conflict, "Product already add");
        }

        if (product.QuantityInStock < orderProduct.Count)
        {
            return new StatusResult(ReturnStatusCode.Conflict, "Not enough stock");
        }

        await _dbContext.OrderProducts.AddAsync(new OrderProduct()
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Price = product.Price,
            Count = orderProduct.Count,
        });
        order.TotalPrice += product.Price * orderProduct.Count;

        await _dbContext.SaveChangesAsync();
        return new StatusResult(ReturnStatusCode.Success, "Order success");
    }

    public async Task<StatusResultParametrs<ProductDtoUser>> PatchOrderResult(OrderProductDto orderProduct, string userId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId);
        if (order == null)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.BadRequest,"No order found", null);
        }
        return await PatchOrder(orderProduct, order);
    }

    public async Task<StatusResultParametrs<ProductDtoUser>> PatchOrderAdmin(OrderProductDto orderProduct, int id)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.BadRequest,"No order found", null);
        }
        return await PatchOrder(orderProduct, order);
    }

    
    private async Task<StatusResultParametrs<ProductDtoUser>> PatchOrder(OrderProductDto orderProductDto, Order order)
    {
        var product = await _dbContext.Products.FindAsync(orderProductDto.ProductId);

        if (product == null)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.BadRequest,"No product found", null);
        }

        if (order.Status)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.Conflict, "Order already completed", null);
        }

        if (product.IsDeleted)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.Deleted,"Product deleted", null);
        }

        if (!product.IsActive)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.Conflict, "Product not active", null);
        }

        var orderProduct = await _dbContext.OrderProducts.FirstOrDefaultAsync(op => op.ProductId == orderProductDto.ProductId && op.OrderId == order.Id);
        if (orderProduct == null)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.BadRequest,"No order found", null);
        }

        int newCount = orderProductDto.Count;

        if (newCount <= 0)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.BadRequest, "Not Valid Parameters", null);
        }

        if (newCount > product.QuantityInStock)
        {
            return new StatusResultParametrs<ProductDtoUser>(ReturnStatusCode.Conflict, "Not enough stock", null);
        }

        orderProduct.Count = newCount;
        
        await _dbContext.SaveChangesAsync();

        // Пересчёт всей суммы
        order.TotalPrice = await _dbContext.OrderProducts
            .Where(op => op.OrderId == order.Id)
            .SumAsync(op => op.Count * op.Product.Price);


        await _dbContext.SaveChangesAsync();
        return new StatusResultParametrs<ProductDtoUser>(
            ReturnStatusCode.Success,
            "Success",
            new ProductDtoUser
            {
                Id = orderProduct.Product.Id,
                Count = orderProduct.Count,
                Price = orderProduct.Price,
                ImgPatch = orderProduct.Product.ImgPath,
                Name = orderProduct.Product.Name,
                TotalPrice = order.TotalPrice
            }
        );
    }

    public async Task<OrderProductYouDto?> GetAllProduct(string userId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId);
        if (order == null)
        {
            return null;
        }

        List<OrderProductListDto> products = await _dbContext.OrderProducts
            .Where(op => op.OrderId == order.Id)
            .Select(op => new OrderProductListDto
            {
                Product = new OrderProductListOnListDto
                {
                    ProductId = op.Product.Id,
                    ProductName = op.Product.Name,
                    ImgPath = op.Product.ImgPath,
                    ProductQuantityInStock = op.Product.QuantityInStock,
                },
                Price = op.Price,
                Count = op.Count,
            })
            .ToListAsync();


        var orderProductYou = new OrderProductYouDto()
        {
            TotalPrice = order.TotalPrice,
            OredersProducts = products,
        };

        return orderProductYou;
    }

    public async Task<StatusResultParametrs<decimal?>> DeleteProduct(string userId, int productId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId);
        if (order == null)
        {
            return new StatusResultParametrs<decimal?>(ReturnStatusCode.NotFound, "No order found", null);
        }

        if (order.Status)
        {
            return new StatusResultParametrs<decimal?>(ReturnStatusCode.Conflict, "Order already completed", null);
        }

        var productDelete = await _dbContext.OrderProducts.FirstOrDefaultAsync(op => op.ProductId == productId && op.OrderId == order.Id);
        if (productDelete == null)
        {
            return new StatusResultParametrs<decimal?>(ReturnStatusCode.BadRequest, "No product found", null);
        }
        
        _dbContext.OrderProducts.Remove(productDelete);
        await _dbContext.SaveChangesAsync();
        
        order.TotalPrice = await _dbContext.OrderProducts
            .Where(op => op.OrderId == order.Id)
            .SumAsync(op => op.Count * op.Product.Price);
        await _dbContext.SaveChangesAsync();
        
        return new StatusResultParametrs<decimal?>(ReturnStatusCode.Success, "Success", order.TotalPrice);

        
    }
}