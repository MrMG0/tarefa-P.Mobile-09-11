using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages.Reservas;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<ReservaDto> Reservas { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn) return RedirectToPage("/Login");

        var user = _apiClient.CurrentUser;
        var idClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        Reservas = await _apiClient.GetReservasAsync(idClube, Status);
        return Page();
    }

    public async Task<IActionResult> OnGetAprovarAsync(int id)
    {
        var sucesso = await _apiClient.AprovarReservaAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Reserva APROVADA com sucesso! O associado foi notificado.";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao aprovar reserva (verifique se já existe outra reserva conflitante para este horário).";
        }
        return RedirectToPage(new { Status });
    }

    public async Task<IActionResult> OnGetRejeitarAsync(int id)
    {
        var sucesso = await _apiClient.RejeitarReservaAsync(id, "Rejeitada pela diretoria.");
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Reserva rejeitada.";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao rejeitar reserva.";
        }
        return RedirectToPage(new { Status });
    }

    public async Task<IActionResult> OnGetCancelarAsync(int id)
    {
        var sucesso = await _apiClient.CancelarReservaAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Reserva cancelada.";
        }
        return RedirectToPage(new { Status });
    }
}
