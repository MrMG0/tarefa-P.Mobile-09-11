using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ClubNet.Shared.DTOs;

namespace ClubNet.Web.Services;

public interface IClubNetApiClient
{
    bool IsLoggedIn { get; }
    UserInfoDto? CurrentUser { get; }
    string? Token { get; }

    Task<LoginResponseDto> LoginAsync(string email, string senha);
    void Logout();

    Task<DashboardStatsDto?> GetDashboardStatsAsync(int? idClube = null);

    // Clubes
    Task<List<ClubeDto>> GetClubesAsync(bool? apenasAtivos = null);
    Task<ClubeDto?> GetClubeByIdAsync(int id);
    Task<bool> CreateClubeAsync(CreateClubeDto dto);
    Task<bool> UpdateClubeAsync(int id, CreateClubeDto dto);
    Task<bool> ToggleClubeStatusAsync(int id);

    // Associados
    Task<List<AssociadoDto>> GetAssociadosAsync(int? idClube = null, string? situacao = null, string? busca = null);
    Task<AssociadoDto?> GetAssociadoByIdAsync(int id);
    Task<bool> UpdateAssociadoAsync(int id, UpdateAssociadoDto dto);
    Task<bool> AprovarAssociadoAsync(int id);
    Task<bool> RejeitarAssociadoAsync(int id);
    Task<bool> CreateDependenteAsync(int associadoId, CreateDependenteDto dto);
    Task<bool> DeleteDependenteAsync(int associadoId, int dependenteId);

    // Eventos
    Task<List<EventoDto>> GetEventosAsync(int? idClube = null);
    Task<bool> CreateEventoAsync(CreateEventoDto dto);
    Task<bool> DeleteEventoAsync(int id);

    // Mensalidades
    Task<List<MensalidadeDto>> GetMensalidadesAsync(int? idClube = null, int? idAssociado = null, string? situacao = null);
    Task<bool> CreateMensalidadeAsync(CreateMensalidadeDto dto);
    Task<bool> UpdateSituacaoMensalidadeAsync(int id, string situacao);

    // Espaços
    Task<List<EspacoDto>> GetEspacosAsync(int? idClube = null);
    Task<bool> CreateEspacoAsync(CreateEspacoDto dto);
    Task<bool> DeleteEspacoAsync(int id);

    // Reservas
    Task<List<ReservaDto>> GetReservasAsync(int? idClube = null, string? status = null);
    Task<bool> AprovarReservaAsync(int id);
    Task<bool> RejeitarReservaAsync(int id, string? observacao = null);
    Task<bool> CancelarReservaAsync(int id);

    // Atendimento
    Task<List<SolicitacaoDto>> GetSolicitacoesAsync(int? idClube = null, string? status = null);
    Task<bool> ResponderSolicitacaoAsync(int id, string status, string resposta);
    Task<List<ReclamacaoDto>> GetReclamacoesAsync(int? idClube = null, string? status = null);
    Task<bool> ResponderReclamacaoAsync(int id, string status, string resposta);

    // Usuários
    Task<List<UserInfoDto>> GetUsuariosAsync(int? idClube = null);
    Task<bool> CreateUsuarioAsync(CreateUsuarioDto dto);
    Task<bool> ToggleUsuarioStatusAsync(int id);
}

