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
[Route("api/v{version:apiVersion}/prenotazioni")]
public class PrenotazioniController : ControllerBase
{
    private readonly AppDbContext _db;

    public PrenotazioniController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("mie")]
    [Authorize(Policy = "RequireGuestRole")]
    public async Task<IActionResult> GetMiePrenotazioni()
    {
        var guestIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(guestIdString, out int guestId)) return Unauthorized();

        var prenotazioni = await _db.Prenotazioni
            .Include(p => p.CasaVacanze)
            .Where(p => p.GuestId == guestId)
            .ToListAsync();

        return Ok(prenotazioni);
    }

    [HttpPost]
    [Authorize(Policy = "RequireGuestRole")]
    public async Task<IActionResult> Create([FromBody] CreatePrenotazioneRequest request)
    {
        var guestIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(guestIdString, out int guestId)) return Unauthorized();

        if (request.DataInizio >= request.DataFine)
            return BadRequest("La data di fine deve essere successiva a quella d'inizio.");

        // --- LOGICA DI OVERBOOKING ---
        bool isOccupata = await _db.Prenotazioni.AnyAsync(p =>
            p.CasaVacanzeId == request.CasaVacanzeId &&
            p.Stato == StatoPrenotazione.Confermata &&
            request.DataInizio < p.DataFine &&
            request.DataFine > p.DataInizio);

        if (isOccupata)
        {
            return BadRequest("La struttura non è disponibile nelle date selezionate.");
        }

        // 2. RECUPERO PREZZO E CALCOLO TOTALE
        var casa = await _db.CaseVacanze.FindAsync(request.CasaVacanzeId);
        if (casa == null) return NotFound("Casa vacanze non trovata.");

        int notti = (int)(request.DataFine - request.DataInizio).TotalDays;
        decimal totale = casa.PrezzoPerNotte * notti;

        // --- SIMULAZIONE PAGAMENTO ---
        bool pagamentoCompletato = SimulaPagamento(totale);

        var prenotazione = new Prenotazione
        {
            DataInizio = request.DataInizio,
            DataFine = request.DataFine,
            Totale = totale,
            GuestId = guestId,
            CasaVacanzeId = request.CasaVacanzeId,
            Stato = pagamentoCompletato ? StatoPrenotazione.Confermata : StatoPrenotazione.InAttesa
        };

        _db.Prenotazioni.Add(prenotazione);
        await _db.SaveChangesAsync();

        if (!pagamentoCompletato) return BadRequest("Transazione di pagamento rifiutata.");

        return Ok(prenotazione);
    }

    // Metodo privato fittizio per simulare il gateway di pagamento
    private bool SimulaPagamento(decimal importo) => importo > 0;
}

public record CreatePrenotazioneRequest(int CasaVacanzeId, DateTime DataInizio, DateTime DataFine);