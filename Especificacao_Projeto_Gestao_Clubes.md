# ESPECIFICAÇÃO DO PROJETO — SISTEMA DE GESTÃO DE CLUBES

**Tipo:** Trabalho acadêmico  
**Objetivo:** Plataforma integrada para administração de clubes, associados, dependentes, eventos, mensalidades, solicitações, reclamações e reservas de espaços.

> Esta especificação foi elaborada com base no rascunho do Projeto Clubes e utiliza como referência a organização técnica e a arquitetura descritas na especificação do VanderPan. O objetivo do sistema é diferente, mas a estrutura tecnológica prevista permanece a mesma.

---

## 1. OBJETIVO DO PROJETO

Desenvolver um sistema integrado para administrar um conjunto de clubes, centralizando cadastros, associados, dependentes, eventos, mensalidades, solicitações e reservas de espaços. A plataforma deverá permitir que cada clube gerencie suas próprias informações, enquanto um perfil de superadministrador mantém a visão e a administração geral da rede.

O sistema será composto por três partes:

- **APLICATIVO MOBILE:** destinado aos associados e dependentes, permitindo consultar informações do clube, acompanhar mensalidades, solicitar associação, enviar solicitações e reclamações e reservar espaços.
- **SITE WEB:** destinado à administração de cada clube, incluindo presidente, administradores e tesoureiro, para gerenciar associados, dependentes, eventos, mensalidades, solicitações, reclamações e agenda de espaços.
- **API RESTful central:** responsável pela autenticação, regras de negócio, comunicação entre aplicativo e site e acesso ao banco de dados.

Também haverá um perfil de **SUPERADMINISTRADOR**, responsável por gerenciar os clubes cadastrados na plataforma e controlar a criação ou liberação dos administradores de cada unidade.

O projeto é acadêmico. As regras que ainda não foram definidas no rascunho estão identificadas como pontos a definir, evitando assumir decisões de negócio sem validação.

## 2. INFRAESTRUTURA E ARQUITETURA

A arquitetura seguirá o mesmo padrão definido para o VanderPan: aplicativo mobile e painel web consumindo uma API central, que concentra a lógica de negócio e acessa o banco MySQL.

| Camada | Responsabilidade | Tecnologia prevista |
|---|---|---|
| Aplicativo mobile | Interface de associados e dependentes; consulta e envio de dados pela API. | C# / .NET MAUI |
| API / Backend | Autenticação, regras de negócio, permissões, cadastros e integração com o banco. | C# / ASP.NET Core Web API |
| Banco de dados | Armazenamento centralizado dos dados de todos os clubes. | MySQL |
| Site web | Painel administrativo dos clubes e painel do superadministrador. | Frontend web consumindo a API REST; tecnologia a definir |

**Fluxo geral de comunicação:**

- Aplicativo .NET MAUI → HTTPS → API ASP.NET Core → MySQL.
- Site administrativo → HTTPS → API ASP.NET Core → MySQL.

O banco será centralizado no servidor. O aplicativo não terá banco local como fonte oficial dos dados; as informações serão consultadas e enviadas à API.

## 3. FERRAMENTAS E TECNOLOGIAS

### 3.1. Aplicativo Mobile

- Linguagem/framework: C# com .NET MAUI.
- Plataforma alvo: Android.
- Consumo da API REST por `HttpClient`.
- Sem banco de dados local.
- IDE: Visual Studio Code.

### 3.2. API / Web Service

- Linguagem/framework: C# com ASP.NET Core Web API.
- Banco de dados: MySQL.
- Acesso a dados: Entity Framework Core ou Dapper, a definir.
- IDE: Visual Studio Code.

### 3.3. Site Web

- Frontend web consumindo a mesma API REST utilizada pelo aplicativo.
- Tecnologia de interface a definir (por exemplo, Blazor ou HTML/CSS/JavaScript com framework).
- Deverá oferecer painéis com permissões diferentes para administrador do clube, tesoureiro e superadministrador.

### 3.4. Controle de versão e apoio

