namespace Market.Models.Market.FromBody;

public class ProductDtoUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Count { get; set; }
    public string ImgPatch { get; set; }
    
    public decimal TotalPrice { get; set; }
}