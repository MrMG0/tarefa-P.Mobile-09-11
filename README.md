# ClubNet — Sistema Integrado de Gestão de Clubes

Sistema integrado para administração de rede de clubes, associados, dependentes, eventos, mensalidades, solicitações, ouvidoria e reservas de espaços com validação de conflitos de agenda.

O projeto foi implementado rigorosamente conforme a especificação técnica em [Especificacao_Projeto_Gestao_Clubes.md](Especificacao_Projeto_Gestao_Clubes.md), dividido nas três camadas na ordem solicitada:

1. **API RESTful Central (`ClubNet.API`)**
2. **Site Web Administrativo (`ClubNet.Web`)**
3. **Aplicativo Mobile do Associado (`ClubNet.App`)**

Adicionalmente, foi criada a biblioteca compartilhada **`ClubNet.Shared`** contendo modelos de domínio, DTOs e constantes, garantindo total consistência tipada entre backend, frontend web e app mobile.

---

## 🏛️ Arquitetura e Tecnologias

| Projeto | Tecnologia | Responsabilidade |
|---|---|---|
| **ClubNet.API** | ASP.NET Core Web API 10, EF Core, JWT, BCrypt | Autenticação, autorização por perfil, regras de negócio multi-clube, validação de conflitos de reservas e endpoints RESTful. Suporte a MySQL 8.0+ e SQLite nativo. |
| **ClubNet.Web** | ASP.NET Core Razor Pages, Bootstrap 5, Bootstrap Icons | Painel administrativo com visões dedicadas para **Superadministrador**, **Presidente**, **Administrador** e **Tesoureiro**. |
| **ClubNet.App** | .NET MAUI (Android / Windows Cross-Platform) | Aplicativo móvel para associados e dependentes: consulta de perfil, dependentes, mensalidades, eventos, solicitação de reservas e atendimento (solicitações/reclamações). |
| **ClubNet.Shared** | .NET Class Library | Entidades de domínio EF Core, DTOs de requisição/resposta, perfis e enumerações. |

---

## 🔑 Credenciais de Acesso Pré-Cadastradas (Seed Inicial)

O banco de dados é inicializado automaticamente com clubes, espaços, eventos, mensalidades e usuários de teste:

| Perfil | E-mail | Senha | Clube Vinculado |
|---|---|---|---|
| **Superadministrador** | `superadmin@clubnet.com` | `Admin@123` | *Acesso Global a todos os clubes* |
| **Presidente** | `presidente.cascavel@clubnet.com` | `Admin@123` | Clube Social Cascavel |
| **Administrador** | `admin.cascavel@clubnet.com` | `Admin@123` | Clube Social Cascavel |
| **Tesoureiro** | `tesoureiro.cascavel@clubnet.com` | `Admin@123` | Clube Social Cascavel |
| **Associado (Ativo)** | `carlos.silva@email.com` | `User@123` | Clube Social Cascavel |
| **Associado (Pendente)** | `marcos.oliveira@email.com` | `User@123` | Clube Social Cascavel |

---

## 🚀 Como Executar o Projeto

### 1. Iniciar a API REST
Abra um terminal na raiz do projeto e execute:
```bash
dotnet run --project src/ClubNet.API/ClubNet.API.csproj --urls http://localhost:5000
```
- A API iniciará na porta `5000` (`http://localhost:5000`).
- O banco de dados SQLite (`clubnet.db`) é criado e populado automaticamente na primeira execução com dados de clubes de Cascavel, Foz do Iguaçu e Guaíra.
- Se desejar usar **MySQL**, configure a connection string em `src/ClubNet.API/appsettings.json` e altere `"DatabaseProvider": "MySql"`. O script DDL completo está disponível em `docs/clubnet_database.sql`.

---

### 2. Iniciar o Site Web Administrativo
Em outro terminal:
```bash
dotnet run --project src/ClubNet.Web/ClubNet.Web.csproj --urls http://localhost:5100
```
- Acesse pelo navegador: `http://localhost:5100`
- O site possui botões de login rápido na tela de login para testar instantaneamente como Superadmin, Presidente, Administrador ou Tesoureiro.
- **Funcionalidades do Site:**
  - **Superadministrador:** Cadastrar novos clubes, listar unidades, ativar/desativar clubes, criar presidentes.
  - **Presidente / Administrador:** Dashboard com KPIs, aprovação/rejeição de pedidos de associação com 1 clique, gerenciamento de dependentes, cadastro e cancelamento de eventos, cadastro de espaços e aprovação/rejeição de reservas na agenda, e fila de atendimento (solicitações e reclamações de ouvidoria com parecer).
  - **Tesoureiro:** Painel financeiro, consulta de mensalidades com filtros (Abertas, Pagas, Vencidas), lançamento de novas mensalidades e quitação/baixa com 1 clique.

---

### 3. Iniciar o Aplicativo Mobile (.NET MAUI)
Em outro terminal:
- **Para executar no Android (Emulador ou Dispositivo):**
  ```bash
  dotnet build src/ClubNet.App/ClubNet.App.csproj -t:Run -f net10.0-android
  ```
- **Para compilar o APK:**
  ```bash
  dotnet publish src/ClubNet.App/ClubNet.App.csproj -f net10.0-android -c Release
  ```
- O app conecta automaticamente na API:
  - No emulador Android, utiliza `http://10.0.2.2:5000` (mapeamento nativo para o localhost do computador host).
  - Possui botões de preenchimento rápido para testar a experiência do associado Carlos (aprovado) ou Marcos (em análise), além de permitir cadastrar novas solicitações de associação com seleção de clube e interesses.

---

## 📂 Estrutura de Arquivos

```
ClubNet/
├── docs/
│   └── clubnet_database.sql          # Script DDL e seeds completos para MySQL
├── src/
│   ├── ClubNet.Shared/               # Modelos de domínio, constantes e DTOs
│   │   ├── Constants/
│   │   ├── Models/
│   │   └── DTOs/
│   ├── ClubNet.API/                  # ASP.NET Core Web API RESTful
│   │   ├── Controllers/              # Auth, Clubes, Usuários, Associados, Eventos,
│   │   │                             # Mensalidades, Espaços, Reservas, Solicitações,
│   │   │                             # Reclamações e Dashboard
│   │   ├── Data/                     # DbContext e DbInitializer
│   │   ├── Security/                 # JWT Service, Token Middleware e BCrypt
│   │   └── Services/                 # CurrentUserService com isolamento multi-clube
│   ├── ClubNet.Web/                  # Painel Web Administrativo (Razor Pages)
│   │   ├── Pages/                    # Clubes, Associados, Eventos, Mensalidades,
│   │   │                             # Espaços, Reservas, Atendimento, Usuários
│   │   └── Services/                 # ClubNetApiClient (Consumo REST com Bearer Token)
│   └── ClubNet.App/                  # Aplicativo Mobile .NET MAUI
│       ├── Services/                 # ApiService para integração com a API
│       ├── Views/                    # Login, Home, Mensalidades, Eventos,
│       │                             # Reservas, Atendimento e Perfil
│       └── AppShell.xaml             # Navegação em abas do Associado
├── Especificacao_Projeto_Gestao_Clubes.md
└── README.md
```