- Git e GitHub para versionamento.
- Claude pelo Antigravity para apoio à documentação e estrutura inicial.
- Gemini integrado ao Visual Studio Code para auxílio durante o desenvolvimento.

### 3.5. Segurança

- Comunicação entre aplicativo, site e API por HTTPS (TLS).
- Senhas armazenadas como hash seguro; preferencialmente bcrypt ou mecanismo equivalente apropriado para senhas.
- Controle de acesso por perfil e por clube, impedindo que um usuário administrativo consulte dados de outro clube sem autorização.

## 4. ORGANIZAÇÃO DOS CLUBES E PERFIS DE ACESSO

### 4.1. Estrutura multi-clube

A plataforma deverá suportar vários clubes em uma única instalação. Cada clube terá seus próprios associados, dependentes, eventos, mensalidades, solicitações, reclamações e reservas. Os registros operacionais deverão estar vinculados ao clube correspondente.

O rascunho cita como exemplos clubes de Foz do Iguaçu e Guaíra, mas a quantidade, os nomes oficiais e a abrangência da rede ainda deverão ser confirmados.

### 4.2. Perfis de usuário

| Perfil | Responsabilidades previstas |
|---|---|
| Superadministrador | Gerenciar o conjunto de clubes, cadastrar unidades e controlar a liberação dos administradores de cada clube. |
| Presidente do clube | Acessar a área administrativa do próprio clube e liberar ou indicar usuários com função administrativa, conforme permissões definidas. |
| Administrador do clube | Gerenciar os dados e as operações do clube ao qual está vinculado. |
| Tesoureiro | Acessar a área financeira, consultar mensalidades e atualizar situações de pagamento. |
| Associado | Utilizar o aplicativo para consultar dados, mensalidades, eventos, fazer solicitações, reclamações e reservas. |
| Dependente | Poderá utilizar o aplicativo e acessar funcionalidades permitidas pelo vínculo com o associado responsável; permissões específicas a definir. |
| Candidato / simpatizante / convidado | Poderá solicitar ingresso ou cadastro, sujeito à aprovação do clube. |

A separação entre os perfis deverá ser aplicada tanto na interface quanto na API. O usuário somente poderá executar operações autorizadas para seu perfil e clube.

## 5. ESPECIFICAÇÃO DO BANCO DE DADOS (MYSQL)

A estrutura abaixo representa uma proposta inicial derivada do rascunho. Os campos podem ser refinados durante a modelagem física, sem alterar as responsabilidades principais das entidades.

### 5.1. Tabela: `clubes`

- `id` INT, PK, auto_increment
- `nome` VARCHAR(200), NOT NULL
- `sigla` VARCHAR(30)
- `endereco` VARCHAR(200)
- `bairro` VARCHAR(100)
- `cidade` VARCHAR(100)
- `uf` CHAR(2)
- `telefone` VARCHAR(20)
- `email` VARCHAR(200)
- `ativo` TINYINT(1), DEFAULT 1

### 5.2. Tabela: `usuarios`

- `id` INT, PK, auto_increment
- `nome` VARCHAR(200), NOT NULL
- `email` VARCHAR(200), NOT NULL, UNIQUE
- `senha_hash` VARCHAR(255), NOT NULL
- `perfil` VARCHAR(30), NOT NULL (`SUPERADMIN`, `PRESIDENTE`, `ADMIN`, `TESOUREIRO`, `ASSOCIADO` ou `DEPENDENTE`)
- `id_clube` INT, FK → `clubes.id` (nulo para superadministrador, quando aplicável)
- `ativo` TINYINT(1), DEFAULT 1
- `data_cadastro` DATETIME, DEFAULT CURRENT_TIMESTAMP

A tabela de usuários centraliza a autenticação. O vínculo com associado ou dependente poderá ser feito por tabelas específicas ou por relacionamento com as entidades correspondentes.

### 5.3. Tabela: `associados`

