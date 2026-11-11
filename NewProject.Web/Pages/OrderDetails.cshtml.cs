using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class OrderDetailsModel : PageModel
{
    private readonly ShoppingApiClient _api;

    public OrderDetailsModel(ShoppingApiClient api)
    {
        _api = api;
    }

    public OrderDto? Order { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken ct)
    {
        string? token =
            HttpContext.Session.GetString("AuthToken");

        if (string.IsNullOrWhiteSpace(token))
            return RedirectToPage("/LoginPage");

        try
        {
            Order = await _api.GetOrderAsync(
                id, token, ct);

            return Order == null
                ? NotFound()
                : Page();
        }
        catch (HttpRequestException)
        {
            return NotFound();
        }
    }
}