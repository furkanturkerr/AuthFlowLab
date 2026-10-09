namespace AuthFlowLab.API.Dtos.UserDtos;

public class UserListDto
{
    public string Id { get; set; } = string.Empty;
    public string NameSurname { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}
