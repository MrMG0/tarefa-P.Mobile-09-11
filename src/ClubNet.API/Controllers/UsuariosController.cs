using ClubNet.API.Data;
using ClubNet.API.Security;
using ClubNet.API.Services;
using ClubNet.Shared.Constants;
using ClubNet.Shared.DTOs;
using ClubNet.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UsuariosController(ClubNetDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserInfoDto>>> GetUsuarios([FromQuery] int? idClube)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var effectiveClubeId = _currentUser.GetEffectiveClubeId(idClube);

        IQueryable<Usuario> query = _db.Usuarios.Include(u => u.Clube);

        if (!_currentUser.IsSuperAdmin)
        {
            if (!_currentUser.ClubeId.HasValue || (!_currentUser.IsPresidente && !_currentUser.IsAdmin))
            {
                return Forbid();
            }
            query = query.Where(u => u.IdClube == _currentUser.ClubeId.Value);
        }
        else if (effectiveClubeId.HasValue)
        {
            query = query.Where(u => u.IdClube == effectiveClubeId.Value);
        }

        var usuarios = await query
            .OrderBy(u => u.Nome)
            .Select(u => new UserInfoDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Perfil = u.Perfil,
                IdClube = u.IdClube,
                NomeClube = u.Clube != null ? u.Clube.Nome : null
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpPost]
    public async Task<ActionResult<UserInfoDto>> CreateUsuario([FromBody] CreateUsuarioDto request)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        // RF24: Presidente pode criar ADMIN ou TESOUREIRO para seu próprio clube
        // Superadmin pode criar qualquer perfil
        var targetClubeId = _currentUser.IsSuperAdmin ? request.IdClube : _currentUser.ClubeId;

        if (!_currentUser.IsSuperAdmin)
        {
            if (!_currentUser.IsPresidente || !targetClubeId.HasValue)
            {
                return Forbid();
            }

            // Presidente só pode criar ADMIN ou TESOUREIRO
            if (request.Perfil != Perfis.Admin && request.Perfil != Perfis.Tesoureiro)
            {
                return BadRequest("O presidente só pode criar usuários com perfil ADMIN ou TESOUREIRO.");
            }
        }

        var emailNormalizado = request.Email.Trim().ToLower();
        var existe = await _db.Usuarios.AnyAsync(u => u.Email.ToLower() == emailNormalizado);
        if (existe)
        {
            return BadRequest("Este e-mail já está em uso.");
        }

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = emailNormalizado,
            SenhaHash = PasswordHasher.HashPassword(request.Senha),
            Perfil = request.Perfil,
            IdClube = targetClubeId,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        string? nomeClube = null;
        if (targetClubeId.HasValue)
        {
            nomeClube = (await _db.Clubes.FindAsync(targetClubeId.Value))?.Nome;
        }

        return Ok(new UserInfoDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
            IdClube = usuario.IdClube,
            NomeClube = nomeClube
        });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();

        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        if (!_currentUser.IsSuperAdmin)
        {
            if (!_currentUser.IsPresidente || usuario.IdClube != _currentUser.ClubeId)
            {
                return Forbid();
            }
        }

        usuario.Ativo = !usuario.Ativo;
        await _db.SaveChangesAsync();

        return Ok(new { usuario.Id, usuario.Ativo });
    }
}
