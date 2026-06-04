using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BBooking.Data;
using BBooking.Models.Entities;
using Asp.Versioning;

namespace BBooking.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Utenti.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("Un utente con questa email esiste già.");
        }

        var user = new Utente
        {
            Nome = request.Nome,
            Email = request.Email,
            Password = request.Password, // Da implementare: hashing della password in un progetto reale
            Ruolo = request.Ruolo
        };

        _db.Utenti.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { Messaggio = "Registrazione completata con successo." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Utenti.SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || user.Password != request.Password)
        {
            return Unauthorized("Credenziali non valide.");
        }

        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Ruolo.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token), Ruolo = user.Ruolo.ToString() });
    }
}

public record RegisterRequest(string Nome, string Email, string Password, RuoloUtente Ruolo);
public record LoginRequest(string Email, string Password);