- `id` INT, PK, auto_increment
- `id_clube` INT, FK → `clubes.id`, NOT NULL
- `nome` VARCHAR(200), NOT NULL
- `cpf` VARCHAR(14), NOT NULL
- `endereco` VARCHAR(200)
- `bairro` VARCHAR(100)
- `cidade` VARCHAR(100)
- `uf` CHAR(2)
- `sexo` VARCHAR(30) (valores permitidos a definir)
- `estado_civil` VARCHAR(30)
- `telefone` VARCHAR(20)
- `email` VARCHAR(200)
- `data_nascimento` DATE
- `tipo_associado` VARCHAR(30) (`ASSOCIADO_FUNDADOR`, `ASSOCIADO` ou `CONVIDADO`, conforme nomenclatura final)
- `situacao_cadastro` VARCHAR(30) (`PENDENTE`, `APROVADO`, `REJEITADO` ou `INATIVO`)
- `data_cadastro` DATETIME

O rascunho menciona associado/fundador com direito a voto e sócio convidado sem direito a voto. A categoria intermediária e os critérios de cada classificação precisam ser confirmados pelo clube.

### 5.4. Tabela: `dependentes`

- `id` INT, PK, auto_increment
- `id_associado` INT, FK → `associados.id`, NOT NULL
- `nome` VARCHAR(200), NOT NULL
- `cpf` VARCHAR(14), se aplicável
- `data_nascimento` DATE
- `parentesco` VARCHAR(50)
- `telefone` VARCHAR(20)
- `email` VARCHAR(200)
- `ativo` TINYINT(1), DEFAULT 1

Os campos exatos dos dependentes e a possibilidade de cada dependente possuir login próprio deverão ser definidos.

### 5.5. Tabela: `interesses_associado`

- `id` INT, PK, auto_increment
- `id_associado` INT, FK → `associados.id`, NOT NULL
- `interesse` VARCHAR(100), NOT NULL

A entidade permite registrar múltiplos interesses por associado, como pesca, caça, comida e bebida, conforme exemplos do rascunho. A lista de opções poderá ser administrável ou predefinida, a definir.

### 5.6. Tabela: `eventos`

- `id` INT, PK, auto_increment
- `id_clube` INT, FK → `clubes.id`, NOT NULL
- `titulo` VARCHAR(200), NOT NULL
- `descricao` TEXT
- `data_inicio` DATETIME, NOT NULL
- `data_fim` DATETIME
- `publico_alvo` VARCHAR(30) (`ASSOCIADOS` ou `PUBLICO_GERAL`)
- `local` VARCHAR(200)
- `ativo` TINYINT(1), DEFAULT 1

Exemplos de eventos citados: almoço do Dia das Mães, jantar do Dia dos Pais e café da tarde. Inscrição, limite de participantes, cobrança e confirmação de presença não foram especificados e deverão ser definidos.

### 5.7. Tabela: `mensalidades`

- `id` INT, PK, auto_increment
- `id_clube` INT, FK → `clubes.id`, NOT NULL
- `id_associado` INT, FK → `associados.id`, NOT NULL
- `referencia_mes` DATE ou VARCHAR(7), NOT NULL
- `valor` DECIMAL(10,2), NOT NULL
- `data_vencimento` DATE, NOT NULL
- `data_pagamento` DATE
- `situacao` VARCHAR(20) (`ABERTA`, `PAGA`, `VENCIDA` ou `CANCELADA`)
- `observacao` TEXT

O rascunho prevê valor mensal e situação financeira. A periodicidade, regras de reajuste, descontos, multas, formas de pagamento e geração automática das cobranças ainda precisam ser estabelecidas.

### 5.8. Tabela: `espacos`

- `id` INT, PK, auto_increment
- `id_clube` INT, FK → `clubes.id`, NOT NULL
- `nome` VARCHAR(150), NOT NULL
- `descricao` TEXT
- `capacidade` INT
- `ativo` TINYINT(1), DEFAULT 1

A entidade representa espaços como salão social e áreas destinadas a festas, churrascos e feijoadas.

### 5.9. Tabela: `reservas`

