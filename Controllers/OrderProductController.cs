using System.Security.Claims;
using Market.Models.Market;
using Market.Models;
using Market.Models.Market.FromBody;
using Market.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderProductController : ControllerBase
{
    private readonly ILogger<OrderProductController> _logger;
    private readonly IOrderProductServices _orderProductServices;

    public OrderProductController(ILogger<OrderProductController> logger, IOrderProductServices orderProductServices)
    {
        _logger = logger;
        _orderProductServices = orderProductServices;
    }
    
    [Authorize(Roles = "Customer,Admin")]
    [HttpPost]
    public async Task<IActionResult> PostNewOrder([FromBody] OrderProductDto orderProduct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId == null) { return Unauthorized(); }
        var status = await _orderProductServices.PostOrder(orderProduct, userId);
        
        switch (status.Code)
        {
            case ReturnStatusCode.BadRequest:
            {
                return BadRequest(new { Error = status.Message });
            }
            case ReturnStatusCode.Conflict:
            {
                return Conflict(new { Error = status.Message });
            }
            case ReturnStatusCode.Deleted:
            {
                return StatusCode(410, new { Deleted = status.Message });
            }
            default:
            {
                return StatusCode(201, new{ Created = status.Message});
            }
        }
    }
    
    [Authorize(Roles = "Customer,Admin")]
    [HttpPatch]
    public async Task<IActionResult> PatchOrder([FromBody] OrderProductDto orderProduct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return BadRequest();
        }
        var status = await _orderProductServices.PatchOrderResult(orderProduct, userId);

        switch (status.Code)
        {
            case ReturnStatusCode.BadRequest:
            {
                return BadRequest(new { Error = status.Message });
            }
            case ReturnStatusCode.Conflict:
            {
                return Conflict(new { Error = status.Message });
            }
            case ReturnStatusCode.Deleted:
            {
                return StatusCode(410, new { Deleted = status.Message });
            }
            default:
            {
                return Ok(new { Product = status.Data});
            }
        }
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetOrderProducts()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var arr = await _orderProductServices.GetAllProduct(userId);

        if (arr.TotalPrice == 0)
        {
            return NoContent();
        }
        return Ok(arr);
    }
    [Authorize(Roles = "Admin")]
    [HttpPatch("Admin/{id}")]
    public async Task<IActionResult> PatchAdminOrder([FromBody] OrderProductDto orderProduct, int id)
    {
        
        var status = await _orderProductServices.PatchOrderAdmin(orderProduct, id);
        switch (status.Code)
        {
            case ReturnStatusCode.BadRequest:
            {
                return BadRequest(new { Error = status.Message });
            }
            case ReturnStatusCode.Conflict:
            {
                return Conflict(new { Error = status.Message });
            }
            case ReturnStatusCode.Deleted:
            {
                return StatusCode(410, new { Deleted = status.Message });
            }
            default:
            {
                return Ok(new { Product = status.Data});
            }
        }
        
    }

    [Authorize(Roles = "Admin,Customer")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderProduct([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return BadRequest();
        }
        
        var status = await _orderProductServices.DeleteProduct(userId, id);
        switch (status.Code)
        {
            case ReturnStatusCode.BadRequest:
            {
                return BadRequest(new { Error = status.Message });
            }
            case ReturnStatusCode.Conflict:
            {
                return Conflict(new { Error = status.Message });
            }
            case ReturnStatusCode.NotFound:
            {
                return NotFound(new { Error = status.Message });
            }
            case ReturnStatusCode.Success:
            {
                return Ok(new { TotalPrice = status.Data });
            }
        }
        return BadRequest(new { Error = status.Message });
    }
}