using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewProject.Api.Data;
using NewProject.Api.Models;

namespace NewProject.Api.Services;

public class AdminAccountSeeder
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _hasher = new();

    public AdminAccountSeeder(
        AppDbContext db,
        IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task SeedAsync()
    {
        string email = _config["AdminAccount:Email"]
            ?? throw new InvalidOperationException(
                "Admin email is missing.");

        string password = _config["AdminAccount:Password"]
            ?? throw new InvalidOperationException(
                "Admin password is missing.");

        string fullName =
            _config["AdminAccount:FullName"] ?? "Administrator";

        email = email.Trim().ToLowerInvariant();

        if (password.Length < 8)
        {
            throw new InvalidOperationException(
                "Admin password must be at least 8 characters.");
        }

        // Check if admin account already exists
        var existingUser = await _db.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            if (existingUser.Role != "Admin")
            {
                throw new InvalidOperationException(
                    "The configured admin email belongs to a non-admin account.");
            }

            return;
        }

        // Create new admin account
        var admin = new User
        {
            FullName = fullName,
            Email = email,
            Role = "Admin",
            IsEmailVerified = true
        };

        // Hash password before saving
        admin.PasswordHash = _hasher.HashPassword(
            admin, password);

        _db.Users.Add(admin);

        await _db.SaveChangesAsync();
    }
}