- `id` INT, PK, auto_increment
- `id_clube` INT, FK → `clubes.id`, NOT NULL
- `id_espaco` INT, FK → `espacos.id`, NOT NULL
- `id_associado` INT, FK → `associados.id`, NOT NULL
- `titulo` VARCHAR(200)
- `data_hora_inicio` DATETIME, NOT NULL
- `data_hora_fim` DATETIME, NOT NULL
- `finalidade` VARCHAR(200)
- `status` VARCHAR(20) (`PENDENTE`, `APROVADA`, `REJEITADA` ou `CANCELADA`)
- `observacao` TEXT

O sistema deverá impedir reservas conflitantes para o mesmo espaço e período. Regras de antecedência, duração máxima, cobrança, caução e aprovação ainda precisam ser definidas.

### 5.10. Tabela: `solicitacoes`

- `id` INT, PK, auto_increment
- `id_clube` INT, FK → `clubes.id`, NOT NULL
- `id_associado` INT, FK → `associados.id`
- `tipo` VARCHAR(50), NOT NULL
- `descricao` TEXT, NOT NULL
- `data_abertura` DATETIME
- `status` VARCHAR(20) (`ABERTA`, `EM_ANALISE`, `RESPONDIDA` ou `ENCERRADA`)
- `resposta` TEXT

A entidade poderá registrar solicitações gerais enviadas pelo aplicativo. Tipos, prazos e responsáveis pelo atendimento serão definidos pelo clube.

### 5.11. Tabela: `reclamacoes`

- `id` INT, PK, auto_increment
- `id_clube` INT, FK → `clubes.id`, NOT NULL
- `id_associado` INT, FK → `associados.id`
- `assunto` VARCHAR(200)
- `descricao` TEXT, NOT NULL
- `data_abertura` DATETIME
- `status` VARCHAR(20) (`ABERTA`, `EM_ANALISE`, `RESPONDIDA` ou `ENCERRADA`)
- `resposta` TEXT

O acesso às reclamações deverá ser restrito aos perfis autorizados. A possibilidade de envio anônimo não foi definida no rascunho.

### 5.12. Relacionamentos principais

- Um clube possui muitos associados, eventos, mensalidades, espaços, reservas, solicitações e reclamações.
- Um associado pertence a um clube e pode possuir vários dependentes e interesses.
- Um espaço pode possuir várias reservas, desde que não haja conflito de horário.
- Uma reserva pertence a um clube, a um espaço e ao associado solicitante.
- Um associado pode possuir várias mensalidades, uma para cada período de cobrança.

## 6. REQUISITOS FUNCIONAIS

### 6.1. Aplicativo Mobile

- **RF01 – Cadastro e solicitação de associação:** O interessado poderá preencher seus dados e enviar uma solicitação de ingresso ao clube. O cadastro ficará pendente até a análise e aprovação por usuário autorizado.
- **RF02 – Login:** O usuário aprovado poderá autenticar-se no aplicativo com suas credenciais. A sessão poderá permanecer salva no dispositivo, conforme política de segurança a definir.
- **RF03 – Identificação do clube:** O aplicativo deverá apresentar os dados e as informações do clube ao qual o usuário está vinculado.
- **RF04 – Consulta de perfil:** O associado poderá consultar seus dados cadastrais e, quando permitido, os dados de seus dependentes.
- **RF05 – Consulta de mensalidades:** O associado poderá consultar mensalidades, valores, vencimentos e situação financeira.
- **RF06 – Consulta de eventos:** O aplicativo exibirá eventos do clube, suas datas, descrições e público destinado.
- **RF07 – Solicitações:** O associado poderá registrar solicitações e acompanhar seu status e eventuais respostas.
- **RF08 – Reclamações:** O associado poderá enviar reclamações e acompanhar seu andamento, respeitando as permissões de acesso.
- **RF09 – Reserva de espaço:** O associado poderá solicitar a reserva de um espaço, informando data e horário de início, data e horário de término e finalidade.
- **RF10 – Acompanhamento de reservas:** O associado poderá consultar o status das reservas solicitadas.
- **RF11 – Dependentes:** O aplicativo deverá permitir visualizar os dependentes vinculados ao associado. O acesso individual dos dependentes dependerá da regra de autenticação definida.

