using ClubNet.API.Data;
using ClubNet.API.Services;
using ClubNet.Shared.Constants;
using ClubNet.Shared.DTOs;
using ClubNet.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventosController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public EventosController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<EventoDto>>> GetEventos([FromQuery] int? idClube)
    {
        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);

        IQueryable<Evento> query = _db.Eventos.Include(e => e.Clube).Where(e => e.Ativo);

        if (effectiveClubeId.HasValue)
        {
            query = query.Where(e => e.IdClube == effectiveClubeId.Value);
        }

        // Se o usuário não estiver autenticado, só vê eventos com PUBLICO_GERAL
        if (!_currentUser.IsAuthenticated)
        {
            query = query.Where(e => e.PublicoAlvo == PublicoAlvoEvento.PublicoGeral);
        }

        var eventos = await query
            .OrderBy(e => e.DataInicio)
            .Select(e => new EventoDto
            {
                Id = e.Id,
                IdClube = e.IdClube,
                NomeClube = e.Clube != null ? e.Clube.Nome : null,
                Titulo = e.Titulo,
                Descricao = e.Descricao,
                DataInicio = e.DataInicio,
                DataFim = e.DataFim,
                PublicoAlvo = e.PublicoAlvo,
                Local = e.Local,
                Ativo = e.Ativo
            })
            .ToListAsync();

        return Ok(eventos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventoDto>> GetEvento(int id)
    {
        var e = await _db.Eventos.Include(x => x.Clube).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();

        if (!_currentUser.IsAuthenticated && e.PublicoAlvo != PublicoAlvoEvento.PublicoGeral)
        {
            return Forbid();
        }

        return Ok(new EventoDto
        {
            Id = e.Id,
            IdClube = e.IdClube,
            NomeClube = e.Clube?.Nome,
            Titulo = e.Titulo,
            Descricao = e.Descricao,
            DataInicio = e.DataInicio,
            DataFim = e.DataFim,
            PublicoAlvo = e.PublicoAlvo,
            Local = e.Local,
            Ativo = e.Ativo
        });
    }

    [HttpPost]
    public async Task<ActionResult<EventoDto>> CreateEvento([FromBody] CreateEventoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var targetClubeId = _currentUser.GetEffectiveClubeId(request.IdClube);
        if (!targetClubeId.HasValue || !_currentUser.CanManageClub(targetClubeId.Value))
        {
            return Forbid();
        }

        var evento = new Evento
        {
            IdClube = targetClubeId.Value,
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao?.Trim(),
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            PublicoAlvo = string.IsNullOrWhiteSpace(request.PublicoAlvo) ? PublicoAlvoEvento.Associados : request.PublicoAlvo,
            Local = request.Local?.Trim(),
            Ativo = true
        };

        _db.Eventos.Add(evento);
        await _db.SaveChangesAsync();

        var clube = await _db.Clubes.FindAsync(evento.IdClube);

        return CreatedAtAction(nameof(GetEvento), new { id = evento.Id }, new EventoDto
        {
            Id = evento.Id,
            IdClube = evento.IdClube,
            NomeClube = clube?.Nome,
            Titulo = evento.Titulo,
            Descricao = evento.Descricao,
            DataInicio = evento.DataInicio,
            DataFim = evento.DataFim,
            PublicoAlvo = evento.PublicoAlvo,
            Local = evento.Local,
            Ativo = evento.Ativo
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvento(int id, [FromBody] CreateEventoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var evento = await _db.Eventos.FindAsync(id);
        if (evento == null) return NotFound();

        if (!_currentUser.CanManageClub(evento.IdClube)) return Forbid();

        evento.Titulo = request.Titulo.Trim();
        evento.Descricao = request.Descricao?.Trim();
        evento.DataInicio = request.DataInicio;
        evento.DataFim = request.DataFim;
        if (!string.IsNullOrWhiteSpace(request.PublicoAlvo)) evento.PublicoAlvo = request.PublicoAlvo;
        evento.Local = request.Local?.Trim();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvento(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var evento = await _db.Eventos.FindAsync(id);
        if (evento == null) return NotFound();

        if (!_currentUser.CanManageClub(evento.IdClube)) return Forbid();

        evento.Ativo = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
