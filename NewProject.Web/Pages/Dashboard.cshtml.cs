using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class DashboardModel : PageModel
{
    private readonly ProductApiClient _products;

    public DashboardModel(ProductApiClient products)
    {
        _products = products;
    }

    public List<ProductDto> Products { get; private set; } = [];

    public string FullName { get; private set; } = "Administrator";

    public string? ErrorMessage { get; private set; }

    public string GetImageUrl(string? path)
        => _products.ImageUrl(path);

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken ct)
    {
        if (!IsAdmin())
            return RedirectToPage("/LoginPage");

        FullName =
            HttpContext.Session.GetString("UserName")
            ?? "Administrator";

        try
        {
            string token =
                HttpContext.Session.GetString("AuthToken")!;

            Products = await _products.GetAllAsync(token, ct);

            // SEARCH
            if (!string.IsNullOrWhiteSpace(Search))
            {
                Products = Products
                    .Where(p =>
                        p.Name.Contains(
                            Search,
                            StringComparison.OrdinalIgnoreCase)) //ignore the uppercase lower case
                    .ToList();
            }

            // STATUS FILTER
            if (Status == "active")
            {
                Products = Products
                    .Where(p => p.IsActive)
                    .ToList();
            }

            if (Status == "inactive")
            {
                Products = Products
                    .Where(p => !p.IsActive)
                    .ToList();
            }

            return Page();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    // DELETE PRODUCT
    public async Task<IActionResult> OnPostDeleteAsync(
        int id,
        CancellationToken ct)
    {
        if (!IsAdmin())
            return RedirectToPage("/LoginPage");

        try
        {
            string token =
                HttpContext.Session.GetString("AuthToken")!;

            await _products.DeleteAsync(id, token, ct);

            TempData["SuccessMessage"] =
                "Product deleted successfully.";
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    // LOGOUT
    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();

        return RedirectToPage("/LoginPage");
    }

    private bool IsAdmin()
    {
        string? token =
            HttpContext.Session.GetString("AuthToken");

        string? role =
            HttpContext.Session.GetString("UserRole");

        return !string.IsNullOrWhiteSpace(token)
            &&
            string.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase);
    }
}
