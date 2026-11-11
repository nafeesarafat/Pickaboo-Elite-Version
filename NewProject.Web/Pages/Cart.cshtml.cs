using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class CartModel : PageModel
{
    private readonly ShoppingApiClient _api;

    public CartModel(ShoppingApiClient api)
    {
        _api = api;
    }

    public CartDto Cart { get; set; } = new();

    public string? ErrorMessage { get; set; }

    private string? Token =>
        HttpContext.Session.GetString("AuthToken");

    private bool IsCustomer =>
        !string.IsNullOrWhiteSpace(Token) &&
        string.Equals(
            HttpContext.Session.GetString("UserRole"),
            "Customer",
            StringComparison.OrdinalIgnoreCase);

    public string GetImageUrl(string? path)
    {
        return _api.ImageUrl(path);
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken ct)
    {
        if (!IsCustomer)
            return RedirectToPage("/LoginPage");

        try
        {
            Cart = await _api.GetCartAsync(Token!, ct);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        int id,
        int quantity,
        CancellationToken ct)
    {
        if (!IsCustomer)
            return RedirectToPage("/LoginPage");

        try
        {
            await _api.UpdateCartItemAsync(
                id, quantity, Token!, ct);
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAsync(
        int id,
        CancellationToken ct)
    {
        if (!IsCustomer)
            return RedirectToPage("/LoginPage");

        try
        {
            await _api.RemoveCartItemAsync(
                id, Token!, ct);
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }
}