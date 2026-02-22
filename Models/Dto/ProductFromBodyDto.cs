using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Market.Models.Market.FromBody;

public class ProductFromBodyDto
{
    [Required]
    [MinLength(3)]
    public string Name { get; set; }
    
    [Required]
    [Range(0.1, int.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public IFormFile? Image { get; set; } 
}