using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewProject.Api.Data;
using NewProject.Api.DTOs;
using NewProject.Api.Models;

namespace NewProject.Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly GmailEmailSender _email;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthService(
        AppDbContext db,
        GmailEmailSender email,
        IConfiguration config)
    {
        _db = db;
        _email = email;
        _config = config;
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string HashCode(string code) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    private static string GenerateCode() =>
        RandomNumberGenerator.GetInt32(0, 1000000)
            .ToString("D6");

    // for comparison
    private static bool CodeMatches(string? storedHash, string code)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

    
        byte[] stored;
        byte[] submitted; // byte array

        try
        {
            stored = Convert.FromHexString(storedHash); 
            submitted = Convert.FromHexString(HashCode(code));
        }
        catch (FormatException)
        {
            return false;
        }

        return stored.Length == submitted.Length &&
               CryptographicOperations.FixedTimeEquals( // compare both arrays
                   stored, submitted);
    }


    // registartion method and verification email
    public async Task<string> RegisterAsync(
        RegistrationRequest request,
        CancellationToken ct)
    {
        string email = NormalizeEmail(request.Email);

        // Check if the email is already registered
        if (await _db.Users.AnyAsync(
            u => u.Email == email, ct))

            throw new InvalidOperationException(
                "An account with this email already exists.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            IsEmailVerified = false  // email not verified yet
        };

        user.PasswordHash = _hasher.HashPassword(
            user, request.Password);

        string code = GenerateCode();

        user.EmailVerificationCodeHash = HashCode(code);
        user.EmailVerificationExpiresAt =
            DateTime.UtcNow.AddMinutes(10);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        try
        {
            await _email.SendCodeAsync(
                email, code, "Registration", ct);
        }
        catch
        {
            user.EmailVerificationCodeHash = null;
            user.EmailVerificationExpiresAt = null;
            await _db.SaveChangesAsync(CancellationToken.None);

            throw;
        }

        return "Registration successful. Check your email for the verification code.";
    }


    // verify email 
    public async Task<string> VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken ct)
    {
        string email = NormalizeEmail(request.Email);

        var user = await _db.Users.SingleOrDefaultAsync(
            u => u.Email == email, ct);

        if (user == null)
            throw new InvalidOperationException(
                "Invalid email or verification code.");

        if (user.IsEmailVerified)
            return "Email is already verified.";

        if (user.EmailVerificationExpiresAt <= DateTime.UtcNow ||
            !CodeMatches(user.EmailVerificationCodeHash, request.Code))
        {
            throw new InvalidOperationException(
                "Invalid or expired verification code.");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationCodeHash = null;
        user.EmailVerificationExpiresAt = null;

        await _db.SaveChangesAsync(ct);

        return "Email verified successfully. You can now log in.";
    }

    /// Login Service Section
    // Login method will return jwt token and user name if login is successful
    public async Task<(string Token, string Name, string Role)> LoginAsync(
        LoginRequest request,
        CancellationToken ct)
    {
        string email = NormalizeEmail(request.Email);

        var user = await _db.Users.SingleOrDefaultAsync(
            u => u.Email == email, ct);

        if (user == null)
            throw new InvalidOperationException(
                "Invalid email or password.");

        var result = _hasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new InvalidOperationException(
                "Invalid email or password.");

        if (!user.IsEmailVerified)
            throw new InvalidOperationException(
                "Please verify your email before logging in.");

        // Generate JWT token for the user
        string token = CreateJwt(user);

        return (token, user.FullName, user.Role);
    }




    // Password Service Layer Section

    //change password
    public async Task<string> ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(
            new object[] { userId }, ct);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (_hasher.VerifyHashedPassword(
                user, user.PasswordHash, request.CurrentPassword)
            == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException(
                "Current password is incorrect.");
        }

        if (_hasher.VerifyHashedPassword(
                user, user.PasswordHash, request.NewPassword)
            != PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException(
                "New password must be different from the current password.");
        }

        user.PasswordHash = _hasher.HashPassword(
            user, request.NewPassword);

        await _db.SaveChangesAsync(ct);

        return "Password changed successfully.";
    }


    // forgot password 
    public async Task<string> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken ct)
    {
        string email = NormalizeEmail(request.Email);

        var user = await _db.Users.SingleOrDefaultAsync(
            u => u.Email == email && u.IsEmailVerified, ct);

        // Same response for all 
        const string response =
            "If a verified account exists, a reset code has been sent.";

        if (user == null)
            return response;

        string code = GenerateCode();

        user.PasswordResetCodeHash = HashCode(code);
        user.PasswordResetExpiresAt =
            DateTime.UtcNow.AddMinutes(10);

        await _db.SaveChangesAsync(ct);

        try
        {
            await _email.SendCodeAsync(
                email, code, "PasswordReset", ct);
        }
        catch
        {
            user.PasswordResetCodeHash = null;
            user.PasswordResetExpiresAt = null;
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        return response;
    }


    //verify password reset code and set new password
    public async Task<string> VerifyPasswordAsync(
        VerifyPasswordRequest request,
        CancellationToken ct)
    {
        string email = NormalizeEmail(request.Email);

        var user = await _db.Users.SingleOrDefaultAsync(
            u => u.Email == email && u.IsEmailVerified, ct);

        if (user == null ||
            user.PasswordResetExpiresAt <= DateTime.UtcNow ||
            !CodeMatches(user.PasswordResetCodeHash, request.Code))
        {
            throw new InvalidOperationException(
                "Invalid or expired reset code.");
        }

        return "Reset code is valid. You can now change your password.";
    }


    ///Reset password 
    public async Task<string> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken ct)
    {
        string email = NormalizeEmail(request.Email);

        var user = await _db.Users.SingleOrDefaultAsync(
            u => u.Email == email && u.IsEmailVerified, ct);

        if (user == null ||
            user.PasswordResetExpiresAt <= DateTime.UtcNow ||
            !CodeMatches(user.PasswordResetCodeHash, request.Code))
        {
            throw new InvalidOperationException(
                "Invalid or expired reset code.");
        }

        user.PasswordHash = _hasher.HashPassword(
            user, request.NewPassword);

        user.PasswordResetCodeHash = null;
        user.PasswordResetExpiresAt = null;

        await _db.SaveChangesAsync(ct);

        return "Password reset successfully. Please log in.";
    }





    /// create JWT token for authenticated user ///
    private string CreateJwt(User user)
    {
        string key = _config["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured.");

        // Create a symmetric security key using the secret key
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));


        // Create signing credentials using the security key and HMAC SHA256 algorithm
        var credentials = new SigningCredentials(
            signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        // token creation
        var token = new JwtSecurityToken(

            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);


        // Generate the JWT token string and return it
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
