using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class VerifyPasswordModel : AuthPageModel
{
    public VerifyPasswordModel(AuthApiClient api) : base(api) { }

    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Code { get; set; } = "";
    [BindProperty] public string NewPassword { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";

    public string Step { get; set; } = "email";

    public void OnGet()
    {
        Step = "email";
    }

    // send reset code
    public async Task<IActionResult> OnPostSendAsync()
    {
        var ok = await CallApiAsync(
            "api/registration/forgot-password",
            new { Email });

        Step = ok ? "verify" : "email";

        return Page();
    }

    //verify reset code
    public async Task<IActionResult> OnPostVerifyAsync()
    {
        var ok = await CallApiAsync(
            "api/registration/verify-password",
            new { Email, Code });

        Step = ok ? "reset" : "verify";

        return Page();
    }

    //reset password
    public async Task<IActionResult> OnPostResetAsync()
    {

        Step = "reset";

        if (NewPassword != ConfirmPassword)
        {
            Message = "Passwords do not match.";
            return Page();
        }

        var ok = await CallApiAsync(
            "api/registration/reset-password",
            new { Email, Code, NewPassword });

        if (ok)
            return RedirectToPage("/LoginPage");

        return Page();
    }
}