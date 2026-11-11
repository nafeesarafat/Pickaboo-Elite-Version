using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class AuthPageModel : PageModel
{
    protected readonly AuthApiClient Api;

    public AuthPageModel(AuthApiClient api)
    {
        Api = api;
    }

    public string? Message { get; protected set; }


    // reusable code for api call 
    protected async Task<bool> CallApiAsync(
        string endpoint, object data, bool authenticated = false)
    {
        var token = authenticated
            ? HttpContext.Session.GetString("AuthToken")
            : null;

        if (authenticated && string.IsNullOrEmpty(token))
        {
            Message = "Please log in first.";
            return false;
        }

        try
        {   /// api is called here and the result is returned
            var result = await Api.PostAsync(endpoint, data, token);
            Message = result.Message;

            return result.Success;
        }
        catch (HttpRequestException)
        {
            Message = "Cannot connect to the API. Check that the backend is running.";
            return false;
        }
    }
}