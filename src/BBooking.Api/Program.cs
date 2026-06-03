using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BBooking.Data;
using BBooking.Models.Entities;
using NSwag;
using NSwag.Generation.Processors.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// --- Configurazione Swagger (NSwag) per supportare JWT ---
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "BBooking API";
    config.AddSecurity("Bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Inserisci il token JWT in questo formato: Bearer {il_tuo_token}"
    });
    
    config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Configurazione JWT ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// --- Configurazione Autorizzazione e Policy (RBAC) ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireHostRole", policy => policy.RequireRole(RuoloUtente.Host.ToString(), RuoloUtente.Admin.ToString()));
    options.AddPolicy("RequireGuestRole", policy => policy.RequireRole(RuoloUtente.Guest.ToString(), RuoloUtente.Admin.ToString()));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

// Aggiungiamo i middleware nella pipeline HTTP in quest'ordine (prima delle rotte)
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// --- Endpoint Minimal API di Login ---
app.MapPost("/api/v1/auth/login", async (LoginRequest request, AppDbContext db, IConfiguration config) =>
{
    var user = await db.Utenti.SingleOrDefaultAsync(u => u.Email == request.Email);

    // Controllo fittizio sulla password
    if (user == null || user.Password != request.Password)
    {
        return Results.Unauthorized();
    }

    var jwtSection = config.GetSection("Jwt");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var ruoloString = user.Ruolo.ToString();

    // Creazione dei Claims
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, ruoloString)
    };

    // Generazione del Token
    var token = new JwtSecurityToken(
        issuer: jwtSection["Issuer"],
        audience: jwtSection["Audience"],
        claims: claims,
        expires: DateTime.Now.AddHours(2), // Validità di 2 ore
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new LoginResponse(tokenString, ruoloString));
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// DTOs per Auth
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Ruolo);
