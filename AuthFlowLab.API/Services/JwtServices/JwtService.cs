using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthFlowLab.API.Services.JwtServices;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string email, string nameSurname, string userId, IEnumerable<string>? roles = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var signingKey = jwtSettings["Key"];

        if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
            throw new InvalidOperationException("JwtSettings:Key must contain at least 32 UTF-8 bytes.");

        if (string.IsNullOrWhiteSpace(jwtSettings["Issuer"]) ||
            string.IsNullOrWhiteSpace(jwtSettings["Audience"]))
            throw new InvalidOperationException("JwtSettings:Issuer and JwtSettings:Audience are required.");

        if (!int.TryParse(jwtSettings["ExpirationMinutes"], out var expirationMinutes) || expirationMinutes <= 0)
            throw new InvalidOperationException("JwtSettings:ExpirationMinutes must be a positive integer.");

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name,nameSurname),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (roles != null)
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
