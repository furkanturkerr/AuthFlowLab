using AuthFlowLab.API.Dtos.UserDtos;
using AuthFlowLab.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthFlowLab.API.Services.UserServices;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;

    public UserService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserListDto>> GetUsersAsync()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.NameSurname)
            .ThenBy(user => user.Id)
            .ToListAsync();

        var userList = new List<UserListDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userList.Add(new UserListDto
            {
                Id = user.Id,
                NameSurname = user.NameSurname,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.OrderBy(role => role).ToArray()
            });
        }

        return userList;
    }
}
