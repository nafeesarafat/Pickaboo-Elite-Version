using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class CheckoutModel : PageModel
{
    private readonly ShoppingApiClient _api;

    public CheckoutModel(ShoppingApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public CheckoutInput Input { get; set; } = new();

    public CartDto Cart { get; set; } = new();

    public string? ErrorMessage { get; set; }

    private string? Token =>
        HttpContext.Session.GetString("AuthToken");

    public async Task<IActionResult> OnGetAsync(
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Token))
            return RedirectToPage("/LoginPage");

        try
        {
            Cart = await _api.GetCartAsync(Token!, ct);

            if (!Cart.Items.Any())
                return RedirectToPage("/Cart");

            return Page();
        }
        catch (HttpRequestException)
        {
            return RedirectToPage("/Cart");
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
                Cart = await _api.GetCartAsync(Token!, ct);
            }
            catch
            {
                return RedirectToPage("/Cart");
            }

            return Page();
        }

        try
        {
            var result = await _api.CheckoutAsync(
                Input, Token!, ct);

            TempData["OrderNumber"] = result.OrderNumber;

            return RedirectToPage("/Order");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;

            try
            {
                Cart = await _api.GetCartAsync(Token!, ct);
            }
            catch
            {
                return RedirectToPage("/Cart");
            }

            return Page();
        }
    }
}