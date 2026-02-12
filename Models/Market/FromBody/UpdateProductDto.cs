using System.ComponentModel.DataAnnotations;

namespace Market.Models.Market.FromBody;

public class UpdateProductDto
{
    [MinLength(3)]
    public string? Name { get; set; }
    
    [Range(0.1, int.MaxValue)]
    public float? Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? QuantityInStock { get; set; }
    public string ImgPatch { get; set; }
    
    public bool? IsActive { get; set; } = true;
}

public class UpdateProductDto2:UpdateProductDto
{
    public int Id { get; set; }
}