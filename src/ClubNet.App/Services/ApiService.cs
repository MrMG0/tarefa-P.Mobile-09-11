using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClubNet.Shared.DTOs;

namespace ClubNet.App.Services;

public interface IApiService
{
    bool IsLoggedIn { get; }
    string? Token { get; }
    UserInfoDto? CurrentUser { get; }
    string BaseUrl { get; set; }

    Task<LoginResponseDto> LoginAsync(string email, string senha);
    Task<LoginResponseDto> RegisterMemberAsync(RegisterMemberDto dto);
    void Logout();

    Task<List<ClubeDto>> GetClubesAsync();
    Task<AssociadoDto?> GetMeuPerfilAsync();
    Task<DependenteDto?> GetMeuDependenteAsync();
    Task<List<DependenteDto>> GetMeusDependentesAsync();
    Task<bool> AddDependenteAsync(CreateDependenteDto dto);
    Task<List<MensalidadeDto>> GetMinhasMensalidadesAsync();
    Task<List<EventoDto>> GetEventosAsync();
    Task<List<EspacoDto>> GetEspacosAsync();
    Task<List<ReservaDto>> GetMinhasReservasAsync();
    Task<(bool Sucesso, string Mensagem)> SolicitarReservaAsync(CreateReservaDto dto);
    Task<List<SolicitacaoDto>> GetMinhasSolicitacoesAsync();
    Task<bool> EnviarSolicitacaoAsync(string tipo, string descricao);
    Task<List<ReclamacaoDto>> GetMinhasReclamacoesAsync();
    Task<bool> EnviarReclamacaoAsync(string assunto, string descricao);
}

