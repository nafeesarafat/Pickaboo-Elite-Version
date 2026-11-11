using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Models;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class CategoriesModel : PageModel
{
    private readonly ShoppingApiClient _api;

    public CategoriesModel(ShoppingApiClient api)
    {
        _api = api;
    }

    public List<CategoryDto> Categories { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Categories = await _api.GetCategoriesAsync(ct);
    }
}