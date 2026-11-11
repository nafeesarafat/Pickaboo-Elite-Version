using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewProject.Api.Data;
using NewProject.Api.DTOs;

namespace NewProject.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "Customer")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    private int CustomerId =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);

    // CUSTOMER ORDER HISTORY
    [HttpGet]
    public async Task<IActionResult> GetMyOrders(
        CancellationToken ct)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Where(x => x.UserId == CustomerId)
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(orders.Select(ToDto));
    }

    // SINGLE ORDER DETAILS
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(
        int id,
        CancellationToken ct)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == CustomerId, ct);

        if (order == null)
            return NotFound(new
            {
                message = "Order not found."
            });

        return Ok(ToDto(order));
    }

    private static OrderDto ToDto(
        NewProject.Api.Models.Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            ShippingAddress = order.ShippingAddress,
            CreatedAtUtc = order.CreatedAtUtc,

            Items = order.Items.Select(x => new OrderItemDto
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity,
                Subtotal = x.Subtotal
            }).ToList()
        };
    }
}