
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class IndexModel : PageModel
{
    private readonly ProductApiClient _products;
    private readonly ShoppingApiClient _shopping;

    public IndexModel(
        ProductApiClient products,
        ShoppingApiClient shopping)
    {
        _products = products;
        _shopping = shopping;
    }

    public List<ProductDto> Products { get; set; } = [];

    public List<CategoryDto> Categories { get; set; } = [];

    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? BrandId { get; set; }

    public string GetImageUrl(string? path)
        => _products.ImageUrl(path);

    public string GetCategoryIcon(string? categoryName)
    {
        var name = categoryName?.ToLowerInvariant() ?? "";

        if (name.Contains("mobile") || name.Contains("phone"))
            return "smartphone";

        if (name.Contains("laptop") || name.Contains("computer"))
            return "laptop";

        if (name.Contains("headphone") || name.Contains("audio"))
            return "headphones";

        if (name.Contains("watch"))
            return "watch";

        if (name.Contains("accessor"))
            return "cable";

        if (name.Contains("home") || name.Contains("appliance"))
            return "house";

        if (name.Contains("fashion") || name.Contains("cloth"))
            return "shirt";

        if (name.Contains("camera"))
            return "camera";

        return "package";
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken ct)
    {
        string? role =
            HttpContext.Session.GetString("UserRole");

        if (string.Equals(
            role,
            "Admin",
            StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Dashboard");
        }

        try
        {
            Products = await _shopping.GetProductsAsync(
                CategoryId,
                BrandId,
                Search,
                ct);
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Unable to load products. Please try again later.";
        }

        try
        {
            Categories = await _shopping.GetCategoriesAsync(ct);
        }
        catch (HttpRequestException)
        {
            // Homepage products remain available even
            // when category loading fails.
            Categories = [];
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(
        int productId,
        CancellationToken ct)
    {
        string? token =
            HttpContext.Session.GetString("AuthToken");

        string? role =
            HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrWhiteSpace(token) ||
            !string.Equals(
                role,
                "Customer",
                StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/LoginPage");
        }

        try
        {
            await _shopping.AddToCartAsync(
                new AddCartItemRequest
                {
                    ProductId = productId,
                    Quantity = 1
                },
                token,
                ct);

            TempData["SuccessMessage"] =
                "Product added to your cart.";

            return RedirectToPage();
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();

        return RedirectToPage("/Index");
    }
}