### 6.2. Site Web – Administração do Clube

- **RF12 – Login administrativo:** Presidente, administrador e tesoureiro acessarão o painel conforme suas permissões.
- **RF13 – Gestão de associados:** Cadastrar, consultar, editar, aprovar, rejeitar e desativar associados do clube.
- **RF14 – Gestão de dependentes:** Consultar e administrar os dependentes vinculados aos associados.
- **RF15 – Gestão de interesses:** Consultar e manter os interesses registrados pelos associados, conforme regras definidas.
- **RF16 – Gestão de eventos:** Cadastrar, editar, consultar e desativar eventos, definindo período, descrição e público-alvo.
- **RF17 – Gestão de mensalidades:** Definir ou registrar mensalidades e consultar a situação financeira dos associados.
- **RF18 – Painel do tesoureiro:** Disponibilizar ao tesoureiro as funções financeiras autorizadas, incluindo consulta e atualização da situação de mensalidades.
- **RF19 – Gestão de espaços:** Cadastrar e editar os espaços disponíveis para reserva, como salão social e áreas de convivência.
- **RF20 – Agenda de reservas:** Exibir as reservas em formato de agenda, com período, espaço, solicitante e status.
- **RF21 – Aprovação de reservas:** Permitir que usuários autorizados aprovem, rejeitem ou cancelem solicitações de reserva.
- **RF22 – Gestão de solicitações:** Listar solicitações recebidas, permitir análise, resposta e alteração de status.
- **RF23 – Gestão de reclamações:** Listar reclamações, restringir seu acesso e registrar respostas e andamento.
- **RF24 – Gestão de administradores:** O presidente poderá liberar ou indicar administradores do próprio clube, respeitando as permissões concedidas pelo superadministrador.

### 6.3. Painel do Superadministrador

- **RF25 – Gestão de clubes:** Cadastrar, consultar, editar, ativar e desativar clubes da plataforma.
- **RF26 – Gestão de acesso administrativo:** Controlar a liberação do acesso inicial dos presidentes ou administradores de cada clube.
- **RF27 – Visão geral:** Consultar a relação de clubes cadastrados e suas informações básicas.

### 6.4. Funcionalidades comuns

- **RF28 – Isolamento de dados:** Todas as operações deverão respeitar o clube vinculado ao usuário. Dados de um clube não poderão ser exibidos a usuários de outro clube sem autorização explícita.
- **RF29 – Histórico de operações:** Alterações relevantes de cadastro, aprovação, mensalidade e reserva deverão manter informações suficientes para rastreabilidade. O nível de auditoria será definido durante o desenvolvimento.

## 7. REQUISITOS NÃO FUNCIONAIS

- **RNF01 – Segurança:** Comunicação por HTTPS/TLS, senhas armazenadas com hash seguro e autenticação com autorização por perfil e clube.
- **RNF02 – Desempenho:** A API deverá responder em tempo aceitável para consultas e operações comuns. Meta inicial sugerida: até 2 segundos em operações usuais, sujeita a validação no ambiente de hospedagem.
- **RNF03 – Portabilidade:** O aplicativo deverá funcionar em Android por meio do .NET MAUI; o painel web deverá funcionar nos principais navegadores modernos.
- **RNF04 – Manutenibilidade:** O código deverá ser organizado por responsabilidades e versionado com Git/GitHub.
- **RNF05 – Disponibilidade:** A API e o banco MySQL deverão estar hospedados em ambiente acessível aos usuários autorizados.
- **RNF06 – Conectividade:** O aplicativo dependerá de conexão com a internet para consultar e enviar informações à API; modo offline não está previsto nesta especificação inicial.
- **RNF07 – Privacidade:** Dados pessoais dos associados e dependentes deverão ser tratados conforme a LGPD, com acesso limitado às finalidades e permissões necessárias.

## 8. REGRAS DE NEGÓCIO

