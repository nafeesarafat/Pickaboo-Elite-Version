using Microsoft.AspNetCore.Mvc;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class VerificationModel : AuthPageModel
{
    public VerificationModel(AuthApiClient api) : base(api) { }

    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Code { get; set; } = "";


    ///email niye page banay
    public void OnGet(string email = "")
    {
        Email = email;
    }

    /// sends verification request
    public async Task<IActionResult> OnPostAsync()
    {
        var ok = await CallApiAsync(  /// api call kore
            "api/registration/verify",
            new { Email, Code });

        if (ok)
            return RedirectToPage("/LoginPage");

        return Page();
    }
} 