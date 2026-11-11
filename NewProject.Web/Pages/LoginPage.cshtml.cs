
using Microsoft.AspNetCore.Mvc;
using NewProject.UI.Services;

namespace NewProject.UI.Pages;

public class LoginPageModel : AuthPageModel
{
    public LoginPageModel(AuthApiClient api)
        : base(api)
    {
    }

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()  
    {
        try
        {
            var result = await Api.PostAsync(  // send req frm frontend to backend
                "api/login/login",
                new { Email, Password });

            Message = result.Message;

            if (!result.Success ||
                string.IsNullOrWhiteSpace(result.Token))
            {
                return Page();
            }

            // Check role
            string role = result.Role ?? "";

            bool isAdmin = string.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase);

            bool isCustomer = string.Equals(
                role,
                "Customer",
                StringComparison.OrdinalIgnoreCase);


            // 
            if (!isAdmin && !isCustomer)
            {
                Message = "Invalid account role.";
                return Page();
            }

            // Save login session
            HttpContext.Session.SetString(
                "AuthToken", result.Token);

            HttpContext.Session.SetString(
                "UserName",
                result.Name ?? (isAdmin ? "Administrator" : "Customer"));

            HttpContext.Session.SetString(
                "UserRole", role);

            // Admin goes to Admin Dashboard
            if (isAdmin)
            {
                return RedirectToPage("/Dashboard");
            }

            // Customer goes to Customer Homepage
            return RedirectToPage("/Index");
        }
        catch (HttpRequestException)
        {
            Message =
                "Cannot connect to the API. Check that the backend is running.";

            return Page();
        }
    }
}
