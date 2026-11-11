namespace NewProject.Api.Models;

public class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = "";

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Pending";

    public string PaymentMethod { get; set; } = "CashOnDelivery";

    public string ShippingAddress { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; }
        = new List<OrderItem>();
}