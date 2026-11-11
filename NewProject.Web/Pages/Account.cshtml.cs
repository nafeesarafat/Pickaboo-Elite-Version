
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class AccountModel : PageModel
{
    private readonly ShoppingApiClient _api;

    public AccountModel(ShoppingApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public UpdateProfileInput Input { get; set; } = new();

    public string Email { get; set; } = "";

    public string? Message { get; set; }

    private string? Token =>
        HttpContext.Session.GetString("AuthToken");

    public async Task<IActionResult> OnGetAsync(
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Token))
            return RedirectToPage("/LoginPage");

        try
        {
            var profile = await _api.GetProfileAsync(
                Token!, ct);

            Input.FullName = profile.FullName;
            Email = profile.Email;

            return Page();
        }
        catch (HttpRequestException)
        {
            return RedirectToPage("/LoginPage");
        }
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Token))
            return RedirectToPage("/LoginPage");

        if (!ModelState.IsValid)
        {
            try
            {
                var profile = await _api.GetProfileAsync(
                    Token!, ct);

                Email = profile.Email;
            }
            catch
            {
                return RedirectToPage("/LoginPage");
            }

            return Page();
        }

        try
        {
            await _api.UpdateProfileAsync(
                Input, Token!, ct);

            HttpContext.Session.SetString(
                "UserName", Input.FullName.Trim());

            TempData["SuccessMessage"] =
                "Your profile has been updated.";

            return RedirectToPage();
        }
        catch (HttpRequestException ex)
        {
            Message = ex.Message;

            try
            {
                var profile = await _api.GetProfileAsync(
                    Token!, ct);

                Email = profile.Email;
            }
            catch
            {
                return RedirectToPage("/LoginPage");
            }

            return Page();
        }
    }

    // CUSTOMER LOGOUT
    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();

        return RedirectToPage("/Index");
    }
}