public class ApiService : IApiService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public string BaseUrl { get; set; }

    public string? Token { get; private set; }
    public UserInfoDto? CurrentUser { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(Token) && CurrentUser != null;

    public ApiService()
    {
        // Se for Android Emulator, 10.0.2.2 mapeia para o localhost do host. Caso contrário localhost.
        BaseUrl = DeviceInfo.Platform == DevicePlatform.Android 
            ? "http://10.0.2.2:5000" 
            : "http://localhost:5000";

        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private void SetAuthHeader()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrEmpty(Token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }
    }

    public async Task<LoginResponseDto> LoginAsync(string email, string senha)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{BaseUrl}/api/auth/login", new LoginRequestDto
            {
                Email = email.Trim(),
                Senha = senha
            });

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
                if (result?.Sucesso == true)
                {
                    Token = result.Token;
                    CurrentUser = result.Usuario;
                    return result;
                }
            }

            var erro = ExtrairMensagemErro(content, "Credenciais inválidas ou erro no acesso.");
            return new LoginResponseDto { Sucesso = false, Mensagem = erro };
        }
        catch (HttpRequestException ex)
        {
            return new LoginResponseDto { Sucesso = false, Mensagem = $"Não foi possível conectar ao servidor em {BaseUrl}. Verifique se a API está em execução.\nDetalhes: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto { Sucesso = false, Mensagem = $"Erro de conexão: {ex.Message}" };
        }
    }

    public async Task<LoginResponseDto> RegisterMemberAsync(RegisterMemberDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{BaseUrl}/api/auth/register-member", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
                if (result?.Sucesso == true)
                {
                    Token = result.Token;
                    CurrentUser = result.Usuario;
                    return result;
                }
            }

            var erro = ExtrairMensagemErro(content, $"Erro ao processar cadastro (Status {(int)response.StatusCode}).");
            return new LoginResponseDto
            {
                Sucesso = false,
                Mensagem = erro
            };
        }
        catch (HttpRequestException ex)
        {
            return new LoginResponseDto
            {
                Sucesso = false,
                Mensagem = $"Não foi possível conectar ao servidor em {BaseUrl}. Verifique se a API está em execução.\nDetalhes: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto
            {
                Sucesso = false,
                Mensagem = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    public void Logout()
    {
        Token = null;
        CurrentUser = null;
    }

    public async Task<List<ClubeDto>> GetClubesAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ClubeDto>>($"{BaseUrl}/api/clubes?apenasAtivos=true", _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<AssociadoDto?> GetMeuPerfilAsync()
    {
        try
        {
            if (!CurrentUser?.IdAssociado.HasValue ?? true) return null;
            SetAuthHeader();
            return await _http.GetFromJsonAsync<AssociadoDto>($"{BaseUrl}/api/associados/{CurrentUser!.IdAssociado!.Value}", _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<DependenteDto?> GetMeuDependenteAsync()
    {
        try
        {
            if (CurrentUser?.IdDependente is not int dependentId || CurrentUser.IdAssociado is not int associadoId)
            {
                return null;
            }

            SetAuthHeader();
            var dependentes = await _http.GetFromJsonAsync<List<DependenteDto>>(
                $"{BaseUrl}/api/associados/{associadoId}/dependentes", _jsonOptions);
            return dependentes?.FirstOrDefault(d => d.Id == dependentId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<DependenteDto>> GetMeusDependentesAsync()
    {
        try
        {
            if (!CurrentUser?.IdAssociado.HasValue ?? true) return new();
            SetAuthHeader();
            return await _http.GetFromJsonAsync<List<DependenteDto>>($"{BaseUrl}/api/associados/{CurrentUser!.IdAssociado!.Value}/dependentes", _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> AddDependenteAsync(CreateDependenteDto dto)
    {
        try
        {
            if (!CurrentUser?.IdAssociado.HasValue ?? true) return false;
            dto.IdAssociado = CurrentUser!.IdAssociado!.Value;
            SetAuthHeader();
            var res = await _http.PostAsJsonAsync($"{BaseUrl}/api/associados/{dto.IdAssociado}/dependentes", dto);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<MensalidadeDto>> GetMinhasMensalidadesAsync()
    {
        try
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<List<MensalidadeDto>>($"{BaseUrl}/api/mensalidades", _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<EventoDto>> GetEventosAsync()
    {
        try
        {
            SetAuthHeader();
            var url = CurrentUser?.IdClube.HasValue == true
                ? $"{BaseUrl}/api/eventos?idClube={CurrentUser.IdClube.Value}"
                : $"{BaseUrl}/api/eventos";
            return await _http.GetFromJsonAsync<List<EventoDto>>(url, _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<EspacoDto>> GetEspacosAsync()
    {
        try
        {
            SetAuthHeader();
            var url = CurrentUser?.IdClube.HasValue == true
                ? $"{BaseUrl}/api/espacos?idClube={CurrentUser.IdClube.Value}"
                : $"{BaseUrl}/api/espacos";
            return await _http.GetFromJsonAsync<List<EspacoDto>>(url, _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<ReservaDto>> GetMinhasReservasAsync()
    {
        try
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<List<ReservaDto>>($"{BaseUrl}/api/reservas", _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    private string ExtrairMensagemErro(string jsonOuTexto, string mensagemPadrao)
    {
        if (string.IsNullOrWhiteSpace(jsonOuTexto)) return mensagemPadrao;
        try
        {
            using var doc = JsonDocument.Parse(jsonOuTexto);

            // 1. Mensagem explícita enviada pelos controllers do ClubNet
            if (doc.RootElement.TryGetProperty("mensagem", out var m1) || doc.RootElement.TryGetProperty("Mensagem", out m1))
            {
                var str = m1.GetString();
                if (!string.IsNullOrWhiteSpace(str)) return str;
            }

            // 2. Erros de validação do ModelState (ProblemDetails do ASP.NET Core: errors: { "Campo": ["Erro 1"] })
            if (doc.RootElement.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
            {
                var mensagens = new List<string>();
                foreach (var prop in errorsProp.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var err in prop.Value.EnumerateArray())
                        {
                            var s = err.GetString();
                            if (!string.IsNullOrWhiteSpace(s)) mensagens.Add(s);
                        }
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) mensagens.Add(s);
                    }
                }
                if (mensagens.Count > 0)
                {
                    return string.Join("\n", mensagens);
                }
            }

            // 3. Detalhe ou título do ProblemDetails
            if (doc.RootElement.TryGetProperty("detail", out var mDetail))
            {
                var str = mDetail.GetString();
                if (!string.IsNullOrWhiteSpace(str)) return str;
            }

            if (doc.RootElement.TryGetProperty("title", out var m2))
            {
                var str = m2.GetString();
                if (!string.IsNullOrWhiteSpace(str) && str != "One or more validation errors occurred.") return str;
            }
        }
        catch
        {
            // Retorna o texto se não for JSON
        }

        var textoLimpo = jsonOuTexto.Trim().Trim('"');
        return textoLimpo.Length > 250 ? mensagemPadrao : textoLimpo;
    }

    public async Task<(bool Sucesso, string Mensagem)> SolicitarReservaAsync(CreateReservaDto dto)
    {
        try
        {
            SetAuthHeader();
            dto.IdAssociado = CurrentUser?.IdAssociado;
            dto.IdClube = CurrentUser?.IdClube;

            var res = await _http.PostAsJsonAsync($"{BaseUrl}/api/reservas", dto);
            if (res.IsSuccessStatusCode)
            {
                return (true, "Solicitação de reserva enviada com sucesso! Aguarde a aprovação da diretoria.");
            }

            var erroJson = await res.Content.ReadAsStringAsync();
            var msg = ExtrairMensagemErro(erroJson, "Horário indisponível para este espaço.");
            return (false, msg);
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao solicitar reserva: {ex.Message}");
        }
    }

    public async Task<List<SolicitacaoDto>> GetMinhasSolicitacoesAsync()
    {
        try
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<List<SolicitacaoDto>>($"{BaseUrl}/api/solicitacoes", _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> EnviarSolicitacaoAsync(string tipo, string descricao)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.PostAsJsonAsync($"{BaseUrl}/api/solicitacoes", new CreateSolicitacaoDto
            {
                IdClube = CurrentUser?.IdClube,
                IdAssociado = CurrentUser?.IdAssociado,
                Tipo = tipo,
                Descricao = descricao
            });
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<ReclamacaoDto>> GetMinhasReclamacoesAsync()
    {
        try
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<List<ReclamacaoDto>>($"{BaseUrl}/api/reclamacoes", _jsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> EnviarReclamacaoAsync(string assunto, string descricao)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.PostAsJsonAsync($"{BaseUrl}/api/reclamacoes", new CreateReclamacaoDto
            {
                IdClube = CurrentUser?.IdClube,
                IdAssociado = CurrentUser?.IdAssociado,
                Assunto = assunto,
                Descricao = descricao
            });
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
