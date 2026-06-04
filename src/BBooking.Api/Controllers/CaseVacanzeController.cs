using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BBooking.Data;
using BBooking.Models.Entities;
using Asp.Versioning;

namespace BBooking.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/case")]
public class CaseVacanzeController : ControllerBase
{
    private readonly AppDbContext _db;

    public CaseVacanzeController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var caseVacanze = await _db.CaseVacanze
            .Include(c => c.Localita)
            .ToListAsync();
            
        return Ok(caseVacanze);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var casa = await _db.CaseVacanze
            .Include(c => c.Localita)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (casa == null) return NotFound("Casa vacanze non trovata.");
        return Ok(casa);
    }

    [HttpPost]
    [Authorize(Policy = "RequireHostRole")]
    public async Task<IActionResult> Create([FromBody] CreateCasaVacanzeRequest request)
    {
        var hostIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(hostIdString, out int hostId)) return Unauthorized();

        var casa = new CasaVacanze
        {
            Nome = request.Nome,
            Descrizione = request.Descrizione,
            PrezzoPerNotte = request.PrezzoPerNotte,
            LocalitaId = request.LocalitaId,
            HostId = hostId
        };

        _db.CaseVacanze.Add(casa);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = casa.Id }, casa);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireHostRole")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCasaVacanzeRequest request)
    {
        var hostIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(hostIdString, out int hostId)) return Unauthorized();

        var casa = await _db.CaseVacanze.FindAsync(id);
        if (casa == null) return NotFound("Casa vacanze non trovata.");

        // Sicurezza: L'Host può modificare solo le TUE case
        if (casa.HostId != hostId) return Forbid();

        casa.Nome = request.Nome;
        casa.Descrizione = request.Descrizione;
        casa.PrezzoPerNotte = request.PrezzoPerNotte;
        casa.LocalitaId = request.LocalitaId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireHostRole")]
    public async Task<IActionResult> Delete(int id)
    {
        var hostIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(hostIdString, out int hostId)) return Unauthorized();

        var casa = await _db.CaseVacanze.FindAsync(id);
        if (casa == null) return NotFound("Casa vacanze non trovata.");
        if (casa.HostId != hostId) return Forbid(); // Check proprietà

        _db.CaseVacanze.Remove(casa);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateCasaVacanzeRequest(string Nome, string Descrizione, decimal PrezzoPerNotte, int LocalitaId);
public record UpdateCasaVacanzeRequest(string Nome, string Descrizione, decimal PrezzoPerNotte, int LocalitaId);