using Market.Models;
using Market.Models.Market.FromBody;

namespace Market.Services.Interface;

public interface IOrderServices
{
    public Task CreateAсcaunt(string userId);
    Task<List<OrderProductListDtoPeople>> GetAllOrder(int pageNumber = 1, int pageSize = 10);
    Task<int> GetOrdersCountPage(int pageSize = 10);
    Task<OrderProductListDtoPeople> GetOneOrders(int id);
    Task<StatusResult> PathStatus(int id, bool isActive);
    Task<int> GetCoutProduct(string userId);
}