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
public class AuthController : ControllerBase
{
    private readonly ClubNetDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(ClubNetDbContext db, IJwtService jwtService, ICurrentUserService currentUser)
    {
        _db = db;
        _jwtService = jwtService;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            return BadRequest(new LoginResponseDto { Sucesso = false, Mensagem = "E-mail e senha são obrigatórios." });
        }

        var usuario = await _db.Usuarios
            .Include(u => u.Clube)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());

        if (usuario == null || !PasswordHasher.VerifyPassword(request.Senha, usuario.SenhaHash))
        {
            return Unauthorized(new LoginResponseDto { Sucesso = false, Mensagem = "Credenciais inválidas." });
        }

        if (!usuario.Ativo)
        {
            return Unauthorized(new LoginResponseDto { Sucesso = false, Mensagem = "Usuário inativo. Entre em contato com a administração." });
        }

        // Buscar id do associado vinculado, se existir
        var associado = await _db.Associados
            .FirstOrDefaultAsync(a => a.IdUsuario == usuario.Id || (usuario.IdClube.HasValue && a.Email == usuario.Email && a.IdClube == usuario.IdClube));
        Dependente? dependente = null;

        if (usuario.Perfil == Perfis.Dependente && usuario.IdClube.HasValue)
        {
            dependente = await _db.Dependentes
                .Include(d => d.Associado)
                .FirstOrDefaultAsync(d => d.Email == usuario.Email && d.Associado!.IdClube == usuario.IdClube);
            if (dependente?.Associado == null || dependente.Associado.SituacaoCadastro != SituacaoCadastro.Aprovado)
            {
                return Unauthorized(new LoginResponseDto { Sucesso = false, Mensagem = "A conta do dependente não está vinculada a um associado aprovado." });
            }

            associado = dependente.Associado;
        }

        // Se for perfil associado e o cadastro estiver pendente ou rejeitado
        if (usuario.Perfil == Perfis.Associado && associado != null)
        {
            if (associado.SituacaoCadastro == SituacaoCadastro.Pendente)
            {
                return Unauthorized(new LoginResponseDto { Sucesso = false, Mensagem = "Sua solicitação de associação ainda está em análise pela diretoria." });
            }
            if (associado.SituacaoCadastro == SituacaoCadastro.Rejeitado)
            {
                return BadRequest(new LoginResponseDto { Sucesso = false, Mensagem = "Sua solicitação de associação foi rejeitada pela administração." });
            }
        }

        var token = _jwtService.GenerateToken(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil,
            usuario.IdClube,
            associado?.Id,
            dependente?.Id
        );

        return Ok(new LoginResponseDto
        {
            Sucesso = true,
            Mensagem = "Login realizado com sucesso.",
            Token = token,
            Usuario = new UserInfoDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil,
                IdClube = usuario.IdClube,
                NomeClube = usuario.Clube?.Nome,
                IdAssociado = associado?.Id,
                IdDependente = dependente?.Id
            }
        });
    }

    [HttpPost("register-member")]
    public async Task<ActionResult<LoginResponseDto>> RegisterMember([FromBody] RegisterMemberDto request)
    {
        var clube = await _db.Clubes.FindAsync(request.IdClube);
        if (clube == null || !clube.Ativo)
        {
            return BadRequest(new LoginResponseDto { Sucesso = false, Mensagem = "Clube selecionado inválido ou inativo." });
        }

        var tipoAssociado = string.IsNullOrWhiteSpace(request.TipoAssociado)
            ? TiposAssociado.Convidado
            : request.TipoAssociado.Trim().ToUpperInvariant();
        if (tipoAssociado != TiposAssociado.Convidado && tipoAssociado != TiposAssociado.Simpatizante)
        {
            return BadRequest(new LoginResponseDto { Sucesso = false, Mensagem = "No cadastro pelo aplicativo, selecione Convidado ou Simpatizante." });
        }

        var emailNormalizado = request.Email.Trim().ToLower();
        var emailExiste = await _db.Usuarios.AnyAsync(u => u.Email.ToLower() == emailNormalizado);
        if (emailExiste)
        {
            return BadRequest(new LoginResponseDto { Sucesso = false, Mensagem = "Este e-mail já está cadastrado no sistema." });
        }

        var cpfLimpo = request.Cpf.Replace(".", "").Replace("-", "").Trim();
        var cpfExisteNoClube = await _db.Associados.AnyAsync(a => a.IdClube == request.IdClube && a.Cpf.Replace(".", "").Replace("-", "").Trim() == cpfLimpo);
        if (cpfExisteNoClube)
        {
            return BadRequest(new LoginResponseDto { Sucesso = false, Mensagem = "Já existe uma solicitação ou associado com este CPF neste clube." });
        }

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = emailNormalizado,
            SenhaHash = PasswordHasher.HashPassword(request.Senha),
            Perfil = Perfis.Associado,
            IdClube = request.IdClube,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var associado = new Associado
        {
            IdClube = request.IdClube,
            Nome = request.Nome.Trim(),
            Cpf = request.Cpf.Trim(),
            Email = emailNormalizado,
            Endereco = request.Endereco?.Trim(),
            Bairro = request.Bairro?.Trim(),
            Cidade = request.Cidade?.Trim(),
            Uf = request.Uf?.Trim().ToUpper(),
            Sexo = request.Sexo,
            EstadoCivil = request.EstadoCivil,
            Telefone = request.Telefone?.Trim(),
            DataNascimento = request.DataNascimento,
            TipoAssociado = tipoAssociado,
            SituacaoCadastro = SituacaoCadastro.Pendente,
            DataCadastro = DateTime.UtcNow,
            IdUsuario = usuario.Id
        };

        _db.Associados.Add(associado);
        await _db.SaveChangesAsync();

        if (request.InteressesIniciais != null && request.InteressesIniciais.Count > 0)
        {
            foreach (var interesse in request.InteressesIniciais.Where(i => !string.IsNullOrWhiteSpace(i)))
            {
                _db.InteressesAssociado.Add(new InteresseAssociado
                {
                    IdAssociado = associado.Id,
                    Interesse = interesse.Trim()
                });
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new LoginResponseDto
        {
            Sucesso = true,
            Mensagem = "Solicitação de associação enviada com sucesso! Aguarde a análise da diretoria.",
        });
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        var usuario = await _db.Usuarios
            .Include(u => u.Clube)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value);

        if (usuario == null) return NotFound();

        var associado = await _db.Associados
            .FirstOrDefaultAsync(a => a.IdUsuario == usuario.Id || (usuario.IdClube.HasValue && a.Email == usuario.Email && a.IdClube == usuario.IdClube));
        var dependente = usuario.Perfil == Perfis.Dependente
            ? await _db.Dependentes.FirstOrDefaultAsync(d => d.Email == usuario.Email && d.Associado!.IdClube == usuario.IdClube)
            : null;

        return Ok(new UserInfoDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
            IdClube = usuario.IdClube,
            NomeClube = usuario.Clube?.Nome,
            IdAssociado = dependente?.IdAssociado ?? associado?.Id,
            IdDependente = dependente?.Id
        });
    }
}
