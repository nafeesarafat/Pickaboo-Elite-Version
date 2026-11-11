
using System.ComponentModel.DataAnnotations;

namespace NewProject.UI.Models;

public class ProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public int BrandId { get; set; }

    public string? BrandName { get; set; }

    public bool IsActive { get; set; }
}

public class ProductFormInput
{
    [Required]
    [StringLength(160)]
    public string Name { get; set; } = "";

    [StringLength(2000)]
    public string Description { get; set; } = "";

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int BrandId { get; set; }
    public string BrandName { get; set; } = "";
}
