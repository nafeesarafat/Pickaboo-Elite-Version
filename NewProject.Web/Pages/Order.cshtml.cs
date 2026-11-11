using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class OrdersModel : PageModel
{
    private readonly ShoppingApiClient _api;

    public OrdersModel(ShoppingApiClient api)
    {
        _api = api;
    }

    public List<OrderDto> Orders { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(
        CancellationToken ct)
    {
        string? token =
            HttpContext.Session.GetString("AuthToken");

        if (string.IsNullOrWhiteSpace(token) ||
            !string.Equals(
                HttpContext.Session.GetString("UserRole"),
                "Customer",
                StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/LoginPage");
        }

        try
        {
            Orders = await _api.GetOrdersAsync(token, ct);

            return Page();
        }
        catch (HttpRequestException)
        {
            return Page();
        }
    }
}