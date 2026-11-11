using System.ComponentModel.DataAnnotations;

namespace NewProject.UI.Models;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class BrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? LogoUrl { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = [];
    public decimal Total { get; set; }
}

public class AddCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

public class CheckoutInput
{
    [Required]
    [StringLength(500)]
    public string ShippingAddress { get; set; } = "";
}

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

public class CustomerProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileInput
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = "";
}