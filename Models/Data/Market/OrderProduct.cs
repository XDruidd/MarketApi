using System.ComponentModel.DataAnnotations;

namespace Market.Models.Market;

public class OrderProduct
{
    [Key]
    public int Id { get; set; }
    
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Price { get; set; }
    public int Count { get; set; }
}