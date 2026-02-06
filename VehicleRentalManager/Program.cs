using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using VehicleRentalManager.Services;
using dotenv.net;

// Load environment variables from .env file BEFORE creating the builder
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Reload configuration to include environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Register AuthService for dependency injection
builder.Services.AddScoped<AuthService>();

// Add Blazored Local Storage for client-side token management
builder.Services.AddBlazoredLocalStorage();

// Authentication with Cookies + Google OAuth
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Google";
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["GoogleOAuth:ClientId"]!;
        options.ClientSecret = builder.Configuration["GoogleOAuth:ClientSecret"]!;
        options.CallbackPath = "/auth/google-callback"; // Same path as configured in Google redirect URI
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
            ),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

// Configure HttpClient for API calls
builder.Services.AddHttpClient();

// Register JWT token service (backend only, client-side will handle auth separately)
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Add authorization services
builder.Services.AddAuthorization();

// Add CORS to allow requests from Blazor client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register UserRepository for dependency injection
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Apply CORS policy
app.UseCors("AllowBlazorClient");

app.MapControllers();

app.Run();