- **RN01 – Separação por clube:** Cada associado, evento, mensalidade, espaço, reserva, solicitação e reclamação deverá estar vinculado a um clube.
- **RN02 – Acesso do superadministrador:** O superadministrador poderá gerenciar todos os clubes cadastrados na plataforma.
- **RN03 – Acesso administrativo do clube:** O presidente e os administradores atuarão somente no clube ao qual estão vinculados, salvo permissão superior explicitamente concedida.
- **RN04 – Aprovação de cadastro:** Solicitações de ingresso não se tornam automaticamente cadastros ativos. A aprovação deverá ser realizada por usuário autorizado.
- **RN05 – Categorias de associado:** O rascunho diferencia associado/fundador, com direito a voto, e sócio convidado, sem direito a voto. A categoria e as condições de simpatizante precisam ser formalizadas pelo clube.
- **RN06 – Dependentes:** Dependentes deverão permanecer vinculados a um associado responsável. As permissões e os direitos próprios dos dependentes ainda serão definidos.
- **RN07 – Mensalidades:** A situação financeira deverá ser consultável pelo associado e pelos perfis administrativos autorizados. Regras de vencimento, inadimplência e cálculo de encargos ainda serão definidas.
- **RN08 – Público dos eventos:** Cada evento deverá indicar se é destinado somente aos associados ou se é aberto ao público geral.
- **RN09 – Reserva de espaço:** A reserva deverá conter data e hora de início e de término. Não poderá haver duas reservas aprovadas conflitantes para o mesmo espaço.
- **RN10 – Aprovação de reserva:** A solicitação de reserva deverá possuir status e poderá depender de aprovação administrativa. O fluxo final de aprovação deverá ser confirmado.
- **RN11 – Solicitações e reclamações:** Cada registro deverá possuir responsável, data de abertura e status de acompanhamento, com acesso restrito ao clube correspondente.
- **RN12 – Unicidade de cadastro:** O CPF do associado deverá ser validado para evitar cadastros duplicados. Definir se a unicidade será global ou por clube.
- **RN13 – Integridade dos dados:** Registros relacionados a um clube não poderão ser associados indevidamente a outro clube.

## 9. FLUXOS PRINCIPAIS DO SISTEMA

### 9.1. Fluxo – Solicitação e aprovação de associação

1. O interessado abre o aplicativo e seleciona a opção de cadastro ou solicitação de associação.
2. Preenche os dados pessoais solicitados e informa o clube pretendido.
3. O aplicativo envia os dados à API, que registra a solicitação com status pendente.
4. O administrador autorizado acessa o painel web e analisa a solicitação.
5. O administrador aprova ou rejeita o cadastro.
6. Em caso de aprovação, o registro passa a integrar os associados do clube e o usuário poderá receber/liberar acesso ao aplicativo conforme o processo de autenticação definido.

### 9.2. Fluxo – Consulta de mensalidades

1. O associado acessa a área financeira no aplicativo.
2. O aplicativo solicita à API as mensalidades vinculadas ao associado autenticado.
3. A API valida a identidade, o vínculo com o clube e as permissões.
4. O sistema apresenta valores, períodos, vencimentos e situação de cada mensalidade.

### 9.3. Fluxo – Cadastro e publicação de evento

1. O administrador acessa a área de eventos no site.
2. Preenche título, descrição, período, local e público-alvo.
3. A API valida os dados e grava o evento vinculado ao clube.
4. O evento passa a ser exibido aos usuários do público definido.

### 9.4. Fluxo – Solicitação de reserva

1. O associado seleciona o espaço desejado no aplicativo.
2. Informa data e hora de início, data e hora de término e finalidade.
3. A API verifica a disponibilidade do espaço no período solicitado.
4. Se houver conflito, o sistema informa que o período não está disponível.
5. Se não houver conflito, a solicitação é registrada com status pendente, caso dependa de aprovação.
6. O administrador consulta a agenda no site e aprova ou rejeita a solicitação.
7. O aplicativo apresenta ao associado o status atualizado.

### 9.5. Fluxo – Atendimento de solicitação ou reclamação

