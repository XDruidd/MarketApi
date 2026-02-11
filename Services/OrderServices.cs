using Market.Models;
using Market.Models.Market;
using Market.Models.Market.FromBody;
using Market.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Market.Services;

public class OrderServices : IOrderServices
{ 
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OrderServices> _logger;
    private AppDbContext dbcontext;

    public OrderServices(AppDbContext dbcontext)
    {
        this.dbcontext = dbcontext;
    }

    public OrderServices(AppDbContext dbContext, ILogger<OrderServices> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task CreateAсcaunt(string userId)
    {
        await _dbContext.Orders.AddAsync(new Order() { UserId = userId });
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<List<OrderProductListDtoPeople>> GetAllOrder(int pageNumber = 1, int pageSize = 10)
    {
        var products = await _dbContext.Orders.Select(o => new OrderProductListDtoPeople()
        {
            Id = o.Id,
            TotalPrice = o.TotalPrice,
            Status = o.Status,
            OrderProducts = _dbContext.OrderProducts.Where(op => op.OrderId == o.Id).Select(op => new AdminProductDto()
            {
                Id = op.Product.Id,
                Name = op.Product.Name,
                Price = op.Product.Price,
                Count = op.Count,
                QuantityInStock = op.Product.QuantityInStock,
            }).ToList()
        })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return products;
    }

    public async Task<OrderProductListDtoPeople> GetOneOrders(int id)
    {
        var products = await _dbContext.Orders
            .Where(o => o.Id == id)
            .Select(o => new OrderProductListDtoPeople()
            {
                Id = o.Id,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                OrderProducts = _dbContext.OrderProducts.Where(op => op.OrderId == o.Id).Select(op => new AdminProductDto()
                {
                    Id = op.Product.Id,
                    Name = op.Product.Name,
                    Price = op.Product.Price,
                    Count = op.Count,
                    QuantityInStock = op.Product.QuantityInStock,
                }).ToList()
            })
            .FirstOrDefaultAsync();
        
        return products;
    }
    public async Task<int> GetOrdersCountPage(int pageSize = 10)
    {
        var ordersCount = await _dbContext.Orders.CountAsync();
        var totalPages = (int)Math.Ceiling((double)ordersCount / pageSize);
        return totalPages;
    }

    public async Task<StatusResult> PathStatus(int id, bool isActive)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
            return new StatusResult(ReturnStatusCode.BadRequest, "Order not found");
        }

        if (!isActive)
        {
            return new StatusResult(ReturnStatusCode.Conflict, "Order now is status");
        }
        if (order.Status)
        {
            return new StatusResult(ReturnStatusCode.Conflict, "Conflict station, Order already active");
        }

        var productSell = await _dbContext.OrderProducts
            .Where(op => op.OrderId == order.Id)
            .Include(op => op.Product)
            .ToListAsync();
        
        foreach (var op in productSell)
        {
            if (op.Product == null)
            {
                return new StatusResult(ReturnStatusCode.NotFound, "Product not found");
            }
            
            if (op.Count > op.Product.QuantityInStock)
            {
                return (new StatusResult(ReturnStatusCode.Conflict,  $"Not enough stock for product '{op.Product.Name}'"));
            }

            if (!op.Product.IsActive || op.Product.IsDeleted)
            {
                return new StatusResult(ReturnStatusCode.Conflict, $"Product '{op.Product.Name}' is not found");
            }
        }

        foreach (var op in productSell)
        {
            if (op.Product == null) {return new StatusResult(ReturnStatusCode.NotFound, "Product not found"); }
            op.Product.QuantityInStock -= op.Count;
        }
        order.Status = isActive;
        await _dbContext.SaveChangesAsync();

        return new StatusResult(ReturnStatusCode.Success,"Order status updated");
    }
}