using AuthFlowLab.API.Dtos.UserDtos;

namespace AuthFlowLab.API.Services.UserServices;

public interface IUserService
{
    Task<List<UserListDto>> GetUsersAsync();
}
