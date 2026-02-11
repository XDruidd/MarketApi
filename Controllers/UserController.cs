using Market.Services;
using Market.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserServices _userServices;

    public UserController(ILogger<UserController> logger, IUserServices userServices)
    {
        _logger = logger;
        _userServices = userServices;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Get()
    {
        var user = await _userServices.GetAllUsers();
        if (user.Count == 0) { return NoContent(); }
        
        return Ok(user);
    }

}