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
public class ReclamacoesController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ReclamacoesController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReclamacaoDto>>> GetReclamacoes([FromQuery] int? idClube, [FromQuery] string? status)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);

        IQueryable<Reclamacao> query = _db.Reclamacoes
            .Include(r => r.Clube)
            .Include(r => r.Associado);

        if (_currentUser.IsAssociado)
        {
            if (!_currentUser.AssociadoId.HasValue) return Ok(new List<ReclamacaoDto>());
            query = query.Where(r => r.IdAssociado == _currentUser.AssociadoId.Value);
        }
        else
        {
            if (!_currentUser.IsSuperAdmin)
            {
                if (!_currentUser.ClubeId.HasValue || (!_currentUser.IsPresidente && !_currentUser.IsAdmin))
                {
                    return Forbid();
                }
                query = query.Where(r => r.IdClube == _currentUser.ClubeId.Value);
            }
            else if (effectiveClubeId.HasValue)
            {
                query = query.Where(r => r.IdClube == effectiveClubeId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var list = await query
            .OrderByDescending(r => r.DataAbertura)
            .Select(r => new ReclamacaoDto
            {
                Id = r.Id,
                IdClube = r.IdClube,
                NomeClube = r.Clube != null ? r.Clube.Nome : null,
                IdAssociado = r.IdAssociado,
                NomeAssociado = r.Associado != null ? r.Associado.Nome : null,
                Assunto = r.Assunto,
                Descricao = r.Descricao,
                DataAbertura = r.DataAbertura,
                Status = r.Status,
                Resposta = r.Resposta
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<ReclamacaoDto>> CreateReclamacao([FromBody] CreateReclamacaoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var targetClubeId = _currentUser.GetEffectiveClubeId(request.IdClube);
        if (!targetClubeId.HasValue) return BadRequest("Clube não identificado.");

        var reclamacao = new Reclamacao
        {
            IdClube = targetClubeId.Value,
            IdAssociado = _currentUser.AssociadoId ?? request.IdAssociado,
            Assunto = request.Assunto?.Trim(),
            Descricao = request.Descricao.Trim(),
            DataAbertura = DateTime.UtcNow,
            Status = StatusAtendimento.Aberta
        };

        _db.Reclamacoes.Add(reclamacao);
        await _db.SaveChangesAsync();

        var clube = await _db.Clubes.FindAsync(reclamacao.IdClube);
        var associado = reclamacao.IdAssociado.HasValue ? await _db.Associados.FindAsync(reclamacao.IdAssociado.Value) : null;

        return Ok(new ReclamacaoDto
        {
            Id = reclamacao.Id,
            IdClube = reclamacao.IdClube,
            NomeClube = clube?.Nome,
            IdAssociado = reclamacao.IdAssociado,
            NomeAssociado = associado?.Nome,
            Assunto = reclamacao.Assunto,
            Descricao = reclamacao.Descricao,
            DataAbertura = reclamacao.DataAbertura,
            Status = reclamacao.Status,
            Resposta = reclamacao.Resposta
        });
    }

    [HttpPatch("{id}/atendimento")]
    public async Task<IActionResult> ResponderReclamacao(int id, [FromBody] UpdateAtendimentoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var reclamacao = await _db.Reclamacoes.FindAsync(id);
        if (reclamacao == null) return NotFound();

        if (!_currentUser.CanManageClub(reclamacao.IdClube)) return Forbid();

        reclamacao.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Resposta))
        {
            reclamacao.Resposta = request.Resposta.Trim();
        }

        await _db.SaveChangesAsync();
        return Ok(new { reclamacao.Id, reclamacao.Status, reclamacao.Resposta });
    }
}
