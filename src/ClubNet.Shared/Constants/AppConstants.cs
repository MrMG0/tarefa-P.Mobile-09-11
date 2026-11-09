namespace ClubNet.Shared.Constants;

public static class Perfis
{
    public const string SuperAdmin = "SUPERADMIN";
    public const string Presidente = "PRESIDENTE";
    public const string Admin = "ADMIN";
    public const string Tesoureiro = "TESOUREIRO";
    public const string Associado = "ASSOCIADO";
    public const string Dependente = "DEPENDENTE";

    public static readonly string[] Todos = [SuperAdmin, Presidente, Admin, Tesoureiro, Associado, Dependente];
    public static readonly string[] Administrativos = [SuperAdmin, Presidente, Admin, Tesoureiro];
    public static readonly string[] GestaoClube = [SuperAdmin, Presidente, Admin];
}

public static class SituacaoCadastro
{
    public const string Pendente = "PENDENTE";
    public const string Aprovado = "APROVADO";
    public const string Rejeitado = "REJEITADO";
    public const string Inativo = "INATIVO";
}

public static class TiposAssociado
{
    public const string AssociadoFundador = "ASSOCIADO_FUNDADOR";
    public const string Associado = "ASSOCIADO";
    public const string Convidado = "CONVIDADO";
}

public static class SituacaoMensalidade
{
    public const string Aberta = "ABERTA";
    public const string Paga = "PAGA";
    public const string Vencida = "VENCIDA";
    public const string Cancelada = "CANCELADA";
}

public static class StatusReserva
{
    public const string Pendente = "PENDENTE";
    public const string Aprovada = "APROVADA";
    public const string Rejeitada = "REJEITADA";
    public const string Cancelada = "CANCELADA";
}

public static class StatusAtendimento
{
    public const string Aberta = "ABERTA";
    public const string EmAnalise = "EM_ANALISE";
    public const string Respondida = "RESPONDIDA";
    public const string Encerrada = "ENCERRADA";
}

public static class PublicoAlvoEvento
{
    public const string Associados = "ASSOCIADOS";
    public const string PublicoGeral = "PUBLICO_GERAL";
}
