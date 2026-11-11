using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewProject.Api.DTOs;
using NewProject.Api.Services;

namespace NewProject.Api.Controllers;

[ApiController]
[Route("api/registration")]
public class RegistrationEndpointController : ControllerBase
{
    private readonly AuthService _auth;

    public RegistrationEndpointController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("registration")]
    public async Task<IActionResult> Registration(
        RegistrationRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(new MessageResponse
            {
                Message = await _auth.RegisterAsync(request, ct)
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new MessageResponse
            { Message = ex.Message });
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(
        VerifyEmailRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(new MessageResponse
            {
                Message = await _auth.VerifyEmailAsync(request, ct)
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new MessageResponse
            { Message = ex.Message });
        }
    }




    /// Password Controller Section - Nafis



    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request, CancellationToken ct)
    {
        try
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!); 

            return Ok(new MessageResponse
            {
                Message = await _auth.ChangePasswordAsync(
                    userId, request, ct)
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new MessageResponse
            { Message = ex.Message });
        }
    }

    [HttpPost("verify-password")]
    public async Task<IActionResult> VerifyPassword(
        VerifyPasswordRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(new MessageResponse
            {
                Message = await _auth.VerifyPasswordAsync(request, ct)
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new MessageResponse
            { Message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request, CancellationToken ct)
    {
        return Ok(new MessageResponse
        {
            Message = await _auth.ForgotPasswordAsync(request, ct)
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(new MessageResponse
            {
                Message = await _auth.ResetPasswordAsync(request, ct)
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new MessageResponse
            { Message = ex.Message });
        }
    }
}