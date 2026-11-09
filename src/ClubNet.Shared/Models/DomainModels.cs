using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubNet.Shared.Models;

[Table("clubes")]
public class Clube
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(30)]
    [Column("sigla")]
    public string? Sigla { get; set; }

    [MaxLength(200)]
    [Column("endereco")]
    public string? Endereco { get; set; }

    [MaxLength(100)]
    [Column("bairro")]
    public string? Bairro { get; set; }

    [MaxLength(100)]
    [Column("cidade")]
    public string? Cidade { get; set; }

    [MaxLength(2)]
    [Column("uf")]
    public string? Uf { get; set; }

    [MaxLength(20)]
    [Column("telefone")]
    public string? Telefone { get; set; }

    [MaxLength(200)]
    [Column("email")]
    public string? Email { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("senha_hash")]
    public string SenhaHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    [Column("perfil")]
    public string Perfil { get; set; } = string.Empty;

    [Column("id_clube")]
    public int? IdClube { get; set; }

    [ForeignKey(nameof(IdClube))]
    public Clube? Clube { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;

    [Column("data_cadastro")]
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}

[Table("associados")]
public class Associado
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_clube")]
    public int IdClube { get; set; }

    [ForeignKey(nameof(IdClube))]
    public Clube? Clube { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(14)]
    [Column("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("endereco")]
    public string? Endereco { get; set; }

    [MaxLength(100)]
    [Column("bairro")]
    public string? Bairro { get; set; }

    [MaxLength(100)]
    [Column("cidade")]
    public string? Cidade { get; set; }

    [MaxLength(2)]
    [Column("uf")]
    public string? Uf { get; set; }

    [MaxLength(30)]
    [Column("sexo")]
    public string? Sexo { get; set; }

    [MaxLength(30)]
    [Column("estado_civil")]
    public string? EstadoCivil { get; set; }

    [MaxLength(20)]
    [Column("telefone")]
    public string? Telefone { get; set; }

    [MaxLength(200)]
    [Column("email")]
    public string? Email { get; set; }

    [Column("data_nascimento")]
    public DateTime? DataNascimento { get; set; }

    [MaxLength(30)]
    [Column("tipo_associado")]
    public string TipoAssociado { get; set; } = "ASSOCIADO";

    [MaxLength(30)]
    [Column("situacao_cadastro")]
    public string SituacaoCadastro { get; set; } = "PENDENTE";

    [Column("data_cadastro")]
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [Column("id_usuario")]
    public int? IdUsuario { get; set; }

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }

    public ICollection<Dependente> Dependentes { get; set; } = new List<Dependente>();
    public ICollection<InteresseAssociado> Interesses { get; set; } = new List<InteresseAssociado>();
}

[Table("dependentes")]
public class Dependente
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_associado")]
    public int IdAssociado { get; set; }

    [ForeignKey(nameof(IdAssociado))]
    public Associado? Associado { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(14)]
    [Column("cpf")]
    public string? Cpf { get; set; }

    [Column("data_nascimento")]
    public DateTime? DataNascimento { get; set; }

    [MaxLength(50)]
    [Column("parentesco")]
    public string? Parentesco { get; set; }

    [MaxLength(20)]
    [Column("telefone")]
    public string? Telefone { get; set; }

    [MaxLength(200)]
    [Column("email")]
    public string? Email { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}

[Table("interesses_associado")]
public class InteresseAssociado
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_associado")]
    public int IdAssociado { get; set; }

    [ForeignKey(nameof(IdAssociado))]
    public Associado? Associado { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("interesse")]
    public string Interesse { get; set; } = string.Empty;
}

[Table("eventos")]
public class Evento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_clube")]
    public int IdClube { get; set; }

    [ForeignKey(nameof(IdClube))]
    public Clube? Clube { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("data_inicio")]
    public DateTime DataInicio { get; set; }

    [Column("data_fim")]
    public DateTime? DataFim { get; set; }

    [MaxLength(30)]
    [Column("publico_alvo")]
    public string PublicoAlvo { get; set; } = "ASSOCIADOS";

    [MaxLength(200)]
    [Column("local")]
    public string? Local { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}

[Table("mensalidades")]
public class Mensalidade
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_clube")]
    public int IdClube { get; set; }

    [ForeignKey(nameof(IdClube))]
    public Clube? Clube { get; set; }

    [Required]
    [Column("id_associado")]
    public int IdAssociado { get; set; }

    [ForeignKey(nameof(IdAssociado))]
    public Associado? Associado { get; set; }

    [Required]
    [MaxLength(7)]
    [Column("referencia_mes")]
    public string ReferenciaMes { get; set; } = string.Empty; // ex: "2026-11"

    [Column("valor", TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    [Column("data_vencimento")]
    public DateTime DataVencimento { get; set; }

    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    [MaxLength(20)]
    [Column("situacao")]
    public string Situacao { get; set; } = "ABERTA";

    [Column("observacao")]
    public string? Observacao { get; set; }
}

[Table("espacos")]
public class Espaco
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_clube")]
    public int IdClube { get; set; }

    [ForeignKey(nameof(IdClube))]
    public Clube? Clube { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("capacidade")]
    public int? Capacidade { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}

[Table("reservas")]
public class Reserva
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_clube")]
    public int IdClube { get; set; }

    [ForeignKey(nameof(IdClube))]
    public Clube? Clube { get; set; }

    [Required]
    [Column("id_espaco")]
    public int IdEspaco { get; set; }

    [ForeignKey(nameof(IdEspaco))]
    public Espaco? Espaco { get; set; }

    [Required]
    [Column("id_associado")]
    public int IdAssociado { get; set; }

    [ForeignKey(nameof(IdAssociado))]
    public Associado? Associado { get; set; }

    [MaxLength(200)]
    [Column("titulo")]
    public string? Titulo { get; set; }

    [Column("data_hora_inicio")]
    public DateTime DataHoraInicio { get; set; }

    [Column("data_hora_fim")]
    public DateTime DataHoraFim { get; set; }

    [MaxLength(200)]
    [Column("finalidade")]
    public string? Finalidade { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "PENDENTE";

    [Column("observacao")]
    public string? Observacao { get; set; }
}

[Table("solicitacoes")]
public class Solicitacao
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_clube")]
    public int IdClube { get; set; }

    [ForeignKey(nameof(IdClube))]
    public Clube? Clube { get; set; }

    [Column("id_associado")]
    public int? IdAssociado { get; set; }

    [ForeignKey(nameof(IdAssociado))]
    public Associado? Associado { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Column("data_abertura")]
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ABERTA";

    [Column("resposta")]
    public string? Resposta { get; set; }
}

[Table("reclamacoes")]
public class Reclamacao
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_clube")]
    public int IdClube { get; set; }

    [ForeignKey(nameof(IdClube))]
    public Clube? Clube { get; set; }

    [Column("id_associado")]
    public int? IdAssociado { get; set; }

    [ForeignKey(nameof(IdAssociado))]
    public Associado? Associado { get; set; }

    [MaxLength(200)]
    [Column("assunto")]
    public string? Assunto { get; set; }

    [Required]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Column("data_abertura")]
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ABERTA";

    [Column("resposta")]
    public string? Resposta { get; set; }
}
