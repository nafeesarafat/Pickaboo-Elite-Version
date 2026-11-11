using System.Net.Http.Headers;
using System.Net.Http.Json;
using NewProject.UI.Models;

namespace NewProject.UI.Services;

public class ShoppingApiClient
{
    private readonly HttpClient _http;

    public ShoppingApiClient(HttpClient http)
    {
        _http = http;
    }

    // 
    public string ImageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "";

        if (Uri.TryCreate(path, UriKind.Absolute, out var absolute))
            return absolute.ToString();

        return new Uri(
            _http.BaseAddress!,
            path.TrimStart('/')).ToString();
    }

    // PUBLIC CATEGORIES
    public async Task<List<CategoryDto>> GetCategoriesAsync(
        CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<CategoryDto>>(
            "api/categories", ct) ?? [];
    }

    // PUBLIC BRANDS
    public async Task<List<BrandDto>> GetBrandsAsync(
        CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<BrandDto>>(
            "api/brands", ct) ?? [];
    }

    // GET FILTERED PRODUCTS
    public async Task<List<ProductDto>> GetProductsAsync(
        int? categoryId = null,
        int? brandId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var query = new List<string>();

        if (categoryId.HasValue)
            query.Add($"categoryId={categoryId.Value}");

        if (brandId.HasValue)
            query.Add($"brandId={brandId.Value}");

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(search)}");
        }

        string url = "api/products";

        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        return await _http.GetFromJsonAsync<List<ProductDto>>(
            url, ct) ?? [];
    }

    // GET CART
    public async Task<CartDto> GetCartAsync(
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Get, "api/cart", token);

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);

        return (await response.Content
            .ReadFromJsonAsync<CartDto>(
                cancellationToken: ct)) ?? new CartDto();
    }

    // ADD TO CART
    public async Task AddToCartAsync(
        AddCartItemRequest input,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Post, "api/cart/items", token);

        request.Content = JsonContent.Create(input);

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);
    }

    // UPDATE CART ITEM
    public async Task UpdateCartItemAsync(
        int id,
        int quantity,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Put,
            $"api/cart/items/{id}",
            token);

        request.Content = JsonContent.Create(
            new UpdateCartItemRequest
            {
                Quantity = quantity
            });

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);
    }

    // REMOVE CART ITEM
    public async Task RemoveCartItemAsync(
        int id,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Delete,
            $"api/cart/items/{id}",
            token);

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);
    }

    // CHECKOUT
    public async Task<CheckoutResult> CheckoutAsync(
        CheckoutInput input,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Post, "api/cart/checkout", token);

        request.Content = JsonContent.Create(input);

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);

        return (await response.Content
            .ReadFromJsonAsync<CheckoutResult>(
                cancellationToken: ct))!;
    }

    // GET ORDERS
    public async Task<List<OrderDto>> GetOrdersAsync(
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Get, "api/orders", token);

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);

        return await response.Content
            .ReadFromJsonAsync<List<OrderDto>>(
                cancellationToken: ct) ?? [];
    }

    // GET SINGLE ORDER
    public async Task<OrderDto?> GetOrderAsync(
        int id,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Get, $"api/orders/{id}", token);

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);

        return await response.Content
            .ReadFromJsonAsync<OrderDto>(
                cancellationToken: ct);
    }

    // GET PROFILE
    public async Task<CustomerProfileDto> GetProfileAsync(
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Get, "api/account/me", token);

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);

        return (await response.Content
            .ReadFromJsonAsync<CustomerProfileDto>(
                cancellationToken: ct))!;
    }

    // UPDATE PROFILE
    public async Task UpdateProfileAsync(
        UpdateProfileInput input,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Put, "api/account/me", token);

        request.Content = JsonContent.Create(input);

        using var response = await _http.SendAsync(
            request, ct);

        await EnsureSuccess(response, ct);
    }

    // JWT REQUEST HELPER
    private static HttpRequestMessage Create(
        HttpMethod method,
        string url,
        string token)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    // ERROR HANDLING
    private static async Task EnsureSuccess(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        string message =
            $"Request failed ({(int)response.StatusCode}).";

        try
        {
            var error = await response.Content
                .ReadFromJsonAsync<ApiError>(
                    cancellationToken: ct);

            message = error?.Message
                ?? error?.Title
                ?? message;
        }
        catch
        {
            // Use fallback message.
        }

        throw new HttpRequestException(
            message, null, response.StatusCode);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
        public string? Title { get; set; }
    }
}

public class CheckoutResult
{
    public string Message { get; set; } = "";
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal Total { get; set; }
}