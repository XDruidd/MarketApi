using Market.Models;
using Market.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class OrderController : Controller
{
    private readonly ILogger<OrderProductController> _logger;
    private readonly IOrderServices _orderServices;

    public OrderController(ILogger<OrderProductController> logger, IOrderServices orderServices)
    {
        _logger = logger;
        _orderServices = orderServices;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("GetAllOrders/{page}")]
    public async Task<IActionResult> GetAllOrders(int page)
    {
        var orders = await _orderServices.GetAllOrder(page);
        if (!orders.Any()){return NoContent();}
        
        return Ok(orders);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("pagenum")]
    public async Task<IActionResult> GetPage()
    {
        var orders = await _orderServices.GetOrdersCountPage();
        return Ok(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("GetOrders/{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderServices.GetOneOrders(id);
        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }
        
        return Ok(order);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("Status/{id}/{isStatus}")]
    public async Task<IActionResult> PathStatus(int id, bool isStatus)
    {
        var status = await _orderServices.PathStatus(id, isStatus);

        switch (status.Code)
        {
            case ReturnStatusCode.Conflict:
            {
                return Conflict(new { message = status.Message });
            }
            case ReturnStatusCode.BadRequest:
            {
                return BadRequest(new { message = status.Message });
            }
            case ReturnStatusCode.NotFound:
            {
                return NotFound(new { message = status.Message });
            }
            case ReturnStatusCode.Success:
            {
                return Ok(new { message = status.Message });
            }
            default:
            {
                return StatusCode(500, new { message = "Not understand Error" } );
            }
        }
    }
}