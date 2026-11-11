using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class RegistrationPageModel : AuthPageModel
{
    public RegistrationPageModel(AuthApiClient api) : base(api) { }

    [BindProperty] public string FullName { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";


    // registration api call request pathay OnPostAsync()
    public async Task<IActionResult> OnPostAsync()
    {
        // Check password confirmation
        //if (Password != ConfirmPassword)
        //{
        //    ModelState.AddModelError(
        //        nameof(ConfirmPassword),
        //        "Passwords do not match.");
        //}

        //if (!ModelState.IsValid)
        //    return Page();

        //////
        var ok = await CallApiAsync(   // calls api
            "api/registration/registration",
            new { FullName, Email, Password });

        if (ok)
        {
            return RedirectToPage("/Verification",
                new { email = Email });
        }
        return Page();
    }
}