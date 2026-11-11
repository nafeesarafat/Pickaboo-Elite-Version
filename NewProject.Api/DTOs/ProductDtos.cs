
using System.ComponentModel.DataAnnotations;

namespace NewProject.Api.DTOs;

public class ProductInsertRequest
{
    [Required, StringLength(20)]
    public string Name { get; set; } = "";

    [StringLength(200)]
    public string Description { get; set; } = "";

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int BrandId { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ProductResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";

    public int BrandId { get; set; }
    public string BrandName { get; set; } = "";
}

public class ProductImageResponse
{
    public string ImageUrl { get; set; } = "";
}
