using System.ComponentModel.DataAnnotations;

namespace Market.Models.Market.FromBody;

public class OrderProductDto
{
    public int ProductId { get; set; }
    [Range(1, int.MaxValue)]
    public int Count { get; set; }
}
public class OrderProductListOnListDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int ProductQuantityInStock { get; set; }
}


public class OrderProductListDto
{
    public OrderProductListOnListDto Product { get; set; }
    public decimal Price { get; set; }
    public int Count { get; set; }
}

public class OrderProductYouDto
{
    public decimal TotalPrice {get; set;}
    
    public List<OrderProductListDto> OredersProducts { get; set; }  
}

public class OrderProductListDto1 : OrderProductListDto
{
    public int OrderId { get; set; }
}

public class OrderProductListDtoPeople(){
    public int Id { get; set; }
    public decimal TotalPrice { get; set; }
    public bool Status { get; set; }
    
    public List<AdminProductDto> OrderProducts { get; set; }
}