
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewProject.Api.Data;
using NewProject.Api.DTOs;
using NewProject.Api.Models;

namespace NewProject.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    // GET ALL PRODUCTS (ADMIN)
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        CancellationToken ct)
    {
        var products = await _db.Products
    .AsNoTracking()
    .Include(p => p.Category)
    .Include(p => p.Brand)
    .OrderByDescending(p => p.CreatedAtUtc)
    .ToListAsync(ct);

        return Ok(products.Select(ToResponse).ToList());
    }


    // GET ACTIVE PRODUCTS (PUBLIC HOMEPAGE)
    // Homepage: Get all active products

    [AllowAnonymous]
    [HttpGet("/api/products")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>>
        GetPublicProducts(
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] string? search,
            CancellationToken ct)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive && p.StockQuantity > 0);

        if (categoryId.HasValue)
        {
            query = query.Where(
                p => p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            query = query.Where(
                p => p.BrandId == brandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();

            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.Description.Contains(term) ||
                p.Category.Name.Contains(term) ||
                p.Brand.Name.Contains(term));
        }

        var products = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(products.Select(ToResponse).ToList());
    }



    // GET SINGLE PRODUCT
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(
        int id,
        CancellationToken ct)
    {
        var product = await _db.Products
    .AsNoTracking()
    .Include(p => p.Category)
    .Include(p => p.Brand)
    .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
        {
            return NotFound(new MessageResponse
            {
                Message = "Product not found."
            });
        }

        return Ok(ToResponse(product));
    }


    // CREATE PRODUCT
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(
        ProductInsertRequest request,
        CancellationToken ct)
    {
        // VALIDATE CATEGORY AND BRAND
        if (!await _db.Categories.AnyAsync(
                x => x.Id == request.CategoryId && x.IsActive, ct) ||
            !await _db.Brands.AnyAsync(
                x => x.Id == request.BrandId && x.IsActive, ct))
        {
            return BadRequest(new
            {
                message = "Select a valid category and brand."
            });
        }

        // CREATE PRODUCT
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            ImageUrl = request.ImageUrl?.Trim()
        };

        _db.Products.Add(product);

        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            ToResponse(product));
    }


    // UPDATE PRODUCT
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(
        int id,
        ProductInsertRequest request,
        CancellationToken ct)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
        {
            return NotFound(new MessageResponse
            {
                Message = "Product not found."
            });
        }

        // VALIDATE CATEGORY AND BRAND
        if (!await _db.Categories.AnyAsync(
                x => x.Id == request.CategoryId && x.IsActive, ct) ||
            !await _db.Brands.AnyAsync(
                x => x.Id == request.BrandId && x.IsActive, ct))
        {
            return BadRequest(new
            {
                message = "Select a valid category and brand."
            });
        }

        // UPDATE PRODUCT INFORMATION
        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.UpdatedAtUtc = DateTime.UtcNow;
        product.ImageUrl = request.ImageUrl?.Trim();

        await _db.SaveChangesAsync(ct);

        return Ok(ToResponse(product));
    }


    // DELETE PRODUCT
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
        {
            return NotFound(new MessageResponse
            {
                Message = "Product not found."
            });
        }

        string? imageUrl = product.ImageUrl;

        _db.Products.Remove(product);

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }


    // ENTITY TO RESPONSE DTO
    private static ProductResponse ToResponse(Product p)
    {
        return new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            IsActive = p.IsActive,

            // CATEGORY
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? "",

            // BRAND
            BrandId = p.BrandId,
            BrandName = p.Brand?.Name ?? "",

            CreatedAtUtc = p.CreatedAtUtc,
            UpdatedAtUtc = p.UpdatedAtUtc
        };
    }
}
