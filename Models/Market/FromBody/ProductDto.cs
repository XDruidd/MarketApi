namespace Market.Models.Market.FromBody;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Price { get; set; }
    public int QuantityInStock { get; set; }
    
    public string ImgPatch { get; set; }
}

public class AdminProductDto : ProductDto
{
    public int Count { get; set; }
}