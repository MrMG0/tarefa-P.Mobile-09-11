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
public class AssociadosController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AssociadosController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssociadoDto>>> GetAssociados([FromQuery] int? idClube, [FromQuery] string? situacao, [FromQuery] string? busca)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);
        if (!_currentUser.IsSuperAdmin && !effectiveClubeId.HasValue)
        {
            return Forbid();
        }

        IQueryable<Associado> query = _db.Associados
            .Include(a => a.Clube)
            .Include(a => a.Dependentes)
            .Include(a => a.Interesses);

        if (effectiveClubeId.HasValue)
        {
            query = query.Where(a => a.IdClube == effectiveClubeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(situacao))
        {
            query = query.Where(a => a.SituacaoCadastro == situacao);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(a => a.Nome.ToLower().Contains(termo) || a.Cpf.Contains(termo) || (a.Email != null && a.Email.ToLower().Contains(termo)));
        }

        var associados = await query
            .AsSplitQuery()
            .OrderBy(a => a.Nome)
            .ToListAsync();

        var list = associados
            .Select(a => new AssociadoDto
            {
                Id = a.Id,
                IdClube = a.IdClube,
                NomeClube = a.Clube != null ? a.Clube.Nome : null,
                Nome = a.Nome,
                Cpf = a.Cpf,
                Endereco = a.Endereco,
                Bairro = a.Bairro,
                Cidade = a.Cidade,
                Uf = a.Uf,
                Sexo = a.Sexo,
                EstadoCivil = a.EstadoCivil,
                Telefone = a.Telefone,
                Email = a.Email,
                DataNascimento = a.DataNascimento,
                TipoAssociado = a.TipoAssociado,
                SituacaoCadastro = a.SituacaoCadastro,
                DataCadastro = a.DataCadastro,
                IdUsuario = a.IdUsuario,
                Dependentes = a.Dependentes.Select(d => new DependenteDto
                {
                    Id = d.Id,
                    IdAssociado = d.IdAssociado,
                    NomeAssociado = a.Nome,
                    Nome = d.Nome,
                    Cpf = d.Cpf,
                    DataNascimento = d.DataNascimento,
                    Parentesco = d.Parentesco,
                    Telefone = d.Telefone,
                    Email = d.Email,
                    Ativo = d.Ativo
                }).ToList(),
                Interesses = a.Interesses.Select(i => i.Interesse).ToList()
            })
            .ToList();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssociadoDto>> GetAssociado(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var a = await _db.Associados
            .Include(x => x.Clube)
            .Include(x => x.Dependentes)
            .Include(x => x.Interesses)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (a == null) return NotFound();

        // Validar permissão de visualização
        if (!_currentUser.IsSuperAdmin)
        {
            if (_currentUser.IsAssociado && _currentUser.AssociadoId != a.Id)
            {
                return Forbid();
            }
            if ((_currentUser.IsPresidente || _currentUser.IsAdmin || _currentUser.IsTesoureiro) && _currentUser.ClubeId != a.IdClube)
            {
                return Forbid();
            }
        }

        return Ok(new AssociadoDto
        {
            Id = a.Id,
            IdClube = a.IdClube,
            NomeClube = a.Clube?.Nome,
            Nome = a.Nome,
            Cpf = a.Cpf,
            Endereco = a.Endereco,
            Bairro = a.Bairro,
            Cidade = a.Cidade,
            Uf = a.Uf,
            Sexo = a.Sexo,
            EstadoCivil = a.EstadoCivil,
            Telefone = a.Telefone,
            Email = a.Email,
            DataNascimento = a.DataNascimento,
            TipoAssociado = a.TipoAssociado,
            SituacaoCadastro = a.SituacaoCadastro,
            DataCadastro = a.DataCadastro,
            IdUsuario = a.IdUsuario,
            Dependentes = a.Dependentes.Select(d => new DependenteDto
            {
                Id = d.Id,
                IdAssociado = d.IdAssociado,
                NomeAssociado = a.Nome,
                Nome = d.Nome,
                Cpf = d.Cpf,
                DataNascimento = d.DataNascimento,
                Parentesco = d.Parentesco,
                Telefone = d.Telefone,
                Email = d.Email,
                Ativo = d.Ativo
            }).ToList(),
            Interesses = a.Interesses.Select(i => i.Interesse).ToList()
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssociado(int id, [FromBody] UpdateAssociadoDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var associado = await _db.Associados.FindAsync(id);
        if (associado == null) return NotFound();

        if (!_currentUser.IsSuperAdmin)
        {
            var isOwn = _currentUser.IsAssociado && _currentUser.AssociadoId == associado.Id;
            var isClubStaff = (_currentUser.IsPresidente || _currentUser.IsAdmin) && _currentUser.ClubeId == associado.IdClube;

            if (!isOwn && !isClubStaff) return Forbid();
        }

        if (!string.IsNullOrWhiteSpace(request.Nome)) associado.Nome = request.Nome.Trim();
        if (request.Endereco != null) associado.Endereco = request.Endereco.Trim();
        if (request.Bairro != null) associado.Bairro = request.Bairro.Trim();
        if (request.Cidade != null) associado.Cidade = request.Cidade.Trim();
        if (request.Uf != null) associado.Uf = request.Uf.Trim().ToUpper();
        if (request.Sexo != null) associado.Sexo = request.Sexo;
        if (request.EstadoCivil != null) associado.EstadoCivil = request.EstadoCivil;
        if (request.Telefone != null) associado.Telefone = request.Telefone.Trim();
        if (request.Email != null) associado.Email = request.Email.Trim().ToLower();
        if (request.DataNascimento.HasValue) associado.DataNascimento = request.DataNascimento;

        // Apenas diretoria pode alterar tipo e situação
        if (_currentUser.IsSuperAdmin || _currentUser.IsPresidente || _currentUser.IsAdmin)
        {
            if (!string.IsNullOrWhiteSpace(request.TipoAssociado)) associado.TipoAssociado = request.TipoAssociado;
            if (!string.IsNullOrWhiteSpace(request.SituacaoCadastro)) associado.SituacaoCadastro = request.SituacaoCadastro;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/aprovar")]
    public async Task<IActionResult> AprovarAssociado(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var associado = await _db.Associados.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.Id == id);
        if (associado == null) return NotFound();

        if (!_currentUser.CanManageClub(associado.IdClube))
        {
            return Forbid();
        }

        associado.SituacaoCadastro = SituacaoCadastro.Aprovado;
        if (associado.Usuario != null)
        {
            associado.Usuario.Ativo = true;
        }

        await _db.SaveChangesAsync();
        return Ok(new { associado.Id, associado.SituacaoCadastro, Mensagem = "Associado aprovado com sucesso." });
    }

    [HttpPatch("{id}/rejeitar")]
    public async Task<IActionResult> RejeitarAssociado(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var associado = await _db.Associados.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.Id == id);
        if (associado == null) return NotFound();

        if (!_currentUser.CanManageClub(associado.IdClube))
        {
            return Forbid();
        }

        associado.SituacaoCadastro = SituacaoCadastro.Rejeitado;
        if (associado.Usuario != null)
        {
            associado.Usuario.Ativo = false;
        }

        await _db.SaveChangesAsync();
        return Ok(new { associado.Id, associado.SituacaoCadastro, Mensagem = "Solicitação rejeitada." });
    }

    [HttpPatch("{id}/desativar")]
    public async Task<IActionResult> DesativarAssociado(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var associado = await _db.Associados.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.Id == id);
        if (associado == null) return NotFound();

        if (!_currentUser.CanManageClub(associado.IdClube))
        {
            return Forbid();
        }

        associado.SituacaoCadastro = SituacaoCadastro.Inativo;
        if (associado.Usuario != null)
        {
            associado.Usuario.Ativo = false;
        }

        await _db.SaveChangesAsync();
        return Ok(new { associado.Id, associado.SituacaoCadastro });
    }

    // --- DEPENDENTES ---
    [HttpGet("{id}/dependentes")]
    public async Task<ActionResult<List<DependenteDto>>> GetDependentes(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var associado = await _db.Associados.FindAsync(id);
        if (associado == null) return NotFound();

        if (!_currentUser.IsSuperAdmin)
        {
            if (_currentUser.IsAssociado && _currentUser.AssociadoId != id) return Forbid();
            if ((_currentUser.IsPresidente || _currentUser.IsAdmin) && _currentUser.ClubeId != associado.IdClube) return Forbid();
        }

        var dependentes = await _db.Dependentes
            .Where(d => d.IdAssociado == id)
            .Select(d => new DependenteDto
            {
                Id = d.Id,
                IdAssociado = d.IdAssociado,
                NomeAssociado = associado.Nome,
                Nome = d.Nome,
                Cpf = d.Cpf,
                DataNascimento = d.DataNascimento,
                Parentesco = d.Parentesco,
                Telefone = d.Telefone,
                Email = d.Email,
                Ativo = d.Ativo
            })
            .ToListAsync();

        return Ok(dependentes);
    }

    [HttpPost("{id}/dependentes")]
    public async Task<ActionResult<DependenteDto>> CreateDependente(int id, [FromBody] CreateDependenteDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var associado = await _db.Associados.FindAsync(id);
        if (associado == null) return NotFound();

        if (!_currentUser.IsSuperAdmin)
        {
            if (_currentUser.IsAssociado && _currentUser.AssociadoId != id) return Forbid();
            if ((_currentUser.IsPresidente || _currentUser.IsAdmin) && _currentUser.ClubeId != associado.IdClube) return Forbid();
        }

        var dependente = new Dependente
        {
            IdAssociado = id,
            Nome = request.Nome.Trim(),
            Cpf = request.Cpf?.Trim(),
            DataNascimento = request.DataNascimento,
            Parentesco = request.Parentesco?.Trim(),
            Telefone = request.Telefone?.Trim(),
            Email = request.Email?.Trim().ToLower(),
            Ativo = true
        };

        _db.Dependentes.Add(dependente);
        await _db.SaveChangesAsync();

        return Ok(new DependenteDto
        {
            Id = dependente.Id,
            IdAssociado = id,
            NomeAssociado = associado.Nome,
            Nome = dependente.Nome,
            Cpf = dependente.Cpf,
            DataNascimento = dependente.DataNascimento,
            Parentesco = dependente.Parentesco,
            Telefone = dependente.Telefone,
            Email = dependente.Email,
            Ativo = dependente.Ativo
        });
    }

    [HttpDelete("{id}/dependentes/{depId}")]
    public async Task<IActionResult> DeleteDependente(int id, int depId)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var dep = await _db.Dependentes.FirstOrDefaultAsync(d => d.Id == depId && d.IdAssociado == id);
        if (dep == null) return NotFound();

        var associado = await _db.Associados.FindAsync(id);
        if (associado == null) return NotFound();

        if (!_currentUser.IsSuperAdmin)
        {
            if (_currentUser.IsAssociado && _currentUser.AssociadoId != id) return Forbid();
            if ((_currentUser.IsPresidente || _currentUser.IsAdmin) && _currentUser.ClubeId != associado.IdClube) return Forbid();
        }

        _db.Dependentes.Remove(dep);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // --- INTERESSES ---
    [HttpGet("{id}/interesses")]
    public async Task<ActionResult<List<string>>> GetInteresses(int id)
    {
        var interesses = await _db.InteressesAssociado
            .Where(i => i.IdAssociado == id)
            .Select(i => i.Interesse)
            .ToListAsync();

        return Ok(interesses);
    }

    [HttpPost("{id}/interesses")]
    public async Task<IActionResult> AddInteresse(int id, [FromBody] string interesse)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();
        if (string.IsNullOrWhiteSpace(interesse)) return BadRequest("Interesse não pode ser vazio.");

        var associado = await _db.Associados.FindAsync(id);
        if (associado == null) return NotFound();

        if (!_currentUser.IsSuperAdmin && _currentUser.AssociadoId != id && !_currentUser.CanManageClub(associado.IdClube))
        {
            return Forbid();
        }

        var existe = await _db.InteressesAssociado.AnyAsync(i => i.IdAssociado == id && i.Interesse.ToLower() == interesse.Trim().ToLower());
        if (!existe)
        {
            _db.InteressesAssociado.Add(new InteresseAssociado
            {
                IdAssociado = id,
                Interesse = interesse.Trim()
            });
            await _db.SaveChangesAsync();
        }

        return Ok(new { IdAssociado = id, Interesse = interesse.Trim() });
    }

    [HttpDelete("{id}/interesses/{interesse}")]
    public async Task<IActionResult> RemoveInteresse(int id, string interesse)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var associado = await _db.Associados.FindAsync(id);
        if (associado == null) return NotFound();

        if (!_currentUser.IsSuperAdmin && _currentUser.AssociadoId != id && !_currentUser.CanManageClub(associado.IdClube))
        {
            return Forbid();
        }

        var item = await _db.InteressesAssociado
            .FirstOrDefaultAsync(i => i.IdAssociado == id && i.Interesse.ToLower() == interesse.Trim().ToLower());

        if (item != null)
        {
            _db.InteressesAssociado.Remove(item);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }
}
