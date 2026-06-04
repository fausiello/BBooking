using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using BBooking.Data;
using BBooking.Models.Entities;
using NSwag;
using NSwag.Generation.Processors.Security;
using BBooking.Api.Controllers;
using Asp.Versioning;
using BBooking.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();

// --- Configurazione Client gRPC ---
builder.Services.AddGrpcClient<PagamentoService.PagamentoServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:7001");
});

// --- Configurazione API Versioning ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Versione di default (1.0)
    options.AssumeDefaultVersionWhenUnspecified = true; // Se l'URL non ha la v, usa la default
    options.ReportApiVersions = true; // Aggiunge gli header nella risposta (es. api-supported-versions: 1.0)
}).AddMvc();

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

builder.Services.AddCors(options =>
    options.AddPolicy("AllowBlazor", policy =>
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000", "https://localhost:7153", "http://localhost:5038")
              .AllowAnyMethod()
              .AllowAnyHeader())
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

// Aggiungiamo i middleware nella pipeline HTTP in quest'ordine (prima delle rotte)
app.UseCors("AllowBlazor");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
