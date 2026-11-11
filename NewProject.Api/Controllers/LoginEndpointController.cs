using Microsoft.AspNetCore.Mvc;
using NewProject.Api.DTOs;
using NewProject.Api.Services;

namespace NewProject.Api.Controllers;

[ApiController]
[Route("api/login")]
public class LoginEndpointController : ControllerBase
{
    private readonly AuthService _auth;

    public LoginEndpointController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _auth.LoginAsync(request, cancellationToken);

            return Ok(new LoginResponse
            {
                Token = result.Token,
                Name = result.Name,
                Role = result.Role
            });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new MessageResponse
            { Message = ex.Message });
        }
    }
}