namespace AuthFlowLab.WebUI.Dtos.LoginDtos;

public class LoginResponseDto
{
    public bool Succeeded { get; set; }

    public string Message { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public string UserId { get; set; } = string.Empty;
    public string NameSurname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}