using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class BrandsModel : PageModel
{
    private readonly ShoppingApiClient _api;

    public BrandsModel(ShoppingApiClient api)
    {
        _api = api;
    }

    public List<BrandDto> Brands { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Brands = await _api.GetBrandsAsync(ct);
    }
}