using ClubNet.API.Data;
using ClubNet.API.Services;
using ClubNet.Shared.Constants;
using ClubNet.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardStatsDto>> GetStats([FromQuery] int? idClube)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);

        var queryClubes = _db.Clubes.AsQueryable();
        var queryAssociados = _db.Associados.AsQueryable();
        var queryEventos = _db.Eventos.Where(e => e.Ativo && e.DataInicio >= DateTime.UtcNow.Date);
        var queryReservas = _db.Reservas.AsQueryable();
        var queryMensalidades = _db.Mensalidades.AsQueryable();
        var querySolicitacoes = _db.Solicitacoes.AsQueryable();
        var queryReclamacoes = _db.Reclamacoes.AsQueryable();

        if (effectiveClubeId.HasValue)
        {
            var cId = effectiveClubeId.Value;
            queryAssociados = queryAssociados.Where(a => a.IdClube == cId);
            queryEventos = queryEventos.Where(e => e.IdClube == cId);
            queryReservas = queryReservas.Where(r => r.IdClube == cId);
            queryMensalidades = queryMensalidades.Where(m => m.IdClube == cId);
            querySolicitacoes = querySolicitacoes.Where(s => s.IdClube == cId);
            queryReclamacoes = queryReclamacoes.Where(rc => rc.IdClube == cId);
        }

        var openMensalidades = await queryMensalidades
            .Where(m => m.Situacao == SituacaoMensalidade.Aberta || m.Situacao == SituacaoMensalidade.Vencida)
            .ToListAsync();

        var stats = new DashboardStatsDto
        {
            TotalClubes = await queryClubes.CountAsync(),
            TotalAssociadosAtivos = await queryAssociados.CountAsync(a => a.SituacaoCadastro == SituacaoCadastro.Aprovado),
            TotalSolicitacoesAssociacaoPendentes = await queryAssociados.CountAsync(a => a.SituacaoCadastro == SituacaoCadastro.Pendente),
            TotalEventosProximos = await queryEventos.CountAsync(),
            TotalReservasPendentes = await queryReservas.CountAsync(r => r.Status == StatusReserva.Pendente),
            TotalMensalidadesAbertas = openMensalidades.Count,
            ValorTotalMensalidadesAbertas = openMensalidades.Sum(m => m.Valor),
            TotalSolicitacoesAbertas = await querySolicitacoes.CountAsync(s => s.Status == StatusAtendimento.Aberta || s.Status == StatusAtendimento.EmAnalise),
            TotalReclamacoesAbertas = await queryReclamacoes.CountAsync(r => r.Status == StatusAtendimento.Aberta || r.Status == StatusAtendimento.EmAnalise)
        };

        return Ok(stats);
    }
}
