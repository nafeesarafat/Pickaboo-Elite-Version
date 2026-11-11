using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewProject.Api.Data;
using NewProject.Api.DTOs;
using NewProject.Api.Models;

namespace NewProject.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Customer")]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;

    public CartController(AppDbContext db)
    {
        _db = db;
    }

    private int CustomerId =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);

    private async Task<Cart> GetOrCreateCart(
        CancellationToken ct)
    {
        var cart = await _db.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(
                x => x.UserId == CustomerId, ct);

        if (cart != null)
            return cart;

        cart = new Cart
        {
            UserId = CustomerId
        };

        _db.Carts.Add(cart);

        await _db.SaveChangesAsync(ct);

        return cart;
    }

    // GET CUSTOMER CART
    [HttpGet]
    public async Task<IActionResult> GetCart(
        CancellationToken ct)
    {
        var cart = await GetOrCreateCart(ct);

        return Ok(ToDto(cart));
    }

    // ADD PRODUCT
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        AddCartItemRequest request,
        CancellationToken ct)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(
                x => x.Id == request.ProductId &&
                     x.IsActive, ct);

        if (product == null)
            return NotFound(new { message = "Product not found." });

        if (product.StockQuantity < request.Quantity)
            return BadRequest(new { message = "Insufficient stock." });

        var cart = await GetOrCreateCart(ct);

        var item = cart.Items.FirstOrDefault(
            x => x.ProductId == product.Id);

        if (item == null)
        {
            item = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity
            };

            cart.Items.Add(item);
        }
        else
        {
            if (item.Quantity + request.Quantity >
                product.StockQuantity)
            {
                return BadRequest(new
                {
                    message = "Requested quantity exceeds stock."
                });
            }

            item.Quantity += request.Quantity;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "Product added to cart.",
            cart = ToDto(cart)
        });
    }

    // UPDATE QUANTITY
    [HttpPut("items/{id:int}")]
    public async Task<IActionResult> UpdateItem(
        int id,
        UpdateCartItemRequest request,
        CancellationToken ct)
    {
        var cart = await GetOrCreateCart(ct);

        var item = cart.Items.FirstOrDefault(
            x => x.Id == id);

        if (item == null)
            return NotFound(new { message = "Cart item not found." });

        if (request.Quantity > item.Product.StockQuantity)
        {
            return BadRequest(new
            {
                message = "Requested quantity exceeds available stock."
            });
        }

        item.Quantity = request.Quantity;
        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Ok(ToDto(cart));
    }

    // REMOVE ITEM
    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> RemoveItem(
        int id,
        CancellationToken ct)
    {
        var cart = await GetOrCreateCart(ct);

        var item = cart.Items.FirstOrDefault(
            x => x.Id == id);

        if (item == null)
            return NotFound();

        _db.CartItems.Remove(item);

        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Ok(ToDto(cart));
    }

    // CHECKOUT
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(
        CheckoutRequest request,
        CancellationToken ct)
    {
        var cart = await GetOrCreateCart(ct);

        if (!cart.Items.Any())
            return BadRequest(new { message = "Your cart is empty." });

        decimal total = 0;

        var order = new Order
        {
            OrderNumber = $"PKB-{Guid.NewGuid():N}"
                .Substring(0, 15).ToUpperInvariant(),

            UserId = CustomerId,
            ShippingAddress = request.ShippingAddress.Trim(),
            PaymentMethod = "CashOnDelivery",
            Status = "Pending"
        };

        foreach (var cartItem in cart.Items)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(
                    x => x.Id == cartItem.ProductId &&
                         x.IsActive, ct);

            if (product == null ||
                product.StockQuantity < cartItem.Quantity)
            {
                return BadRequest(new
                {
                    message = $"{cartItem.Product.Name} is no longer available in the requested quantity."
                });
            }

            decimal subtotal =
                product.Price * cartItem.Quantity;

            total += subtotal;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity,
                Subtotal = subtotal
            });

            product.StockQuantity -= cartItem.Quantity;
        }

        order.TotalAmount = total;

        _db.Orders.Add(order);

        _db.CartItems.RemoveRange(cart.Items);

        // EF Core SaveChanges executes these tracked changes
        // in one database transaction.
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "Order placed successfully.",
            orderId = order.Id,
            orderNumber = order.OrderNumber,
            total = order.TotalAmount
        });
    }

    private static CartDto ToDto(Cart cart)
    {
        return new CartDto
        {
            Items = cart.Items.Select(x => new CartItemDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                ImageUrl = x.Product.ImageUrl,
                UnitPrice = x.Product.Price,
                Quantity = x.Quantity,
                Subtotal = x.Product.Price * x.Quantity
            }).ToList(),

            Total = cart.Items.Sum(
                x => x.Product.Price * x.Quantity)
        };
    }
}