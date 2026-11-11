namespace NewProject.Api.DTOs;

public class MessageResponse
{
    public string Message { get; set; } = "";
}
public class LoginResponse  
{
    public string Token { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "Customer";
}