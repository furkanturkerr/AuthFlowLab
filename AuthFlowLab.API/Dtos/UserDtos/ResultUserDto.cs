namespace AuthFlowLab.API.Dtos.UserDtos;

public class ResultUserDto
{
    public string Id { get; set; } = string.Empty;
    public string NameSurname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}