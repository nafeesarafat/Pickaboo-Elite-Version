
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class ProductFormModel : PageModel
{
    private readonly ProductApiClient _products;
    private readonly ShoppingApiClient _shopping;

    public ProductFormModel(
        ProductApiClient products,
        ShoppingApiClient shopping)
    {
        _products = products;
        _shopping = shopping;
    }

    [BindProperty]
    public ProductFormInput Input { get; set; } = new();

    public string? ExistingImageUrl { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool IsEdit { get; private set; }

    public int CategoryId { get; set; }
    public int BrandId { get; set; }

    // Category and Brand dropdown data
    public List<CategoryDto> Categories { get; private set; } = [];

    public List<BrandDto> Brands { get; private set; } = [];

    // GET: ADD OR EDIT PRODUCT
    public async Task<IActionResult> OnGetAsync(
        int? id,
        CancellationToken ct)
    {
        if (!IsAdmin())
            return RedirectToPage("/LoginPage");

        try
        {
            // Load categories and brands first
            await LoadCatalogAsync(ct);

            // Add new product
            if (id is null)
                return Page();

            // Edit existing product
            var product = await _products.GetByIdAsync(
                id.Value,
                Token(),
                ct);

            if (product is null)
                return NotFound();

            IsEdit = true;

            Input = new ProductFormInput
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,

                // Existing category and brand selection
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,

                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive
            };

            ExistingImageUrl =
                _products.ImageUrl(product.ImageUrl);

            return Page();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    // LOAD CATEGORY AND BRAND DATA
    private async Task LoadCatalogAsync(
        CancellationToken ct)
    {
        Categories = await _shopping.GetCategoriesAsync(ct);

        Brands = await _shopping.GetBrandsAsync(ct);
    }

    // POST: SAVE PRODUCT
    public async Task<IActionResult> OnPostAsync(
        int? id,
        CancellationToken ct)
    {
        if (!IsAdmin())
            return RedirectToPage("/LoginPage");

        IsEdit = id.HasValue;

        // Validation check
        if (!ModelState.IsValid)
        {
            await LoadCatalogAsync(ct);

            ExistingImageUrl =
                _products.ImageUrl(Input.ImageUrl);

            return Page();
        }

        try
        {
            ProductDto saved;

            if (id.HasValue)
            {
                // Update existing product
                saved = await _products.UpdateAsync(
                    id.Value,
                    Input,
                    Token(),
                    ct);
            }
            else
            {
                // Create new product
                saved = await _products.CreateAsync(
                    Input,
                    Token(),
                    ct);
            }

            // Product saved successfully
            return RedirectToPage("/Dashboard");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;

            // Reload dropdown data before rendering page
            await LoadCatalogAsync(ct);

            ExistingImageUrl =
                _products.ImageUrl(Input.ImageUrl);

            return Page();
        }
    }

    // GET AUTH TOKEN
    private string Token()
    {
        return HttpContext.Session.GetString("AuthToken")!;
    }

    // CHECK ADMIN LOGIN
    private bool IsAdmin()
    {
        return
            !string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("AuthToken"))
            &&
            string.Equals(
                HttpContext.Session.GetString("UserRole"),
                "Admin",
                StringComparison.OrdinalIgnoreCase);
    }
}
