using AuthFlowLab.API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthFlowLab.API.Contexts;

public class AuthContext : IdentityDbContext<AppUser, AppRole, string>
{
    public AuthContext(DbContextOptions<AuthContext> options) : base(options)
    {
    }
}