1. O associado registra uma solicitação ou reclamação pelo aplicativo.
2. A API salva o registro no clube correspondente e define o status inicial.
3. O administrador autorizado consulta o painel de atendimento.
4. O responsável registra uma resposta ou atualiza o status.
5. O associado acompanha o andamento pelo aplicativo.

## 10. TELAS E PÁGINAS PREVISTAS

### 10.1. Aplicativo Mobile – Associado

1. Login / cadastro / solicitação de associação.
2. Início – identificação do clube e atalhos para as funções principais.
3. Meu Perfil – dados cadastrais do associado.
4. Meus Dependentes – consulta dos dependentes vinculados.
5. Mensalidades – valores, vencimentos e situação financeira.
6. Eventos – lista e detalhes dos eventos do clube.
7. Solicitações – criação e acompanhamento.
8. Reclamações – envio e acompanhamento.
9. Reservar Espaço – seleção de espaço e período.
10. Minhas Reservas – histórico e status das solicitações.

### 10.2. Site Web – Administração do Clube

1. Login administrativo.
2. Dashboard do clube.
3. Associados – listagem, cadastro, edição e aprovação.
4. Dependentes – consulta e manutenção.
5. Eventos – CRUD e definição do público-alvo.
6. Mensalidades – configuração/registro e consulta financeira.
7. Área do Tesoureiro – acompanhamento da situação financeira.
8. Espaços – cadastro e manutenção.
9. Agenda de Reservas – visualização e aprovação.
10. Solicitações – fila de atendimento.
11. Reclamações – fila de atendimento e respostas.
12. Administradores – gerenciamento dos acessos permitidos ao clube.

### 10.3. Site Web – Superadministrador

1. Login do superadministrador.
2. Dashboard geral.
3. Cadastro e manutenção dos clubes.
4. Liberação e gerenciamento dos acessos iniciais dos clubes.

## 11. API REST – ENDPOINTS PRINCIPAIS (PROPOSTA)

Os caminhos abaixo são uma proposta inicial para organização da API. Os contratos, DTOs, códigos de retorno e políticas de autenticação serão definidos durante a implementação.

| Grupo | Método e endpoint | Finalidade |
|---|---|---|
| Autenticação | `POST /api/auth/login` | Autenticar usuário e retornar credencial de acesso. |
| Clubes | `GET /api/clubes` | Listar clubes conforme permissão. |
| Clubes | `POST /api/clubes` | Cadastrar clube (superadministrador). |
| Clubes | `PUT /api/clubes/{id}` | Editar clube. |
| Clubes | `PATCH /api/clubes/{id}/status` | Ativar ou desativar clube. |
| Associados | `GET /api/associados` | Listar associados do clube autorizado. |
| Associados | `POST /api/associados/solicitacoes` | Enviar solicitação de associação. |
| Associados | `PATCH /api/associados/{id}/aprovar` | Aprovar solicitação. |
| Associados | `PATCH /api/associados/{id}/rejeitar` | Rejeitar solicitação. |
| Associados | `PUT /api/associados/{id}` | Editar associado. |
| Dependentes | `GET /api/associados/{id}/dependentes` | Listar dependentes. |
| Dependentes | `POST /api/associados/{id}/dependentes` | Cadastrar dependente. |
| Eventos | `GET /api/eventos` | Listar eventos visíveis ao usuário. |
| Eventos | `POST /api/eventos` | Cadastrar evento. |
| Eventos | `PUT /api/eventos/{id}` | Editar evento. |
| Mensalidades | `GET /api/mensalidades` | Consultar mensalidades. |
| Mensalidades | `POST /api/mensalidades` | Registrar mensalidade/cobrança. |
| Mensalidades | `PATCH /api/mensalidades/{id}/situacao` | Atualizar situação financeira. |
| Espaços | `GET /api/espacos` | Listar espaços disponíveis. |
| Espaços | `POST /api/espacos` | Cadastrar espaço. |
| Reservas | `GET /api/reservas` | Consultar reservas e agenda. |
| Reservas | `POST /api/reservas` | Solicitar reserva. |
| Reservas | `PATCH /api/reservas/{id}/aprovar` | Aprovar reserva. |
| Reservas | `PATCH /api/reservas/{id}/rejeitar` | Rejeitar reserva. |
| Solicitações | `GET /api/solicitacoes` | Listar solicitações do clube. |
| Solicitações | `POST /api/solicitacoes` | Registrar solicitação. |
| Reclamações | `GET /api/reclamacoes` | Listar reclamações autorizadas. |
| Reclamações | `POST /api/reclamacoes` | Registrar reclamação. |

