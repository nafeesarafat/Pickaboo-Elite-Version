using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewProject.Api.Data;
using NewProject.Api.DTOs;
using NewProject.Api.Models;

namespace NewProject.Api.Controllers;

[ApiController]
[Route("api")]
public class CatalogController : ControllerBase
{
    private readonly AppDbContext _db;

    public CatalogController(AppDbContext db)
    {
        _db = db;
    }

    // PUBLIC: GET CATEGORIES
    [AllowAnonymous]
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(
        CancellationToken ct)
    {
        var result = await _db.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    // PUBLIC: GET BRANDS
    [AllowAnonymous]
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(
        CancellationToken ct)
    {
        var result = await _db.Brands
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new BrandDto
            {
                Id = x.Id,
                Name = x.Name,
                LogoUrl = x.LogoUrl
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    // ADMIN: CREATE CATEGORY
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/categories")]
    public async Task<IActionResult> CreateCategory(
        CreateCategoryRequest request,
        CancellationToken ct)
    {
        string name = request.Name.Trim();

        if (await _db.Categories.AnyAsync(
            x => x.Name == name, ct))
        {
            return Conflict(new
            {
                message = "Category already exists."
            });
        }

        var category = new Category
        {
            Name = name,
            Description = request.Description?.Trim()
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        });
    }

    // ADMIN: CREATE BRAND
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/brands")]
    public async Task<IActionResult> CreateBrand(
        CreateBrandRequest request,
        CancellationToken ct)
    {
        string name = request.Name.Trim();

        if (await _db.Brands.AnyAsync(
            x => x.Name == name, ct))
        {
            return Conflict(new
            {
                message = "Brand already exists."
            });
        }

        var brand = new Brand
        {
            Name = name
        };

        _db.Brands.Add(brand);
        await _db.SaveChangesAsync(ct);

        return Ok(new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name
        });
    }
}