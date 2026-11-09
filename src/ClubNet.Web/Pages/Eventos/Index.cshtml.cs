using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages.Eventos;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<EventoDto> Eventos { get; set; } = new();

    [BindProperty]
    public CreateEventoDto NovoEvento { get; set; } = new()
    {
        DataInicio = DateTime.Today.AddDays(7).AddHours(10)
    };

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn) return RedirectToPage("/Login");

        var user = _apiClient.CurrentUser;
        var idClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        Eventos = await _apiClient.GetEventosAsync(idClube);
        return Page();
    }

    public async Task<IActionResult> OnPostCriarAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoEvento.Titulo))
        {
            TempData["MensagemErro"] = "Título do evento é obrigatório.";
            return RedirectToPage();
        }

        var user = _apiClient.CurrentUser;
        NovoEvento.IdClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        var sucesso = await _apiClient.CreateEventoAsync(NovoEvento);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Evento cadastrado e publicado com sucesso!";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao cadastrar evento.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDeletarAsync(int id)
    {
        var sucesso = await _apiClient.DeleteEventoAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Evento desativado.";
        }
        return RedirectToPage();
    }
}
