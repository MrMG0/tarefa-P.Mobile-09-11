using ClubNet.API.Security;
using ClubNet.Shared.Constants;
using ClubNet.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubNet.API.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ClubNetDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Clubes.AnyAsync())
        {
            return; // Banco já populado
        }

        // 1. Clubes
        var clubeCascavel = new Clube
        {
            Nome = "Clube Social Cascavel",
            Sigla = "CSC",
            Endereco = "Av. Brasil, 4500",
            Bairro = "Centro",
            Cidade = "Cascavel",
            Uf = "PR",
            Telefone = "(45) 3224-1000",
            Email = "contato@clubesocialcascavel.com.br",
            Ativo = true
        };

        var clubeFoz = new Clube
        {
            Nome = "Iate Clube Foz do Iguaçu",
            Sigla = "ICFI",
            Endereco = "Av. das Cataratas, 1200",
            Bairro = "Vila Yolanda",
            Cidade = "Foz do Iguaçu",
            Uf = "PR",
            Telefone = "(45) 3523-2000",
            Email = "atendimento@iateclubefoz.com.br",
            Ativo = true
        };

        var clubeGuaira = new Clube
        {
            Nome = "Clube Náutico Guaíra",
            Sigla = "CNG",
            Endereco = "Rua Beira Rio, 350",
            Bairro = "Porto Velho",
            Cidade = "Guaíra",
            Uf = "PR",
            Telefone = "(44) 3642-3000",
            Email = "secretaria@nauticoguaira.com.br",
            Ativo = true
        };

        db.Clubes.AddRange(clubeCascavel, clubeFoz, clubeGuaira);
        await db.SaveChangesAsync();

        // 2. Usuários Administrativos
        var senhaPadraoHash = PasswordHasher.HashPassword("Admin@123");
        var senhaUserHash = PasswordHasher.HashPassword("User@123");

        var superAdmin = new Usuario
        {
            Nome = "Super Administrador Geral",
            Email = "superadmin@clubnet.com",
            SenhaHash = senhaPadraoHash,
            Perfil = Perfis.SuperAdmin,
            IdClube = null,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        var presidenteCascavel = new Usuario
        {
            Nome = "Roberto Menezes (Presidente)",
            Email = "presidente.cascavel@clubnet.com",
            SenhaHash = senhaPadraoHash,
            Perfil = Perfis.Presidente,
            IdClube = clubeCascavel.Id,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        var adminCascavel = new Usuario
        {
            Nome = "Fernanda Souza (Administradora)",
            Email = "admin.cascavel@clubnet.com",
            SenhaHash = senhaPadraoHash,
            Perfil = Perfis.Admin,
            IdClube = clubeCascavel.Id,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        var tesoureiroCascavel = new Usuario
        {
            Nome = "Paulo Ricardo (Tesoureiro)",
            Email = "tesoureiro.cascavel@clubnet.com",
            SenhaHash = senhaPadraoHash,
            Perfil = Perfis.Tesoureiro,
            IdClube = clubeCascavel.Id,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        // Usuários Associados
        var userCarlos = new Usuario
        {
            Nome = "Carlos Eduardo Silva",
            Email = "carlos.silva@email.com",
            SenhaHash = senhaUserHash,
            Perfil = Perfis.Associado,
            IdClube = clubeCascavel.Id,
            Ativo = true,
            DataCadastro = DateTime.UtcNow.AddMonths(-6)
        };

        var userMarcos = new Usuario
        {
            Nome = "Marcos Vinicius de Oliveira",
            Email = "marcos.oliveira@email.com",
            SenhaHash = senhaUserHash,
            Perfil = Perfis.Associado,
            IdClube = clubeCascavel.Id,
            Ativo = false, // pendente
            DataCadastro = DateTime.UtcNow.AddDays(-2)
        };

        db.Usuarios.AddRange(superAdmin, presidenteCascavel, adminCascavel, tesoureiroCascavel, userCarlos, userMarcos);
        await db.SaveChangesAsync();

        // 3. Associados
        var associadoCarlos = new Associado
        {
            IdClube = clubeCascavel.Id,
            IdUsuario = userCarlos.Id,
            Nome = "Carlos Eduardo Silva",
            Cpf = "123.456.789-00",
            Endereco = "Rua Pará, 150",
            Bairro = "Country",
            Cidade = "Cascavel",
            Uf = "PR",
            Sexo = "Masculino",
            EstadoCivil = "Casado",
            Telefone = "(45) 99988-1122",
            Email = "carlos.silva@email.com",
            DataNascimento = new DateTime(1985, 4, 15),
            TipoAssociado = TiposAssociado.AssociadoFundador,
            SituacaoCadastro = SituacaoCadastro.Aprovado,
            DataCadastro = DateTime.UtcNow.AddMonths(-6)
        };

        var associadoMarcos = new Associado
        {
            IdClube = clubeCascavel.Id,
            IdUsuario = userMarcos.Id,
            Nome = "Marcos Vinicius de Oliveira",
            Cpf = "987.654.321-99",
            Endereco = "Av. Toledo, 890",
            Bairro = "Coqueiral",
            Cidade = "Cascavel",
            Uf = "PR",
            Sexo = "Masculino",
            EstadoCivil = "Solteiro",
            Telefone = "(45) 98877-3344",
            Email = "marcos.oliveira@email.com",
            DataNascimento = new DateTime(1992, 9, 20),
            TipoAssociado = TiposAssociado.Associado,
            SituacaoCadastro = SituacaoCadastro.Pendente,
            DataCadastro = DateTime.UtcNow.AddDays(-2)
        };

        db.Associados.AddRange(associadoCarlos, associadoMarcos);
        await db.SaveChangesAsync();

        // 4. Dependentes de Carlos
        var depMariana = new Dependente
        {
            IdAssociado = associadoCarlos.Id,
            Nome = "Mariana Silva",
            Cpf = "234.567.890-11",
            DataNascimento = new DateTime(1987, 8, 12),
            Parentesco = "Cônjuge",
            Telefone = "(45) 99988-1123",
            Email = "mariana.silva@email.com",
            Ativo = true
        };

        var depLucas = new Dependente
        {
            IdAssociado = associadoCarlos.Id,
            Nome = "Lucas Gabriel Silva",
            Cpf = "345.678.901-22",
            DataNascimento = new DateTime(2014, 2, 28),
            Parentesco = "Filho",
            Ativo = true
        };

        db.Dependentes.AddRange(depMariana, depLucas);

        // 5. Interesses
        db.InteressesAssociado.AddRange(
            new InteresseAssociado { IdAssociado = associadoCarlos.Id, Interesse = "Pesca Esportiva" },
            new InteresseAssociado { IdAssociado = associadoCarlos.Id, Interesse = "Churrasco e Confraternização" },
            new InteresseAssociado { IdAssociado = associadoCarlos.Id, Interesse = "Natação" },
            new InteresseAssociado { IdAssociado = associadoMarcos.Id, Interesse = "Futebol Suíço" },
            new InteresseAssociado { IdAssociado = associadoMarcos.Id, Interesse = "Música ao Vivo" }
        );

        // 6. Espaços
        var espacoSalao = new Espaco
        {
            IdClube = clubeCascavel.Id,
            Nome = "Salão Social Nobre",
            Descricao = "Salão climatizado com palco, bar e cozinha industrial de apoio.",
            Capacidade = 250,
            Ativo = true
        };

        var espacoChurrasqueira = new Espaco
        {
            IdClube = clubeCascavel.Id,
            Nome = "Quiosque Churrasqueira Master",
            Descricao = "Área coberta próxima às piscinas com churrasqueira dupla e freezer.",
            Capacidade = 40,
            Ativo = true
        };

        var espacoQuadra = new Espaco
        {
            IdClube = clubeCascavel.Id,
            Nome = "Quadra Poliesportiva Coberta",
            Descricao = "Piso vinílico oficial para futsal, basquete e vôlei.",
            Capacidade = 60,
            Ativo = true
        };

        db.Espacos.AddRange(espacoSalao, espacoChurrasqueira, espacoQuadra);
        await db.SaveChangesAsync();

        // 7. Eventos
        db.Eventos.AddRange(
            new Evento
            {
                IdClube = clubeCascavel.Id,
                Titulo = "Almoço do Dia das Mães",
                Descricao = "Tradicional confraternização com buffet especial e música ao vivo para todas as famílias associadas.",
                DataInicio = DateTime.UtcNow.AddDays(7).Date.AddHours(12),
                DataFim = DateTime.UtcNow.AddDays(7).Date.AddHours(16),
                PublicoAlvo = PublicoAlvoEvento.Associados,
                Local = "Salão Social Nobre",
                Ativo = true
            },
            new Evento
            {
                IdClube = clubeCascavel.Id,
                Titulo = "Torneio Aberto de Pesca da Primavera",
                Descricao = "Competição esportiva de pesca de dourado e tucunaré, com premiação e almoço comunitário.",
                DataInicio = DateTime.UtcNow.AddDays(15).Date.AddHours(8),
                DataFim = DateTime.UtcNow.AddDays(15).Date.AddHours(17),
                PublicoAlvo = PublicoAlvoEvento.PublicoGeral,
                Local = "Lago e Trapiche do Clube",
                Ativo = true
            },
            new Evento
            {
                IdClube = clubeFoz.Id,
                Titulo = "Regata Internacional de Foz do Iguaçu",
                Descricao = "Encontro de velejadores e competições náuticas no Lago de Itaipu.",
                DataInicio = DateTime.UtcNow.AddDays(20).Date.AddHours(9),
                DataFim = DateTime.UtcNow.AddDays(20).Date.AddHours(18),
                PublicoAlvo = PublicoAlvoEvento.PublicoGeral,
                Local = "Represa de Itaipu",
                Ativo = true
            }
        );

        // 8. Mensalidades de Carlos
        db.Mensalidades.AddRange(
            new Mensalidade
            {
                IdClube = clubeCascavel.Id,
                IdAssociado = associadoCarlos.Id,
                ReferenciaMes = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM"),
                Valor = 180.00m,
                DataVencimento = DateTime.UtcNow.AddMonths(-1).Date.AddDays(10),
                DataPagamento = DateTime.UtcNow.AddMonths(-1).Date.AddDays(8),
                Situacao = SituacaoMensalidade.Paga,
                Observacao = "Pago via PIX"
            },
            new Mensalidade
            {
                IdClube = clubeCascavel.Id,
                IdAssociado = associadoCarlos.Id,
                ReferenciaMes = DateTime.UtcNow.ToString("yyyy-MM"),
                Valor = 180.00m,
                DataVencimento = DateTime.UtcNow.Date.AddDays(10),
                Situacao = SituacaoMensalidade.Aberta,
                Observacao = "Mensalidade do mês corrente"
            },
            new Mensalidade
            {
                IdClube = clubeCascavel.Id,
                IdAssociado = associadoCarlos.Id,
                ReferenciaMes = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM"),
                Valor = 180.00m,
                DataVencimento = DateTime.UtcNow.AddMonths(1).Date.AddDays(10),
                Situacao = SituacaoMensalidade.Aberta,
                Observacao = "Próxima mensalidade"
            }
        );

        // 9. Reservas
        db.Reservas.AddRange(
            new Reserva
            {
                IdClube = clubeCascavel.Id,
                IdEspaco = espacoChurrasqueira.Id,
                IdAssociado = associadoCarlos.Id,
                Titulo = "Aniversário Mariana",
                DataHoraInicio = DateTime.UtcNow.AddDays(3).Date.AddHours(11),
                DataHoraFim = DateTime.UtcNow.AddDays(3).Date.AddHours(18),
                Finalidade = "Almoço de aniversário em família",
                Status = StatusReserva.Aprovada,
                Observacao = "Reserva aprovada pela gerência."
            },
            new Reserva
            {
                IdClube = clubeCascavel.Id,
                IdEspaco = espacoSalao.Id,
                IdAssociado = associadoCarlos.Id,
                Titulo = "Jantar com Amigos da Pesca",
                DataHoraInicio = DateTime.UtcNow.AddDays(10).Date.AddHours(19),
                DataHoraFim = DateTime.UtcNow.AddDays(10).Date.AddHours(23),
                Finalidade = "Confraternização da equipe de pesca",
                Status = StatusReserva.Pendente,
                Observacao = "Aguardando confirmação da diretoria."
            }
        );

        // 10. Solicitações e Reclamações
        db.Solicitacoes.Add(new Solicitacao
        {
            IdClube = clubeCascavel.Id,
            IdAssociado = associadoCarlos.Id,
            Tipo = "Carteirinha Digital",
            Descricao = "Gostaria de solicitar a emissão de via impressa da carteirinha para meu dependente Lucas.",
            DataAbertura = DateTime.UtcNow.AddDays(-3),
            Status = StatusAtendimento.Respondida,
            Resposta = "Sua carteirinha foi emitida e já está disponível para retirada na secretaria do clube."
        });

        db.Reclamacoes.Add(new Reclamacao
        {
            IdClube = clubeCascavel.Id,
            IdAssociado = associadoCarlos.Id,
            Assunto = "Iluminação no Trapiche Principal",
            Descricao = "Dois refletores do trapiche de embarque estavam apagados na última sexta-feira à noite.",
            DataAbertura = DateTime.UtcNow.AddDays(-1),
            Status = StatusAtendimento.EmAnalise,
            Resposta = "Nossa equipe de manutenção já identificou a lâmpada queimada e fará a troca amanhã."
        });

        await db.SaveChangesAsync();
    }
}
