using System.ComponentModel.DataAnnotations;
using AuthFlowLab.API.Dtos.RegisterDtos;
using AuthFlowLab.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthFlowLab.API.Services.RegisterServices;

public class RegisterService : IRegisterService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public RegisterService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        ArgumentNullException.ThrowIfNull(registerDto);

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(registerDto, new ValidationContext(registerDto),
                validationResults, validateAllProperties: true))
        {
            return new RegisterResponseDto
            {
                Message = "Kayıt bilgileri geçersiz.",
                Errors = validationResults.Select(result => result.ErrorMessage ?? "Geçersiz alan.").ToArray()
            };
        }

        // İlk kayıt için User rolü bulunmalıdır; istemciden rol alınmaz.
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            var roleResult = await _roleManager.CreateAsync(new AppRole { Name = "User" });
            if (!roleResult.Succeeded && !await _roleManager.RoleExistsAsync("User"))
            {
                return new RegisterResponseDto
                {
                    Message = "Varsayılan kullanıcı rolü oluşturulamadı.",
                    Errors = roleResult.Errors.Select(error => error.Description).ToArray()
                };
            }
        }

        var email = registerDto.Email.Trim();
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            NameSurname = registerDto.NameSurname.Trim()
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return new RegisterResponseDto
            {
                Message = "Kayıt oluşturulamadı.",
                Errors = result.Errors.Select(error => error.Description).ToArray()
            };
        }

        var roleAssignment = await _userManager.AddToRoleAsync(user, "User");
        if (!roleAssignment.Succeeded)
        {
            return new RegisterResponseDto
            {
                Message = "Hesap oluşturuldu ancak kullanıcı rolü atanamadı.",
                Errors = roleAssignment.Errors.Select(error => error.Description).ToArray()
            };
        }

        return new RegisterResponseDto
        {
            Succeeded = true,
            Message = "Kayıt başarılı.",
            UserId = user.Id
        };
    }
}
