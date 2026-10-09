namespace AuthFlowLab.API.Dtos.RegisterDtos;

public class RegisterResponseDto
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string[] Errors { get; set; } = [];
}
