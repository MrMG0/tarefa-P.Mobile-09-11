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
public class ReservasController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ReservasController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReservaDto>>> GetReservas(
        [FromQuery] int? idClube,
        [FromQuery] int? idEspaco,
        [FromQuery] int? idAssociado,
        [FromQuery] string? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);

        IQueryable<Reserva> query = _db.Reservas
            .Include(r => r.Clube)
            .Include(r => r.Espaco)
            .Include(r => r.Associado);

        if (_currentUser.IsAssociado)
        {
            if (!_currentUser.AssociadoId.HasValue) return Ok(new List<ReservaDto>());
            query = query.Where(r => r.IdAssociado == _currentUser.AssociadoId.Value);
        }
        else
        {
            if (!_currentUser.IsSuperAdmin)
            {
                if (!_currentUser.ClubeId.HasValue) return Forbid();
                query = query.Where(r => r.IdClube == _currentUser.ClubeId.Value);
            }
            else if (effectiveClubeId.HasValue)
            {
                query = query.Where(r => r.IdClube == effectiveClubeId.Value);
            }

            if (idAssociado.HasValue)
            {
                query = query.Where(r => r.IdAssociado == idAssociado.Value);
            }
        }

        if (idEspaco.HasValue)
        {
            query = query.Where(r => r.IdEspaco == idEspaco.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(r => r.DataHoraInicio >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(r => r.DataHoraFim <= dataFim.Value);
        }

        var reservas = await query
            .OrderByDescending(r => r.DataHoraInicio)
            .Select(r => new ReservaDto
            {
                Id = r.Id,
                IdClube = r.IdClube,
                NomeClube = r.Clube != null ? r.Clube.Nome : null,
                IdEspaco = r.IdEspaco,
                NomeEspaco = r.Espaco != null ? r.Espaco.Nome : null,
                IdAssociado = r.IdAssociado,
                NomeAssociado = r.Associado != null ? r.Associado.Nome : null,
                Titulo = r.Titulo,
                DataHoraInicio = r.DataHoraInicio,
                DataHoraFim = r.DataHoraFim,
                Finalidade = r.Finalidade,
                Status = r.Status,
                Observacao = r.Observacao
            })
            .ToListAsync();

        return Ok(reservas);
    }

    [HttpPost]
    public async Task<ActionResult<ReservaDto>> CreateReserva([FromBody] CreateReservaDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        if (request.DataHoraFim <= request.DataHoraInicio)
        {
            return BadRequest("A data e hora de término deve ser posterior à data e hora de início.");
        }

        var espaco = await _db.Espacos.Include(e => e.Clube).FirstOrDefaultAsync(e => e.Id == request.IdEspaco);
        if (espaco == null || !espaco.Ativo)
        {
            return BadRequest("Espaço inválido ou inativo.");
        }

        int targetAssociadoId;
        if (_currentUser.IsAssociado)
        {
            if (!_currentUser.AssociadoId.HasValue)
            {
                return BadRequest("Usuário não possui associado vinculado.");
            }
            targetAssociadoId = _currentUser.AssociadoId.Value;
        }
        else
        {
            if (!request.IdAssociado.HasValue)
            {
                return BadRequest("Informe o associado para o qual a reserva será realizada.");
            }
            targetAssociadoId = request.IdAssociado.Value;
        }

        var associado = await _db.Associados.FindAsync(targetAssociadoId);
        if (associado == null) return NotFound("Associado não encontrado.");

        if (associado.IdClube != espaco.IdClube)
        {
            return BadRequest("O associado não pertence ao clube do espaço reservado.");
        }

        if (associado.SituacaoCadastro != SituacaoCadastro.Aprovado)
        {
            return BadRequest("Apenas associados com cadastro aprovado podem solicitar reservas.");
        }

        // RN09: Verificação rigorosa de conflito de horário para o mesmo espaço
        var conflito = await _db.Reservas
            .AnyAsync(r => r.IdEspaco == request.IdEspaco 
                        && (r.Status == StatusReserva.Aprovada || r.Status == StatusReserva.Pendente)
                        && request.DataHoraInicio < r.DataHoraFim 
                        && request.DataHoraFim > r.DataHoraInicio);

        if (conflito)
        {
            return Conflict(new { Mensagem = "Já existe uma reserva pendente ou aprovada para este espaço no horário solicitado." });
        }

        var reserva = new Reserva
        {
            IdClube = espaco.IdClube,
            IdEspaco = espaco.Id,
            IdAssociado = associado.Id,
            Titulo = request.Titulo?.Trim() ?? $"Reserva {espaco.Nome} - {associado.Nome}",
            DataHoraInicio = request.DataHoraInicio,
            DataHoraFim = request.DataHoraFim,
            Finalidade = request.Finalidade?.Trim(),
            Status = StatusReserva.Pendente,
            Observacao = request.Observacao?.Trim()
        };

        _db.Reservas.Add(reserva);
        await _db.SaveChangesAsync();

        return Ok(new ReservaDto
        {
            Id = reserva.Id,
            IdClube = reserva.IdClube,
            NomeClube = espaco.Clube?.Nome,
            IdEspaco = reserva.IdEspaco,
            NomeEspaco = espaco.Nome,
            IdAssociado = reserva.IdAssociado,
            NomeAssociado = associado.Nome,
            Titulo = reserva.Titulo,
            DataHoraInicio = reserva.DataHoraInicio,
            DataHoraFim = reserva.DataHoraFim,
            Finalidade = reserva.Finalidade,
            Status = reserva.Status,
            Observacao = reserva.Observacao
        });
    }

    [HttpPatch("{id}/aprovar")]
    public async Task<IActionResult> AprovarReserva(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var reserva = await _db.Reservas.FindAsync(id);
        if (reserva == null) return NotFound();

        if (!_currentUser.CanManageClub(reserva.IdClube)) return Forbid();

        // Checar novamente se houve conflito entretanto aprovado
        var conflito = await _db.Reservas
            .AnyAsync(r => r.Id != id 
                        && r.IdEspaco == reserva.IdEspaco 
                        && r.Status == StatusReserva.Aprovada
                        && reserva.DataHoraInicio < r.DataHoraFim 
                        && reserva.DataHoraFim > r.DataHoraInicio);

        if (conflito)
        {
            return Conflict(new { Mensagem = "Não foi possível aprovar: já existe outra reserva conflitante aprovada para o mesmo período." });
        }

        reserva.Status = StatusReserva.Aprovada;
        await _db.SaveChangesAsync();

        return Ok(new { reserva.Id, reserva.Status, Mensagem = "Reserva aprovada com sucesso." });
    }

    [HttpPatch("{id}/rejeitar")]
    public async Task<IActionResult> RejeitarReserva(int id, [FromBody] UpdateStatusReservaDto? dto)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var reserva = await _db.Reservas.FindAsync(id);
        if (reserva == null) return NotFound();

        if (!_currentUser.CanManageClub(reserva.IdClube)) return Forbid();

        reserva.Status = StatusReserva.Rejeitada;
        if (!string.IsNullOrWhiteSpace(dto?.Observacao))
        {
            reserva.Observacao = dto.Observacao;
        }

        await _db.SaveChangesAsync();
        return Ok(new { reserva.Id, reserva.Status, Mensagem = "Reserva rejeitada." });
    }

    [HttpPatch("{id}/cancelar")]
    public async Task<IActionResult> CancelarReserva(int id, [FromBody] UpdateStatusReservaDto? dto)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var reserva = await _db.Reservas.FindAsync(id);
        if (reserva == null) return NotFound();

        if (!_currentUser.IsSuperAdmin)
        {
            var isOwn = _currentUser.IsAssociado && _currentUser.AssociadoId == reserva.IdAssociado;
            var isClubStaff = (_currentUser.IsPresidente || _currentUser.IsAdmin) && _currentUser.ClubeId == reserva.IdClube;

            if (!isOwn && !isClubStaff) return Forbid();
        }

        reserva.Status = StatusReserva.Cancelada;
        if (!string.IsNullOrWhiteSpace(dto?.Observacao))
        {
            reserva.Observacao = dto.Observacao;
        }

        await _db.SaveChangesAsync();
        return Ok(new { reserva.Id, reserva.Status, Mensagem = "Reserva cancelada." });
    }
}
