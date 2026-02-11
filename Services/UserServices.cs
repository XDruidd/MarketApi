using Market.Models.Market.FromBody;
using Market.Models;
using Market.Services.Interface;
using Microsoft.EntityFrameworkCore;
using FromBody_UserDto = Market.Models.Market.FromBody.UserDto;
using UserDto = Market.Models.Market.FromBody.UserDto;

namespace Market.Services;

public class UserServices : IUserServices
{
    private readonly AuthDbContext _dbContext;
    private readonly ILogger<UserServices> _logger;
    
    
    public UserServices(AuthDbContext dbContext, ILogger<UserServices> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<FromBody_UserDto>> GetAllUsers()
    {
        return await _dbContext.Users
            .Select(u => new FromBody_UserDto
            {
                UserName= u.UserName,
                Email = u.Email,
                Phone = u.PhoneNumber,
            })
            .ToListAsync();
    }
}