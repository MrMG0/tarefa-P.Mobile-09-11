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
public class ClubesController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ClubesController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClubeDto>>> GetClubes([FromQuery] bool? apenasAtivos)
    {
        IQueryable<Clube> query = _db.Clubes;

        // Se não for superadmin e for requisição pública (ex: formulário de cadastro no app/site) ou usuário comum
        if (!_currentUser.IsAuthenticated || !_currentUser.IsSuperAdmin)
        {
            query = query.Where(c => c.Ativo);
        }
        else if (apenasAtivos.HasValue && apenasAtivos.Value)
        {
            query = query.Where(c => c.Ativo);
        }

        var clubes = await query
            .OrderBy(c => c.Nome)
            .Select(c => new ClubeDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Sigla = c.Sigla,
                Endereco = c.Endereco,
                Bairro = c.Bairro,
                Cidade = c.Cidade,
                Uf = c.Uf,
                Telefone = c.Telefone,
                Email = c.Email,
                Ativo = c.Ativo,
                TotalAssociados = _db.Associados.Count(a => a.IdClube == c.Id && a.SituacaoCadastro == SituacaoCadastro.Aprovado)
            })
            .ToListAsync();

        return Ok(clubes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClubeDto>> GetClube(int id)
    {
        var c = await _db.Clubes.FindAsync(id);
        if (c == null) return NotFound();

        return Ok(new ClubeDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Sigla = c.Sigla,
            Endereco = c.Endereco,
            Bairro = c.Bairro,
            Cidade = c.Cidade,
            Uf = c.Uf,
            Telefone = c.Telefone,
            Email = c.Email,
            Ativo = c.Ativo,
            TotalAssociados = await _db.Associados.CountAsync(a => a.IdClube == c.Id && a.SituacaoCadastro == SituacaoCadastro.Aprovado)
        });
    }

    [HttpPost]
    public async Task<ActionResult<ClubeDto>> CreateClube([FromBody] CreateClubeDto request)
    {
        if (!_currentUser.IsSuperAdmin)
        {
            return Forbid();
        }

        var clube = new Clube
        {
            Nome = request.Nome.Trim(),
            Sigla = request.Sigla?.Trim().ToUpper(),
            Endereco = request.Endereco?.Trim(),
            Bairro = request.Bairro?.Trim(),
            Cidade = request.Cidade?.Trim(),
            Uf = request.Uf?.Trim().ToUpper(),
            Telefone = request.Telefone?.Trim(),
            Email = request.Email?.Trim().ToLower(),
            Ativo = true
        };

        _db.Clubes.Add(clube);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClube), new { id = clube.Id }, new ClubeDto
        {
            Id = clube.Id,
            Nome = clube.Nome,
            Sigla = clube.Sigla,
            Endereco = clube.Endereco,
            Bairro = clube.Bairro,
            Cidade = clube.Cidade,
            Uf = clube.Uf,
            Telefone = clube.Telefone,
            Email = clube.Email,
            Ativo = clube.Ativo,
            TotalAssociados = 0
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClube(int id, [FromBody] CreateClubeDto request)
    {
        if (!_currentUser.CanManageClub(id))
        {
            return Forbid();
        }

        var clube = await _db.Clubes.FindAsync(id);
        if (clube == null) return NotFound();

        clube.Nome = request.Nome.Trim();
        clube.Sigla = request.Sigla?.Trim().ToUpper();
        clube.Endereco = request.Endereco?.Trim();
        clube.Bairro = request.Bairro?.Trim();
        clube.Cidade = request.Cidade?.Trim();
        clube.Uf = request.Uf?.Trim().ToUpper();
        clube.Telefone = request.Telefone?.Trim();
        clube.Email = request.Email?.Trim().ToLower();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (!_currentUser.IsSuperAdmin)
        {
            return Forbid();
        }

        var clube = await _db.Clubes.FindAsync(id);
        if (clube == null) return NotFound();

        clube.Ativo = !clube.Ativo;
        await _db.SaveChangesAsync();

        return Ok(new { clube.Id, clube.Ativo });
    }
}
