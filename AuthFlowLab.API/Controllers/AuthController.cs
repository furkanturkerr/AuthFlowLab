using AuthFlowLab.API.Dtos.LoginDtos;
using AuthFlowLab.API.Dtos.RegisterDtos;
using AuthFlowLab.API.Services.LoginServices;
using AuthFlowLab.API.Services.RegisterServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthFlowLab.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IRegisterService _registerService;
    private readonly ILoginService _loginService;

    public AuthController(IRegisterService registerService, ILoginService loginService)
    {
        _registerService = registerService;
        _loginService = loginService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDto>> Register(RegisterDto registerDto)
    {
        var response = await _registerService.RegisterAsync(registerDto);
        if (!response.Succeeded)
            return BadRequest(response);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        var response = await _loginService.LoginAsync(loginDto);
        if (response == null)
        {
            return Unauthorized(new LoginResponseDto
            {
                Succeeded = false,
                Message = "Giriş başarısız. E-posta ve şifrenizi kontrol ediniz."
            });
        }

        return Ok(response);
    }
}
