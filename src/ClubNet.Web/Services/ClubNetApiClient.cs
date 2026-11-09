using System.Net.Http.Headers;
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

    private void SetAuthHeader()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        var token = Token;
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<LoginResponseDto> LoginAsync(string email, string senha)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Senha = senha });
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
            return error ?? new LoginResponseDto { Sucesso = false, Mensagem = "Falha na comunicação com a API." };
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
        if (result?.Sucesso == true && !string.IsNullOrEmpty(result.Token) && result.Usuario != null)
        {
            Session?.SetString(SessionTokenKey, result.Token);
            Session?.SetString(SessionUserKey, JsonSerializer.Serialize(result.Usuario));
        }

        return result ?? new LoginResponseDto { Sucesso = false, Mensagem = "Resposta inválida." };
    }

    public void Logout()
    {
        Session?.Remove(SessionTokenKey);
        Session?.Remove(SessionUserKey);
    }

    public async Task<DashboardStatsDto?> GetDashboardStatsAsync(int? idClube = null)
    {
        SetAuthHeader();
        var url = idClube.HasValue ? $"/api/dashboard?idClube={idClube.Value}" : "/api/dashboard";
        return await _http.GetFromJsonAsync<DashboardStatsDto>(url, _jsonOptions);
    }

    // Clubes
    public async Task<List<ClubeDto>> GetClubesAsync(bool? apenasAtivos = null)
    {
        SetAuthHeader();
        var url = apenasAtivos.HasValue ? $"/api/clubes?apenasAtivos={apenasAtivos.Value}" : "/api/clubes";
        return await _http.GetFromJsonAsync<List<ClubeDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<ClubeDto?> GetClubeByIdAsync(int id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<ClubeDto>($"/api/clubes/{id}", _jsonOptions);
    }

    public async Task<bool> CreateClubeAsync(CreateClubeDto dto)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("/api/clubes", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateClubeAsync(int id, CreateClubeDto dto)
    {
        SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"/api/clubes/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleClubeStatusAsync(int id)
    {
        SetAuthHeader();
        var response = await _http.PatchAsync($"/api/clubes/{id}/status", null);
        return response.IsSuccessStatusCode;
    }

    // Associados
    public async Task<List<AssociadoDto>> GetAssociadosAsync(int? idClube = null, string? situacao = null, string? busca = null)
    {
        SetAuthHeader();
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (!string.IsNullOrEmpty(situacao)) query.Add($"situacao={Uri.EscapeDataString(situacao)}");
        if (!string.IsNullOrEmpty(busca)) query.Add($"busca={Uri.EscapeDataString(busca)}");

        var url = "/api/associados" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<AssociadoDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<AssociadoDto?> GetAssociadoByIdAsync(int id)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<AssociadoDto>($"/api/associados/{id}", _jsonOptions);
    }

    public async Task<bool> AprovarAssociadoAsync(int id)
    {
        SetAuthHeader();
        var response = await _http.PatchAsync($"/api/associados/{id}/aprovar", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejeitarAssociadoAsync(int id)
    {
        SetAuthHeader();
        var response = await _http.PatchAsync($"/api/associados/{id}/rejeitar", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateDependenteAsync(int associadoId, CreateDependenteDto dto)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"/api/associados/{associadoId}/dependentes", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteDependenteAsync(int associadoId, int dependenteId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"/api/associados/{associadoId}/dependentes/{dependenteId}");
        return response.IsSuccessStatusCode;
    }

    // Eventos
    public async Task<List<EventoDto>> GetEventosAsync(int? idClube = null)
    {
        SetAuthHeader();
        var url = idClube.HasValue ? $"/api/eventos?idClube={idClube.Value}" : "/api/eventos";
        return await _http.GetFromJsonAsync<List<EventoDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<bool> CreateEventoAsync(CreateEventoDto dto)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("/api/eventos", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteEventoAsync(int id)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"/api/eventos/{id}");
        return response.IsSuccessStatusCode;
    }

    // Mensalidades
    public async Task<List<MensalidadeDto>> GetMensalidadesAsync(int? idClube = null, int? idAssociado = null, string? situacao = null)
    {
        SetAuthHeader();
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (idAssociado.HasValue) query.Add($"idAssociado={idAssociado.Value}");
        if (!string.IsNullOrEmpty(situacao)) query.Add($"situacao={Uri.EscapeDataString(situacao)}");

        var url = "/api/mensalidades" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<MensalidadeDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<bool> CreateMensalidadeAsync(CreateMensalidadeDto dto)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("/api/mensalidades", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSituacaoMensalidadeAsync(int id, string situacao)
    {
        SetAuthHeader();
        var response = await _http.PatchAsJsonAsync($"/api/mensalidades/{id}/situacao", new UpdateSituacaoMensalidadeDto { Situacao = situacao });
        return response.IsSuccessStatusCode;
    }

    // Espaços
    public async Task<List<EspacoDto>> GetEspacosAsync(int? idClube = null)
    {
        SetAuthHeader();
        var url = idClube.HasValue ? $"/api/espacos?idClube={idClube.Value}" : "/api/espacos";
        return await _http.GetFromJsonAsync<List<EspacoDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<bool> CreateEspacoAsync(CreateEspacoDto dto)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("/api/espacos", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteEspacoAsync(int id)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"/api/espacos/{id}");
        return response.IsSuccessStatusCode;
    }

    // Reservas
    public async Task<List<ReservaDto>> GetReservasAsync(int? idClube = null, string? status = null)
    {
        SetAuthHeader();
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

        var url = "/api/reservas" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<ReservaDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<bool> AprovarReservaAsync(int id)
    {
        SetAuthHeader();
        var response = await _http.PatchAsync($"/api/reservas/{id}/aprovar", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejeitarReservaAsync(int id, string? observacao = null)
    {
        SetAuthHeader();
        var response = await _http.PatchAsJsonAsync($"/api/reservas/{id}/rejeitar", new UpdateStatusReservaDto { Status = "REJEITADA", Observacao = observacao });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelarReservaAsync(int id)
    {
        SetAuthHeader();
        var response = await _http.PatchAsJsonAsync($"/api/reservas/{id}/cancelar", new UpdateStatusReservaDto { Status = "CANCELADA" });
        return response.IsSuccessStatusCode;
    }

    // Atendimento
    public async Task<List<SolicitacaoDto>> GetSolicitacoesAsync(int? idClube = null, string? status = null)
    {
        SetAuthHeader();
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

        var url = "/api/solicitacoes" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<SolicitacaoDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<bool> ResponderSolicitacaoAsync(int id, string status, string resposta)
    {
        SetAuthHeader();
        var response = await _http.PatchAsJsonAsync($"/api/solicitacoes/{id}/atendimento", new UpdateAtendimentoDto { Status = status, Resposta = resposta });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ReclamacaoDto>> GetReclamacoesAsync(int? idClube = null, string? status = null)
    {
        SetAuthHeader();
        var query = new List<string>();
        if (idClube.HasValue) query.Add($"idClube={idClube.Value}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

        var url = "/api/reclamacoes" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<ReclamacaoDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<bool> ResponderReclamacaoAsync(int id, string status, string resposta)
    {
        SetAuthHeader();
        var response = await _http.PatchAsJsonAsync($"/api/reclamacoes/{id}/atendimento", new UpdateAtendimentoDto { Status = status, Resposta = resposta });
        return response.IsSuccessStatusCode;
    }

    // Usuários
    public async Task<List<UserInfoDto>> GetUsuariosAsync(int? idClube = null)
    {
        SetAuthHeader();
        var url = idClube.HasValue ? $"/api/usuarios?idClube={idClube.Value}" : "/api/usuarios";
        return await _http.GetFromJsonAsync<List<UserInfoDto>>(url, _jsonOptions) ?? new();
    }

    public async Task<bool> CreateUsuarioAsync(CreateUsuarioDto dto)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("/api/usuarios", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleUsuarioStatusAsync(int id)
    {
        SetAuthHeader();
        var response = await _http.PatchAsync($"/api/usuarios/{id}/status", null);
        return response.IsSuccessStatusCode;
    }
}
