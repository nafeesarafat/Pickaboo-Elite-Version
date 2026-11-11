using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewProject.Api.Data;
using NewProject.Api.DTOs;

namespace NewProject.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize(Roles = "Customer")]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    private int CustomerId =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);

    // GET PROFILE
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(
        CancellationToken ct)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == CustomerId, ct);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.CreatedAt
        });
    }

    // UPDATE PROFILE
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(
        UpdateProfileRequest request,
        CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(
            new object[] { CustomerId }, ct);

        if (user == null)
            return NotFound();

        user.FullName = request.FullName.Trim();

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "Profile updated successfully.",
            user.FullName,
            user.Email
        });
    }
}