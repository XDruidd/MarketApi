using Market.Models.Market;
using Market.Models.Market.FromBody;
using Market.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductServices _productServices;
    
    public ProductController(ILogger<ProductController> logger, IProductServices productServices)
    {
        _logger = logger;
        _productServices = productServices;
    }

    [HttpGet("page/count")]
    public async Task<IActionResult> GetPage()
    {
        return Ok(new {countPage = await _productServices.GetProductsCountPage()});
    }
    
    [HttpGet("{pageId}")]
    public async Task<IActionResult> Get(int pageId)
    {
        if (pageId < 1 || pageId > await _productServices.GetProductsCountPage()) { return BadRequest(); } 
        
        var products = await _productServices.GetProducts(pageId);
        if (products.Count == 0) { return NoContent(); }
        
        return Ok(products);
    }

    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] ProductFromBody product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var created = await _productServices.PostProduct(product);
        return StatusCode(201, created);
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productServices.DeleteProduct(id);
        if(deleted){ return Ok(new {message = "Deleted"} ); }
        
        return BadRequest(new { message = "Not found" });
    }

    [HttpPatch("{pageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Patch(int pageId, [FromBody] UpdateProductDto product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
     
        var path = await _productServices.PathProduct(pageId, product);
        if (path) { return Ok(new {message = "Updated"}); }
        
        return BadRequest();
    }
    
}