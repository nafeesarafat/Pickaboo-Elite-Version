namespace NewProject.Api.Models;

public class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public ICollection<CartItem> Items { get; set; }
        = new List<CartItem>();
}