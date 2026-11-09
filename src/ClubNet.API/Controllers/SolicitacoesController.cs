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
public class SolicitacoesController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SolicitacoesController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<SolicitacaoDto>>> GetSolicitacoes([FromQuery] int? idClube, [FromQuery] string? status)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);

        IQueryable<Solicitacao> query = _db.Solicitacoes
            .Include(s => s.Clube)
            .Include(s => s.Associado);

        if (_currentUser.IsAssociado)
        {
            if (!_currentUser.AssociadoId.HasValue) return Ok(new List<SolicitacaoDto>());
            query = query.Where(s => s.IdAssociado == _currentUser.AssociadoId.Value);
        }
        else
        {
            if (!_currentUser.IsSuperAdmin)
            {
                if (!_currentUser.ClubeId.HasValue) return Forbid();
                query = query.Where(s => s.IdClube == _currentUser.ClubeId.Value);
            }
            else if (effectiveClubeId.HasValue)
            {
                query = query.Where(s => s.IdClube == effectiveClubeId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var list = await query
            .OrderByDescending(s => s.DataAbertura)
            .Select(s => new SolicitacaoDto
            {
                Id = s.Id,
                IdClube = s.IdClube,
                NomeClube = s.Clube != null ? s.Clube.Nome : null,
                IdAssociado = s.IdAssociado,
                NomeAssociado = s.Associado != null ? s.Associado.Nome : null,
                Tipo = s.Tipo,
                Descricao = s.Descricao,
                DataAbertura = s.DataAbertura,
                Status = s.Status,
                Resposta = s.Resposta
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<SolicitacaoDto>> CreateSolicitacao([FromBody] CreateSolicitacaoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var targetClubeId = _currentUser.GetEffectiveClubeId(request.IdClube);
        if (!targetClubeId.HasValue) return BadRequest("Clube não identificado.");

        var solicitacao = new Solicitacao
        {
            IdClube = targetClubeId.Value,
            IdAssociado = _currentUser.AssociadoId ?? request.IdAssociado,
            Tipo = request.Tipo.Trim(),
            Descricao = request.Descricao.Trim(),
            DataAbertura = DateTime.UtcNow,
            Status = StatusAtendimento.Aberta
        };

        _db.Solicitacoes.Add(solicitacao);
        await _db.SaveChangesAsync();

        var clube = await _db.Clubes.FindAsync(solicitacao.IdClube);
        var associado = solicitacao.IdAssociado.HasValue ? await _db.Associados.FindAsync(solicitacao.IdAssociado.Value) : null;

        return Ok(new SolicitacaoDto
        {
            Id = solicitacao.Id,
            IdClube = solicitacao.IdClube,
            NomeClube = clube?.Nome,
            IdAssociado = solicitacao.IdAssociado,
            NomeAssociado = associado?.Nome,
            Tipo = solicitacao.Tipo,
            Descricao = solicitacao.Descricao,
            DataAbertura = solicitacao.DataAbertura,
            Status = solicitacao.Status,
            Resposta = solicitacao.Resposta
        });
    }

    [HttpPatch("{id}/atendimento")]
    public async Task<IActionResult> ResponderSolicitacao(int id, [FromBody] UpdateAtendimentoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var solicitacao = await _db.Solicitacoes.FindAsync(id);
        if (solicitacao == null) return NotFound();

        if (!_currentUser.CanManageClub(solicitacao.IdClube)) return Forbid();

        solicitacao.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Resposta))
        {
            solicitacao.Resposta = request.Resposta.Trim();
        }

        await _db.SaveChangesAsync();
        return Ok(new { solicitacao.Id, solicitacao.Status, solicitacao.Resposta });
    }
}
