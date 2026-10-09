using System.IdentityModel.Tokens.Jwt;
using AuthFlowLab.API.Dtos.LoginDtos;
using AuthFlowLab.API.Entities;
using AuthFlowLab.API.Services.JwtServices;
using Microsoft.AspNetCore.Identity;

namespace AuthFlowLab.API.Services.LoginServices;

public class LoginService : ILoginService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtService _jwtService;

    public LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto);

        if (string.IsNullOrWhiteSpace(loginDto.Email) ||
            string.IsNullOrWhiteSpace(loginDto.Password))
            return null;

        var user = await _userManager.FindByEmailAsync(loginDto.Email.Trim());

        if (user == null)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
            return null;

        if (await _userManager.GetTwoFactorEnabledAsync(user))
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var email = user.Email ?? loginDto.Email.Trim();
        var accessToken = _jwtService.GenerateToken(
            email, user.NameSurname, user.Id, roles);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        return new LoginResponseDto
        {
            Succeeded = true,
            Message = "Login successful",
            AccessToken = accessToken,
            ExpiresAtUtc = new DateTimeOffset(token.ValidTo),
            UserId = user.Id,
            NameSurname = user.NameSurname,
            Email = email,
            Roles = roles.ToArray()
        };
    }
}
