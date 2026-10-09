namespace AuthFlowLab.API.Services.JwtServices;

public interface IJwtService
{
    string GenerateToken(string email, string nameSurname, string userId, IEnumerable<string>? roles = null);
}