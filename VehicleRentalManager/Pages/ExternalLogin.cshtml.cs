using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ExternalLoginModel : PageModel
{
    public IActionResult OnGet(string returnUrl = "/")
    {
        var redirectUrl = Url.Page("/ExternalLoginCallback");
        var properties  = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
}