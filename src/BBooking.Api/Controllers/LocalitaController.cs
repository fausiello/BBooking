using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BBooking.Data;
using BBooking.Models.Entities;
using Asp.Versioning;

namespace BBooking.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/localita")]
public class LocalitaController : ControllerBase
{
    private readonly AppDbContext _db;

    public LocalitaController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var localita = await _db.Localita.ToListAsync();
        return Ok(localita);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Consente l'accesso SOLO al ruolo Admin
    public async Task<IActionResult> Create([FromBody] CreateLocalitaRequest request)
    {
        var localita = new Localita
        {
            Nome = request.Nome,
            Regione = request.Regione
        };

        _db.Localita.Add(localita);
        await _db.SaveChangesAsync();

        return Ok(localita);
    }
}

public record CreateLocalitaRequest(string Nome, string Regione);