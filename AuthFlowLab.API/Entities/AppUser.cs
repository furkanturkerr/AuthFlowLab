using Microsoft.AspNetCore.Identity;

namespace AuthFlowLab.API.Entities;

public class AppUser : IdentityUser
{
    public string NameSurname { get; set; } = string.Empty;
}