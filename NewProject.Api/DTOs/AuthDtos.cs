using System.ComponentModel.DataAnnotations;

namespace NewProject.Api.DTOs;

public class RegistrationRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, MinLength(8), StringLength(128)]
    public string Password { get; set; } = "";
}

public class VerifyEmailRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, RegularExpression(@"^\d{6}$")]
    public string Code { get; set; } = "";
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = "";

    [Required, MinLength(8), StringLength(128)]
    public string NewPassword { get; set; } = "";
}

public class VerifyPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, RegularExpression(@"^\d{6}$")]
    public string Code { get; set; } = "";
}

public class ResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, RegularExpression(@"^\d{6}$")]
    public string Code { get; set; } = "";

    [Required, MinLength(8), StringLength(128)]
    public string NewPassword { get; set; } = "";
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";
}