using Microsoft.AspNetCore.Mvc;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class ChangePasswordModel : AuthPageModel
{
    public ChangePasswordModel(AuthApiClient api) : base(api) { }

    [BindProperty] public string CurrentPassword { get; set; } = "";
    [BindProperty] public string NewPassword { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";

    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(
            HttpContext.Session.GetString("AuthToken")))
            return RedirectToPage("/LoginPage");

        return Page();
    }

    // new password request pathay
    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            Message = "Passwords do not match.";
            return Page();
        }

        await CallApiAsync(   /// api call kore
            "api/registration/change-password",
            new { CurrentPassword, NewPassword },
            authenticated: true);

        return Page();
    }
}