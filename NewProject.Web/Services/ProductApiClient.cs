using System.Net.Http.Headers;
using System.Net.Http.Json;
using NewProject.UI.Models;

namespace NewProject.UI.Services;

public class ProductApiClient
{
    private readonly HttpClient _http;

    public ProductApiClient(HttpClient http)
    {
        _http = http;
    }

    public string ImageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "";

        if (path.StartsWith("http"))
            return path;

        return _http.BaseAddress + path.TrimStart('/');
    }


    // get active products
    public async Task<List<ProductDto>> GetPublicAsync(
        CancellationToken ct = default)
    {
        using var response = await _http.GetAsync(
            "api/products", ct);

        await EnsureSuccess(response, ct);

        return await response.Content
            .ReadFromJsonAsync<List<ProductDto>>(
                cancellationToken: ct) ?? [];
    }


    // get all products (admin)
    public async Task<List<ProductDto>> GetAllAsync(
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(  // jwt token handler
            HttpMethod.Get,
            "api/admin/products",
            token);

        using var response = await _http.SendAsync(request, ct);

        await EnsureSuccess(response, ct);

        return await response.Content
            .ReadFromJsonAsync<List<ProductDto>>(
                cancellationToken: ct) ?? [];
    }

    // GET BY ID
    public async Task<ProductDto?> GetByIdAsync(
        int id,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Get,
            $"api/admin/products/{id}",
            token);

        using var response = await _http.SendAsync(request, ct);

        await EnsureSuccess(response, ct);

        return await response.Content
            .ReadFromJsonAsync<ProductDto>(
                cancellationToken: ct);
    }

    // CREATE
    public async Task<ProductDto> CreateAsync(
        ProductFormInput input,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Post,
            "api/admin/products",
            token);

        request.Content = JsonContent.Create(input);

        using var response = await _http.SendAsync(request, ct);

        await EnsureSuccess(response, ct);

        return (await response.Content
            .ReadFromJsonAsync<ProductDto>(
                cancellationToken: ct))!;
    }

    // UPDATE
    public async Task<ProductDto> UpdateAsync(
        int id,
        ProductFormInput input,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Put,
            $"api/admin/products/{id}",
            token);

        request.Content = JsonContent.Create(input);

        using var response = await _http.SendAsync(request, ct);

        await EnsureSuccess(response, ct);

        return (await response.Content
            .ReadFromJsonAsync<ProductDto>(
                cancellationToken: ct))!;
    }

    // DELETE
    public async Task DeleteAsync(
        int id,
        string token,
        CancellationToken ct = default)
    {
        using var request = Create(
            HttpMethod.Delete,
            $"api/admin/products/{id}",
            token);

        using var response = await _http.SendAsync(request, ct);

        await EnsureSuccess(response, ct);
    }


    // add jwt to request
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

    // HANDLE API ERRORS
    private static async Task EnsureSuccess(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        string message =
            $"API request failed ({(int)response.StatusCode}).";

        try
        {
            var body = await response.Content
                .ReadFromJsonAsync<ApiError>(
                    cancellationToken: ct);

            message = body?.Message
                ?? body?.Title
                ?? message;
        }
        catch
        {
            // default error msg show korbe
        }

        throw new HttpRequestException(
            message,
            null,
            response.StatusCode);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }

        public string? Title { get; set; }
    }
}
