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
public class MensalidadesController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MensalidadesController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<MensalidadeDto>>> GetMensalidades(
        [FromQuery] int? idClube, 
        [FromQuery] int? idAssociado, 
        [FromQuery] string? situacao, 
        [FromQuery] string? referenciaMes)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);

        IQueryable<Mensalidade> query = _db.Mensalidades
            .Include(m => m.Clube)
            .Include(m => m.Associado);

        // Se for associado, só vê as suas próprias mensalidades
        if (_currentUser.IsAssociado)
        {
            if (!_currentUser.AssociadoId.HasValue) return Ok(new List<MensalidadeDto>());
            query = query.Where(m => m.IdAssociado == _currentUser.AssociadoId.Value);
        }
        else
        {
            // Se for diretoria/tesouraria, filtrar por clube
            if (!_currentUser.IsSuperAdmin)
            {
                if (!_currentUser.ClubeId.HasValue) return Forbid();
                query = query.Where(m => m.IdClube == _currentUser.ClubeId.Value);
            }
            else if (effectiveClubeId.HasValue)
            {
                query = query.Where(m => m.IdClube == effectiveClubeId.Value);
            }

            if (idAssociado.HasValue)
            {
                query = query.Where(m => m.IdAssociado == idAssociado.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(situacao))
        {
            query = query.Where(m => m.Situacao == situacao);
        }

        if (!string.IsNullOrWhiteSpace(referenciaMes))
        {
            query = query.Where(m => m.ReferenciaMes == referenciaMes);
        }

        var mensalidades = await query
            .OrderByDescending(m => m.DataVencimento)
            .Select(m => new MensalidadeDto
            {
                Id = m.Id,
                IdClube = m.IdClube,
                NomeClube = m.Clube != null ? m.Clube.Nome : null,
                IdAssociado = m.IdAssociado,
                NomeAssociado = m.Associado != null ? m.Associado.Nome : null,
                ReferenciaMes = m.ReferenciaMes,
                Valor = m.Valor,
                DataVencimento = m.DataVencimento,
                DataPagamento = m.DataPagamento,
                Situacao = m.Situacao,
                Observacao = m.Observacao
            })
            .ToListAsync();

        return Ok(mensalidades);
    }

    [HttpPost]
    public async Task<ActionResult<MensalidadeDto>> CreateMensalidade([FromBody] CreateMensalidadeDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var associado = await _db.Associados.Include(a => a.Clube).FirstOrDefaultAsync(a => a.Id == request.IdAssociado);
        if (associado == null) return NotFound("Associado não encontrado.");

        if (!_currentUser.CanManageFinance(associado.IdClube))
        {
            return Forbid();
        }

        var mensalidade = new Mensalidade
        {
            IdClube = associado.IdClube,
            IdAssociado = associado.Id,
            ReferenciaMes = request.ReferenciaMes.Trim(),
            Valor = request.Valor,
            DataVencimento = request.DataVencimento,
            Situacao = SituacaoMensalidade.Aberta,
            Observacao = request.Observacao?.Trim()
        };

        _db.Mensalidades.Add(mensalidade);
        await _db.SaveChangesAsync();

        return Ok(new MensalidadeDto
        {
            Id = mensalidade.Id,
            IdClube = mensalidade.IdClube,
            NomeClube = associado.Clube?.Nome,
            IdAssociado = mensalidade.IdAssociado,
            NomeAssociado = associado.Nome,
            ReferenciaMes = mensalidade.ReferenciaMes,
            Valor = mensalidade.Valor,
            DataVencimento = mensalidade.DataVencimento,
            DataPagamento = mensalidade.DataPagamento,
            Situacao = mensalidade.Situacao,
            Observacao = mensalidade.Observacao
        });
    }

    [HttpPatch("{id}/situacao")]
    public async Task<IActionResult> UpdateSituacao(int id, [FromBody] UpdateSituacaoMensalidadeDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var mensalidade = await _db.Mensalidades.FindAsync(id);
        if (mensalidade == null) return NotFound();

        if (!_currentUser.CanManageFinance(mensalidade.IdClube))
        {
            return Forbid();
        }

        mensalidade.Situacao = request.Situacao;
        if (request.Situacao == SituacaoMensalidade.Paga)
        {
            mensalidade.DataPagamento = request.DataPagamento ?? DateTime.UtcNow;
        }
        else if (request.Situacao == SituacaoMensalidade.Aberta)
        {
            mensalidade.DataPagamento = null;
        }

        if (request.Observacao != null)
        {
            mensalidade.Observacao = request.Observacao;
        }

        await _db.SaveChangesAsync();
        return Ok(new { mensalidade.Id, mensalidade.Situacao, mensalidade.DataPagamento });
    }
}
