using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRentalManager.Services;

public class ExternalLoginCallbackModel : PageModel
{
    private readonly IJwtService  _jwtService;
    private readonly UserService  _userService;

    public ExternalLoginCallbackModel(IJwtService jwtService, UserService userService)
    {
        _jwtService  = jwtService;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Get the info Google sent back
        var result = await HttpContext.AuthenticateAsync(
            Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            return Redirect("/auth?error=login_failed");

        var email    = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var name     = result.Principal?.FindFirstValue(ClaimTypes.Name);
        var googleId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (email == null || googleId == null)
            return Redirect("/auth?error=no_email");

        name ??= email;

        // Check MongoDB for existing user
        var user = await _userService.FindByEmailAsync(email);
        bool isNewUser = false;

        if (user == null)
        {
            // New user — create in MongoDB, awaiting approval
            user = await _userService.CreateAsync(name, email, googleId);
            isNewUser = true;
        }

        // Not approved yet
        if (!user.IsApproved)
        {
            var status = isNewUser ? "pending_new" : "pending";
            return Redirect($"/auth?status={status}");
        }

        // Approved — issue JWT and let them in
        await _userService.UpdateLastLoginAsync(user.Id!);

        var token = _jwtService.GenerateToken(user.Id!, user.Email, user.Name);

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddHours(8)
        });

        return Redirect("/");
    }
}