namespace NewProject.Api.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public int BrandId { get; set; }

    public Brand Brand { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}