**Observação:** todos os endpoints deverão validar o token, o perfil do usuário e o identificador do clube. O identificador do clube não deverá ser aceito cegamente do aplicativo quando puder ser obtido com segurança a partir da sessão autenticada.

## 12. SEGURANÇA E PROTEÇÃO DE DADOS

- Toda comunicação entre aplicativo/site e API deverá utilizar HTTPS (TLS).
- Senhas nunca deverão ser armazenadas em texto puro; utilizar algoritmo próprio para hash de senhas, como bcrypt ou Argon2.
- A API deverá validar a autorização em cada operação, inclusive o vínculo do usuário com o clube.
- Dados pessoais como CPF, endereço, telefone, e-mail e data de nascimento deverão ser tratados conforme a LGPD.
- Dados financeiros e reclamações deverão ter acesso limitado aos perfis que necessitam dessas informações.
- A aplicação deverá evitar exposição de dados de um clube a usuários de outro clube.

## 13. PONTOS A DEFINIR ANTES DA IMPLEMENTAÇÃO

O rascunho apresenta a finalidade geral e as principais funções, mas alguns critérios ainda não estão especificados. Eles deverão ser decididos com os representantes dos clubes antes de fechar o modelo de dados e as telas.

- Nome oficial do sistema e lista/quantidade de clubes participantes.
- Se cada clube terá uma base de dados lógica compartilhada ou se haverá outra estratégia de separação. Esta especificação considera um MySQL central com identificação do clube nos registros.
- Quais categorias de associado existirão exatamente e quais direitos cada categoria possui, incluindo direito a voto.
- Como funcionará o cadastro de simpatizantes e convidados e se eles terão acesso ao aplicativo antes da aprovação.
- Quais dados são obrigatórios no cadastro de associado e de dependente; se CPF será obrigatório para dependentes.
- Se dependentes terão login próprio e quais funcionalidades poderão utilizar.
- Como serão definidos os valores, vencimentos, reajustes, multas, juros, descontos e formas de pagamento das mensalidades.
- Se o sistema apenas registrará pagamentos ou também integrará com algum meio de pagamento.
- Se eventos exigirão inscrição, confirmação de presença, limite de vagas ou pagamento.
- Quais espaços poderão ser reservados, capacidade, regras de uso, antecedência mínima, duração máxima, cobrança e caução.
- Quem aprova reservas e se determinadas reservas poderão ser aprovadas automaticamente.
- Se solicitações e reclamações poderão ser anônimas, quais categorias existirão e quais prazos de resposta serão aplicados.
- Quais permissões exatas terão presidente, administrador e tesoureiro.
- Se haverá notificações push, e-mail ou outro canal para aprovações, vencimentos e alterações de reservas.
- Política de recuperação de senha, expiração de sessão e autenticação dos usuários.

## 14. DOCUMENTAÇÃO E CONTROLE DO DESENVOLVIMENTO

- O desenvolvimento será versionado com Git/GitHub.
- A documentação deverá registrar arquitetura, modelo de dados, endpoints, regras de negócio e decisões tomadas durante a implementação.
- As ferramentas de IA poderão auxiliar na estruturação, documentação e revisão do código, mas as decisões e validações do projeto deverão ser conferidas pela equipe.
- O projeto será desenvolvido como trabalho acadêmico, mantendo separação entre aplicativo, API, banco de dados e painel web.

---

**FIM DA ESPECIFICAÇÃO**
