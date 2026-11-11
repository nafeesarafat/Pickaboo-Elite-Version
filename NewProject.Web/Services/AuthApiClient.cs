using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NewProject.UI.Services;

public class AuthApiClient
{
	private readonly HttpClient _http; 

	public AuthApiClient(HttpClient http)
	{
		_http = http; // HttpClient is the carrier of backend request
	} 

	public async Task<(
		bool Success, 
		string Message, 
		string? Token, 
		string? Name, 
		string? Role)> PostAsync(string endpoint, object data, string? token = null) //post request to the api server

    {
		using var request = new HttpRequestMessage(  // request is created to send to the api controller
			HttpMethod.Post, endpoint);  


		request.Content = JsonContent.Create(data);  // form data to json format 

		if (!string.IsNullOrWhiteSpace(token))
		{
			request.Headers.Authorization =
				new AuthenticationHeaderValue("Bearer", token);
		}

		using var response = await _http.SendAsync(request); // http post request is sent to the api controller

		var body = await response.Content                    
			.ReadFromJsonAsync<ApiResult>();          

		return (
			response.IsSuccessStatusCode,

			body?.Message ?? (response.IsSuccessStatusCode
				? "Successful"
				: "Request failed."),

			body?.Token,
			body?.Name,
			body?.Role
		);
	}

	private sealed class ApiResult
	{
		public string? Message { get; set; }
		public string? Token { get; set; }

		public string? Name { get; set; }

		public string? Role { get; set; }
    }
}