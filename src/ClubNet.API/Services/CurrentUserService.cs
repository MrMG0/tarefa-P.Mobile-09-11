using System.Security.Claims;
using ClubNet.Shared.Constants;

namespace ClubNet.API.Services;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    string? Email { get; }
    string? Nome { get; }
    string? Perfil { get; }
    int? ClubeId { get; }
    int? AssociadoId { get; }
    int? DependenteId { get; }

    bool IsSuperAdmin { get; }
    bool IsPresidente { get; }
    bool IsAdmin { get; }
    bool IsTesoureiro { get; }
    bool IsAssociado { get; }
    bool IsDependente { get; }

    bool CanManageClub(int clubeId);
    bool CanManageFinance(int clubeId);
    int? GetEffectiveClubeId(int? requestedClubeId);
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true && UserId.HasValue;

    public int? UserId
    {
        get
        {
            var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? Nome => User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Perfil => User?.FindFirst(ClaimTypes.Role)?.Value 
                          ?? User?.FindFirst("perfil")?.Value;

    public int? ClubeId
    {
        get
        {
            var claim = User?.FindFirst("id_clube")?.Value 
                     ?? User?.FindFirst("clube_id")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public int? AssociadoId
    {
        get
        {
            var claim = User?.FindFirst("id_associado")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public int? DependenteId
    {
        get
        {
            var claim = User?.FindFirst("id_dependente")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public bool IsSuperAdmin => string.Equals(Perfil, Perfis.SuperAdmin, StringComparison.OrdinalIgnoreCase);
    public bool IsPresidente => string.Equals(Perfil, Perfis.Presidente, StringComparison.OrdinalIgnoreCase);
    public bool IsAdmin => string.Equals(Perfil, Perfis.Admin, StringComparison.OrdinalIgnoreCase);
    public bool IsTesoureiro => string.Equals(Perfil, Perfis.Tesoureiro, StringComparison.OrdinalIgnoreCase);
    public bool IsAssociado =>
        string.Equals(Perfil, Perfis.Associado, StringComparison.OrdinalIgnoreCase) ||
        IsDependente;
    public bool IsDependente => string.Equals(Perfil, Perfis.Dependente, StringComparison.OrdinalIgnoreCase);

    public bool CanManageClub(int targetClubeId)
    {
        if (IsSuperAdmin) return true;
        if (ClubeId == targetClubeId && (IsPresidente || IsAdmin)) return true;
        return false;
    }

    public bool CanManageFinance(int targetClubeId)
    {
        if (IsSuperAdmin) return true;
        if (ClubeId == targetClubeId && (IsPresidente || IsAdmin || IsTesoureiro)) return true;
        return false;
    }

    public int? GetEffectiveClubeId(int? requestedClubeId)
    {
        if (IsSuperAdmin)
        {
            return requestedClubeId ?? ClubeId;
        }

        // Para qualquer outro perfil, força o ClubeId vinculado ao usuário autenticado
        return ClubeId;
    }
}
