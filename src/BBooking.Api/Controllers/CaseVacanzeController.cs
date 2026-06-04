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
            .Select(c => new CasaVacanzeResponse(
                c.Id,
                c.Nome,
                c.Descrizione,
                c.PrezzoPerNotte,
                c.HostId,
                c.Localita.Nome,
                c.Localita.Regione
            ))
            .ToListAsync();
            
        if (caseVacanze == null || caseVacanze.Count == 0) return NotFound("Nessuna casa vacanze trovata.");
        return Ok(caseVacanze);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var casa = await _db.CaseVacanze
            .Where(c => c.Id == id)
            .Select(c => new CasaVacanzeResponse(
                c.Id,
                c.Nome,
                c.Descrizione,
                c.PrezzoPerNotte,
                c.HostId,
                c.Localita.Nome,
                c.Localita.Regione
            ))
            .FirstOrDefaultAsync();

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
        if (casa.HostId != hostId) return Unauthorized();

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
        if (casa.HostId != hostId) return Unauthorized(); // Check proprietà

        _db.CaseVacanze.Remove(casa);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/servizi")]
    [Authorize(Policy = "RequireHostRole")]
    public async Task<IActionResult> UpdateServizi(int id, [FromBody] List<int> serviziIds)
    {
        var hostIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(hostIdString, out int hostId)) return Unauthorized();

        var casa = await _db.CaseVacanze
            .Include(c => c.Servizi)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (casa == null) return NotFound("Casa vacanze non trovata.");

        // L'Host può modificare solo le sue case, mentre l'Admin può modificare tutto
        if (casa.HostId != hostId && !User.IsInRole("Admin")) return Unauthorized();

        var nuoviServizi = await _db.Servizi
            .Where(s => serviziIds.Contains(s.Id))
            .ToListAsync();

        casa.Servizi.Clear();
        foreach (var servizio in nuoviServizi)
        {
            casa.Servizi.Add(servizio);
        }

        await _db.SaveChangesAsync();
        return Ok(new { Messaggio = "Servizi aggiornati con successo." });
    }
}

public record CreateCasaVacanzeRequest(string Nome, string? Descrizione, decimal PrezzoPerNotte, int LocalitaId);
public record UpdateCasaVacanzeRequest(string Nome, string? Descrizione, decimal PrezzoPerNotte, int LocalitaId);
public record CasaVacanzeResponse(int Id, string Nome, string? Descrizione, decimal PrezzoPerNotte, int HostId, string NomeLocalita, string RegioneLocalita);