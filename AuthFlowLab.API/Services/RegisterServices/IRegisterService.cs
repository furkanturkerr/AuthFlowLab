using AuthFlowLab.API.Dtos.RegisterDtos;

namespace AuthFlowLab.API.Services.RegisterServices;

public interface IRegisterService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto);
}
