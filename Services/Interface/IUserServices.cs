using Market.Models.Market.FromBody;

namespace Market.Services.Interface;

public interface IUserServices
{
    Task<List<UserDto>> GetAllUsers();
}