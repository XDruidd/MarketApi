using Market.Models.Market;
using Market.Models;
using Market.Models.Market.FromBody;

namespace Market.Services.Interface;

public interface IOrderProductServices
{
    Task<StatusResult> PostOrder(OrderProductDto orderProduct, string userId);
    Task<OrderProductYouDto?> GetAllProduct(string userId);
    Task<StatusResult> PatchOrderResult(OrderProductDto orderProduct, string userId, bool isPlus);
    Task<StatusResult> PatchOrderAdmin(OrderProductDto orderProduct, int id, bool isPlus);

}