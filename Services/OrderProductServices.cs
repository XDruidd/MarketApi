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
            if ((orderPId.Count + orderProduct.Count) > product.QuantityInStock)
            {
                return new StatusResult(ReturnStatusCode.Conflict, "Not enough stock");
            }

            orderPId.Count += orderProduct.Count;
            order.TotalPrice += orderProduct.Count * product.Price;
            await _dbContext.SaveChangesAsync();
            return new StatusResult(ReturnStatusCode.Success, "Order success");
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

    public async Task<StatusResult> PatchOrderResult(OrderProductDto orderProduct, string userId, bool isPlus)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId);
        if (order == null)
        {
            return new StatusResult(ReturnStatusCode.BadRequest,"No order found");
        }
        return await PatchOrder(orderProduct, order, isPlus);
    }

    public async Task<StatusResult> PatchOrderAdmin(OrderProductDto orderProduct, int id, bool isPlus)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
            return new StatusResult(ReturnStatusCode.BadRequest,"No order found");
        }
        return await PatchOrder(orderProduct, order, isPlus);
    }

    
    private async Task<StatusResult> PatchOrder(OrderProductDto orderProductDto, Order order, bool isPlus)
    {
        var product = await _dbContext.Products.FindAsync(orderProductDto.ProductId);

        if (product == null)
        {
            return new StatusResult(ReturnStatusCode.BadRequest,"No product found");
        }

        if (order.Status)
        {
            return new StatusResult(ReturnStatusCode.Conflict, "Order already completed");
        }

        if (product.IsDeleted)
        {
            return new StatusResult(ReturnStatusCode.Deleted,"Product deleted");
        }

        if (!product.IsActive)
        {
            return new StatusResult(ReturnStatusCode.Conflict, "Product not active");
        }

        var orderProduct = await _dbContext.OrderProducts.FirstOrDefaultAsync(op => op.ProductId == orderProductDto.ProductId && op.OrderId == order.Id);
        if (orderProduct == null)
        {
            return new StatusResult(ReturnStatusCode.BadRequest,"No order found");
        }

        //додавання
        if (isPlus)
        {
            if ((orderProduct.Count + orderProductDto.Count) > product.QuantityInStock)
            {
                return new StatusResult(ReturnStatusCode.Conflict, "Not enough stock");
            }

            orderProduct.Count += orderProductDto.Count;
            order.TotalPrice += orderProductDto.Count * product.Price;
            await _dbContext.SaveChangesAsync();
            return new StatusResult(ReturnStatusCode.Success ,"Order successful");
        }
        
        if (orderProduct.Count - orderProductDto.Count <= 0)
        {
            //якщо воно уходе в 0 чи в -... то воно удаляється
            _dbContext.OrderProducts.Remove(orderProduct);
            order.TotalPrice -= orderProduct.Count * product.Price;
        }
        else
        {
            //якщо ні то остається і воно в корзині
            orderProduct.Count -= orderProductDto.Count;
            order.TotalPrice -= orderProductDto.Count * product.Price;
        }
        
        await _dbContext.SaveChangesAsync();
        return new StatusResult(ReturnStatusCode.Success ,"Order successful");
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
}