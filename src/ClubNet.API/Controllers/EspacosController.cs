using ClubNet.API.Data;
using ClubNet.API.Services;
using ClubNet.Shared.DTOs;
using ClubNet.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EspacosController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public EspacosController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<EspacoDto>>> GetEspacos([FromQuery] int? idClube)
    {
        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);

        IQueryable<Espaco> query = _db.Espacos.Include(e => e.Clube).Where(e => e.Ativo);

        if (effectiveClubeId.HasValue)
        {
            query = query.Where(e => e.IdClube == effectiveClubeId.Value);
        }

        var espacos = await query
            .OrderBy(e => e.Nome)
            .Select(e => new EspacoDto
            {
                Id = e.Id,
                IdClube = e.IdClube,
                NomeClube = e.Clube != null ? e.Clube.Nome : null,
                Nome = e.Nome,
                Descricao = e.Descricao,
                Capacidade = e.Capacidade,
                Ativo = e.Ativo
            })
            .ToListAsync();

        return Ok(espacos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EspacoDto>> GetEspaco(int id)
    {
        var e = await _db.Espacos.Include(x => x.Clube).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();

        return Ok(new EspacoDto
        {
            Id = e.Id,
            IdClube = e.IdClube,
            NomeClube = e.Clube?.Nome,
            Nome = e.Nome,
            Descricao = e.Descricao,
            Capacidade = e.Capacidade,
            Ativo = e.Ativo
        });
    }

    [HttpPost]
    public async Task<ActionResult<EspacoDto>> CreateEspaco([FromBody] CreateEspacoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var targetClubeId = _currentUser.GetEffectiveClubeId(request.IdClube);
        if (!targetClubeId.HasValue || !_currentUser.CanManageClub(targetClubeId.Value))
        {
            return Forbid();
        }

        var espaco = new Espaco
        {
            IdClube = targetClubeId.Value,
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim(),
            Capacidade = request.Capacidade,
            Ativo = true
        };

        _db.Espacos.Add(espaco);
        await _db.SaveChangesAsync();

        var clube = await _db.Clubes.FindAsync(espaco.IdClube);

        return CreatedAtAction(nameof(GetEspaco), new { id = espaco.Id }, new EspacoDto
        {
            Id = espaco.Id,
            IdClube = espaco.IdClube,
            NomeClube = clube?.Nome,
            Nome = espaco.Nome,
            Descricao = espaco.Descricao,
            Capacidade = espaco.Capacidade,
            Ativo = espaco.Ativo
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEspaco(int id, [FromBody] CreateEspacoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var espaco = await _db.Espacos.FindAsync(id);
        if (espaco == null) return NotFound();

        if (!_currentUser.CanManageClub(espaco.IdClube)) return Forbid();

        espaco.Nome = request.Nome.Trim();
        espaco.Descricao = request.Descricao?.Trim();
        espaco.Capacidade = request.Capacidade;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEspaco(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var espaco = await _db.Espacos.FindAsync(id);
        if (espaco == null) return NotFound();

        if (!_currentUser.CanManageClub(espaco.IdClube)) return Forbid();

        espaco.Ativo = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
