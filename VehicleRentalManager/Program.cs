using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VehicleRentalManager.Services;
using dotenv.net;

// Carrega variáveis do arquivo .env ANTES de criar o builder
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Recarrega a configuração para incluir as variáveis do .env
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Configure HttpClient for API calls
builder.Services.AddHttpClient();

// Register JWT token service (backend only, cliente-side will handle auth separately)
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Add authorization services
builder.Services.AddAuthorizationCore();

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

// TODO: When database is ready, register your repositories here
// builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Apply CORS policy
app.UseCors("AllowBlazorClient");

app.MapControllers();

app.Run();