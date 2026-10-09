using AuthFlowLab.API.Dtos.LoginDtos;

namespace AuthFlowLab.API.Services.LoginServices;

public interface ILoginService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
}