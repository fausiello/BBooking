using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BBooking.Data;
using BBooking.Models.Entities;
using Asp.Versioning;

namespace BBooking.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/servizi")]
public class ServiziController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServiziController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var servizi = await _db.Servizi.ToListAsync();
        return Ok(servizi);
    }

    [HttpPost]
    [Authorize(Policy = "RequireHostRole")] // Consente l'accesso ad Admin e Host
    public async Task<IActionResult> Create([FromBody] CreateServizioRequest request)
    {
        var servizio = new Servizio
        {
            Nome = request.Nome
        };

        _db.Servizi.Add(servizio);
        await _db.SaveChangesAsync();

        return Ok(servizio);
    }
}

public record CreateServizioRequest(string Nome);