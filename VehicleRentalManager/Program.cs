using VehicleRentalManager.Components;
using VehicleRentalManager.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;

DotNetEnv.Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// ── Razor Pages (ExternalLogin + ExternalLoginCallback) ───────────────────────
builder.Services.AddRazorPages();

// ── Blazor ────────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddControllers();
// ── MongoDB services ──────────────────────────────────────────────────────────
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<UserService>();

// ── JWT + Auth services ───────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpContextAccessor();

// ── Authentication ────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var secretKey = builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey not configured.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"]   ?? "VehicleRentalManager",
        ValidAudience            = builder.Configuration["Jwt:Audience"] ?? "VehicleRentalManager",
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(secretKey))
    };
})
.AddGoogle(options =>
{
    options.ClientId     = builder.Configuration["GoogleOAuth:ClientId"]
        ?? throw new InvalidOperationException("GoogleOAuth:ClientId not configured.");
    options.ClientSecret = builder.Configuration["GoogleOAuth:ClientSecret"]
        ?? throw new InvalidOperationException("GoogleOAuth:ClientSecret not configured.");
    options.CallbackPath = "/signin-google";
});

builder.Services.AddAuthorization();

// ── Blazor auth state (reads JWT cookie) ──────────────────────────────────────
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// ── HTTP pipeline ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// ── Logout — must be before UseAntiforgery so it isn't blocked ────────────────
app.MapGet("/auth/logout-redirect", (HttpContext ctx) =>
{
    ctx.Response.Cookies.Append("jwt", "", new CookieOptions
    {
        Expires  = DateTimeOffset.UtcNow.AddDays(-1),
        HttpOnly = true,
        Secure   = ctx.Request.IsHttps,
        SameSite = SameSiteMode.Lax
    });
    ctx.Response.Redirect("/auth");
}).AllowAnonymous();

app.UseAntiforgery();

// ── Route mappings ────────────────────────────────────────────────────────────
app.MapRazorPages();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.MapStaticAssets();

app.Run();
