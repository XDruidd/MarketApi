using Market.Models.Market;
using Market.Models;
using Market.Models.Market.FromBody;

namespace Market.Services.Interface;

public interface IOrderProductServices
{
    Task<StatusResult> PostOrder(OrderProductDto orderProduct, string userId);
    Task<OrderProductYouDto?> GetAllProduct(string userId);
    Task<StatusResultParametrs<ProductDtoUser>> PatchOrderResult(OrderProductDto orderProduct, string userId);
    Task<StatusResultParametrs<ProductDtoUser>> PatchOrderAdmin(OrderProductDto orderProduct, int id);
    Task<StatusResultParametrs<decimal?>> DeleteProduct(string userId, int productId);
}