public class ClubNetApiClient : IClubNetApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string SessionTokenKey = "ClubNet_Token";
    private const string SessionUserKey = "ClubNet_User";

    public ClubNetApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor, IConfiguration config)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var baseUrl = config["ApiBaseUrl"] ?? "http://localhost:5000";
        if (!_http.BaseAddress?.ToString().StartsWith("http") ?? true)
        {
            _http.BaseAddress = new Uri(baseUrl);
        }
    }

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public string? Token => Session?.GetString(SessionTokenKey);

    public UserInfoDto? CurrentUser
    {
        get
        {
            var json = Session?.GetString(SessionUserKey);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<UserInfoDto>(json, _jsonOptions);
        }
    }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Token) && CurrentUser != null;

    private HttpRequestMessage CreateRequest(HttpMethod method, string uri, object? jsonContent = null)
    {
        var request = new HttpRequestMessage(method, uri);
        var token = Token;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        if (jsonContent != null)
        {
            request.Content = JsonContent.Create(jsonContent, options: _jsonOptions);
        }
        return request;
    }

    private async Task<T?> SendAndGetJsonAsync<T>(string uri) where T : class
    {
        try
        {
            using var req = CreateRequest(HttpMethod.Get, uri);
            using var res = await _http.SendAsync(req);
            if (res.StatusCode == HttpStatusCode.Unauthorized)
            {
                Logout();
                return null;
            }
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }
            return await res.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> SendAsync(HttpMethod method, string uri, object? jsonContent = null)
    {
        try
        {
            using var req = CreateRequest(method, uri, jsonContent);
            using var res = await _http.SendAsync(req);
            if (res.StatusCode == HttpStatusCode.Unauthorized)
            {
                Logout();
                return false;
            }
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<LoginResponseDto> LoginAsync(string email, string senha)
    {
        try
        {
            using var req = CreateRequest(HttpMethod.Post, "/api/auth/login", new LoginRequestDto { Email = email, Senha = senha });
            using var response = await _http.SendAsync(req);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
                return error ?? new LoginResponseDto { Sucesso = false, Mensagem = "Credenciais inválidas ou erro no servidor." };
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
            if (result?.Sucesso == true && !string.IsNullOrEmpty(result.Token) && result.Usuario != null)
            {
                Session?.SetString(SessionTokenKey, result.Token);
                Session?.SetString(SessionUserKey, JsonSerializer.Serialize(result.Usuario));
            }

            return result ?? new LoginResponseDto { Sucesso = false, Mensagem = "Resposta inválida." };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto { Sucesso = false, Mensagem = $"Erro ao conectar com a API: {ex.Message}" };
        }
    }

    public void Logout()
    {
        Session?.Remove(SessionTokenKey);
        Session?.Remove(SessionUserKey);
    }

    public async Task<DashboardStatsDto?> GetDashboardStatsAsync(int? idClube = null)
    {
        var url = idClube.HasValue ? $"/api/dashboard?idClube={idClube.Value}" : "/api/dashboard";
        return await SendAndGetJsonAsync<DashboardStatsDto>(url);
    }

    // Clubes
    public async Task<List<ClubeDto>> GetClubesAsync(bool? apenasAtivos = null)
    {
        var url = apenasAtivos.HasValue ? $"/api/clubes?apenasAtivos={apenasAtivos.Value}" : "/api/clubes";
        return await SendAndGetJsonAsync<List<ClubeDto>>(url) ?? new();
    }

    public async Task<ClubeDto?> GetClubeByIdAsync(int id)
    {
        return await SendAndGetJsonAsync<ClubeDto>($"/api/clubes/{id}");
    }

    public async Task<bool> CreateClubeAsync(CreateClubeDto dto)
    {
        return await SendAsync(HttpMethod.Post, "/api/clubes", dto);
    }

    public async Task<bool> UpdateClubeAsync(int id, CreateClubeDto dto)
    {
        return await SendAsync(HttpMethod.Put, $"/api/clubes/{id}", dto);
    }

    public async Task<bool> ToggleClubeStatusAsync(int id)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/clubes/{id}/status");
    }

    // Associados
    public async Task<List<AssociadoDto>> GetAssociadosAsync(int? idClube = null, string? situacao = null, string? busca = null)
    {
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (!string.IsNullOrEmpty(situacao)) query.Add($"situacao={Uri.EscapeDataString(situacao)}");
        if (!string.IsNullOrEmpty(busca)) query.Add($"busca={Uri.EscapeDataString(busca)}");

        var url = "/api/associados" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await SendAndGetJsonAsync<List<AssociadoDto>>(url) ?? new();
    }

    public async Task<AssociadoDto?> GetAssociadoByIdAsync(int id)
    {
        return await SendAndGetJsonAsync<AssociadoDto>($"/api/associados/{id}");
    }

    public async Task<bool> UpdateAssociadoAsync(int id, UpdateAssociadoDto dto)
    {
        return await SendAsync(HttpMethod.Put, $"/api/associados/{id}", dto);
    }

    public async Task<bool> AprovarAssociadoAsync(int id)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/associados/{id}/aprovar");
    }

    public async Task<bool> RejeitarAssociadoAsync(int id)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/associados/{id}/rejeitar");
    }

    public async Task<bool> CreateDependenteAsync(int associadoId, CreateDependenteDto dto)
    {
        return await SendAsync(HttpMethod.Post, $"/api/associados/{associadoId}/dependentes", dto);
    }

    public async Task<bool> DeleteDependenteAsync(int associadoId, int dependenteId)
    {
        return await SendAsync(HttpMethod.Delete, $"/api/associados/{associadoId}/dependentes/{dependenteId}");
    }

    // Eventos
    public async Task<List<EventoDto>> GetEventosAsync(int? idClube = null)
    {
        var url = idClube.HasValue ? $"/api/eventos?idClube={idClube.Value}" : "/api/eventos";
        return await SendAndGetJsonAsync<List<EventoDto>>(url) ?? new();
    }

    public async Task<bool> CreateEventoAsync(CreateEventoDto dto)
    {
        return await SendAsync(HttpMethod.Post, "/api/eventos", dto);
    }

    public async Task<bool> DeleteEventoAsync(int id)
    {
        return await SendAsync(HttpMethod.Delete, $"/api/eventos/{id}");
    }

    // Mensalidades
    public async Task<List<MensalidadeDto>> GetMensalidadesAsync(int? idClube = null, int? idAssociado = null, string? situacao = null)
    {
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (idAssociado.HasValue) query.Add($"idAssociado={idAssociado.Value}");
        if (!string.IsNullOrEmpty(situacao)) query.Add($"situacao={Uri.EscapeDataString(situacao)}");

        var url = "/api/mensalidades" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await SendAndGetJsonAsync<List<MensalidadeDto>>(url) ?? new();
    }

    public async Task<bool> CreateMensalidadeAsync(CreateMensalidadeDto dto)
    {
        return await SendAsync(HttpMethod.Post, "/api/mensalidades", dto);
    }

    public async Task<bool> UpdateSituacaoMensalidadeAsync(int id, string situacao)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/mensalidades/{id}/situacao", new UpdateSituacaoMensalidadeDto { Situacao = situacao });
    }

    // Espaços
    public async Task<List<EspacoDto>> GetEspacosAsync(int? idClube = null)
    {
        var url = idClube.HasValue ? $"/api/espacos?idClube={idClube.Value}" : "/api/espacos";
        return await SendAndGetJsonAsync<List<EspacoDto>>(url) ?? new();
    }

    public async Task<bool> CreateEspacoAsync(CreateEspacoDto dto)
    {
        return await SendAsync(HttpMethod.Post, "/api/espacos", dto);
    }

    public async Task<bool> DeleteEspacoAsync(int id)
    {
        return await SendAsync(HttpMethod.Delete, $"/api/espacos/{id}");
    }

    // Reservas
    public async Task<List<ReservaDto>> GetReservasAsync(int? idClube = null, string? status = null)
    {
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

        var url = "/api/reservas" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await SendAndGetJsonAsync<List<ReservaDto>>(url) ?? new();
    }

    public async Task<bool> AprovarReservaAsync(int id)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/reservas/{id}/aprovar");
    }

    public async Task<bool> RejeitarReservaAsync(int id, string? observacao = null)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/reservas/{id}/rejeitar", new UpdateStatusReservaDto { Status = "REJEITADA", Observacao = observacao });
    }

    public async Task<bool> CancelarReservaAsync(int id)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/reservas/{id}/cancelar", new UpdateStatusReservaDto { Status = "CANCELADA" });
    }

    // Atendimento
    public async Task<List<SolicitacaoDto>> GetSolicitacoesAsync(int? idClube = null, string? status = null)
    {
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

        var url = "/api/solicitacoes" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await SendAndGetJsonAsync<List<SolicitacaoDto>>(url) ?? new();
    }

    public async Task<bool> ResponderSolicitacaoAsync(int id, string status, string resposta)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/solicitacoes/{id}/atendimento", new UpdateAtendimentoDto { Status = status, Resposta = resposta });
    }

    public async Task<List<ReclamacaoDto>> GetReclamacoesAsync(int? idClube = null, string? status = null)
    {
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

        var url = "/api/reclamacoes" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await SendAndGetJsonAsync<List<ReclamacaoDto>>(url) ?? new();
    }

    public async Task<bool> ResponderReclamacaoAsync(int id, string status, string resposta)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/reclamacoes/{id}/atendimento", new UpdateAtendimentoDto { Status = status, Resposta = resposta });
    }

    // Usuários
    public async Task<List<UserInfoDto>> GetUsuariosAsync(int? idClube = null)
    {
        var url = idClube.HasValue ? $"/api/usuarios?idClube={idClube.Value}" : "/api/usuarios";
        return await SendAndGetJsonAsync<List<UserInfoDto>>(url) ?? new();
    }

    public async Task<bool> CreateUsuarioAsync(CreateUsuarioDto dto)
    {
        return await SendAsync(HttpMethod.Post, "/api/usuarios", dto);
    }

    public async Task<bool> ToggleUsuarioStatusAsync(int id)
    {
        return await SendAsync(HttpMethod.Patch, $"/api/usuarios/{id}/status");
    }
}
