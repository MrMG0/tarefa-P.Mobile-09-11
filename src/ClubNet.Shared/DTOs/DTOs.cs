using System.ComponentModel.DataAnnotations;

namespace ClubNet.Shared.DTOs;

// --- AUTH & USUÁRIO ---
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? Token { get; set; }
    public UserInfoDto? Usuario { get; set; }
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public int? IdClube { get; set; }
    public string? NomeClube { get; set; }
    public int? IdAssociado { get; set; }
}

public class RegisterMemberDto
{
    [Required]
    public int IdClube { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public string Cpf { get; set; } = string.Empty;

    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? TipoAssociado { get; set; } = "ASSOCIADO";
    public List<string>? InteressesIniciais { get; set; }
}

public class CreateUsuarioDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public string Perfil { get; set; } = string.Empty;

    public int? IdClube { get; set; }
}

// --- CLUBE ---
public class ClubeDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Sigla { get; set; }
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativo { get; set; }
    public int TotalAssociados { get; set; }
}

public class CreateClubeDto
{
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Sigla { get; set; }

    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}

// --- ASSOCIADO ---
public class AssociadoDto
{
    public int Id { get; set; }
    public int IdClube { get; set; }
    public string? NomeClube { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string TipoAssociado { get; set; } = string.Empty;
    public string SituacaoCadastro { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public int? IdUsuario { get; set; }
    public List<DependenteDto> Dependentes { get; set; } = new();
    public List<string> Interesses { get; set; } = new();
}

public class UpdateAssociadoDto
{
    public string? Nome { get; set; }
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? TipoAssociado { get; set; }
    public string? SituacaoCadastro { get; set; }
}

// --- DEPENDENTE ---
public class DependenteDto
{
    public int Id { get; set; }
    public int IdAssociado { get; set; }
    public string? NomeAssociado { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Parentesco { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativo { get; set; }
}

public class CreateDependenteDto
{
    [Required]
    public int IdAssociado { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    public string? Cpf { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Parentesco { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}

// --- EVENTO ---
public class EventoDto
{
    public int Id { get; set; }
    public int IdClube { get; set; }
    public string? NomeClube { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string PublicoAlvo { get; set; } = "ASSOCIADOS";
    public string? Local { get; set; }
    public bool Ativo { get; set; }
}

public class CreateEventoDto
{
    public int? IdClube { get; set; }

    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    [Required]
    public DateTime DataInicio { get; set; }

    public DateTime? DataFim { get; set; }

    public string PublicoAlvo { get; set; } = "ASSOCIADOS";
    public string? Local { get; set; }
}

// --- MENSALIDADE ---
public class MensalidadeDto
{
    public int Id { get; set; }
    public int IdClube { get; set; }
    public string? NomeClube { get; set; }
    public int IdAssociado { get; set; }
    public string? NomeAssociado { get; set; }
    public string ReferenciaMes { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string Situacao { get; set; } = "ABERTA";
    public string? Observacao { get; set; }
}

public class CreateMensalidadeDto
{
    public int? IdClube { get; set; }

    [Required]
    public int IdAssociado { get; set; }

    [Required]
    public string ReferenciaMes { get; set; } = string.Empty;

    [Required]
    public decimal Valor { get; set; }

    [Required]
    public DateTime DataVencimento { get; set; }

    public string? Observacao { get; set; }
}

public class UpdateSituacaoMensalidadeDto
{
    [Required]
    public string Situacao { get; set; } = string.Empty;
    public DateTime? DataPagamento { get; set; }
    public string? Observacao { get; set; }
}

// --- ESPAÇO ---
public class EspacoDto
{
    public int Id { get; set; }
    public int IdClube { get; set; }
    public string? NomeClube { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? Capacidade { get; set; }
    public bool Ativo { get; set; }
}

public class CreateEspacoDto
{
    public int? IdClube { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }
    public int? Capacidade { get; set; }
}

// --- RESERVA ---
public class ReservaDto
{
    public int Id { get; set; }
    public int IdClube { get; set; }
    public string? NomeClube { get; set; }
    public int IdEspaco { get; set; }
    public string? NomeEspaco { get; set; }
    public int IdAssociado { get; set; }
    public string? NomeAssociado { get; set; }
    public string? Titulo { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string? Finalidade { get; set; }
    public string Status { get; set; } = "PENDENTE";
    public string? Observacao { get; set; }
}

public class CreateReservaDto
{
    public int? IdClube { get; set; }

    [Required]
    public int IdEspaco { get; set; }

    public int? IdAssociado { get; set; }

    public string? Titulo { get; set; }

    [Required]
    public DateTime DataHoraInicio { get; set; }

    [Required]
    public DateTime DataHoraFim { get; set; }

    public string? Finalidade { get; set; }
    public string? Observacao { get; set; }
}

public class UpdateStatusReservaDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
    public string? Observacao { get; set; }
}

// --- SOLICITAÇÃO & RECLAMAÇÃO ---
public class SolicitacaoDto
{
    public int Id { get; set; }
    public int IdClube { get; set; }
    public string? NomeClube { get; set; }
    public int? IdAssociado { get; set; }
    public string? NomeAssociado { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public string Status { get; set; } = "ABERTA";
    public string? Resposta { get; set; }
}

public class CreateSolicitacaoDto
{
    public int? IdClube { get; set; }
    public int? IdAssociado { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    public string Descricao { get; set; } = string.Empty;
}

public class ReclamacaoDto
{
    public int Id { get; set; }
    public int IdClube { get; set; }
    public string? NomeClube { get; set; }
    public int? IdAssociado { get; set; }
    public string? NomeAssociado { get; set; }
    public string? Assunto { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public string Status { get; set; } = "ABERTA";
    public string? Resposta { get; set; }
}

public class CreateReclamacaoDto
{
    public int? IdClube { get; set; }
    public int? IdAssociado { get; set; }

    public string? Assunto { get; set; }

    [Required]
    public string Descricao { get; set; } = string.Empty;
}

public class UpdateAtendimentoDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
    public string? Resposta { get; set; }
}

// --- DASHBOARD ---
public class DashboardStatsDto
{
    public int TotalClubes { get; set; }
    public int TotalAssociadosAtivos { get; set; }
    public int TotalSolicitacoesAssociacaoPendentes { get; set; }
    public int TotalEventosProximos { get; set; }
    public int TotalReservasPendentes { get; set; }
    public int TotalMensalidadesAbertas { get; set; }
    public decimal ValorTotalMensalidadesAbertas { get; set; }
    public int TotalSolicitacoesAbertas { get; set; }
    public int TotalReclamacoesAbertas { get; set; }
}
