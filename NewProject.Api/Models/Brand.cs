namespace NewProject.Api.Models;

public class Brand
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; }
        = new List<Product>();
}