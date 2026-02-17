using System.ComponentModel.DataAnnotations;

namespace Market.Models.Market;

public class Order
{
    [Key]
    public int Id { get; set; }
    
    public string UserId { get; set; }

    public decimal TotalPrice { get; set; } = 0;
    
    public bool Status { get; set; } = false;
    
    public ICollection<OrderProduct> OrderProducts { get; set; }

}