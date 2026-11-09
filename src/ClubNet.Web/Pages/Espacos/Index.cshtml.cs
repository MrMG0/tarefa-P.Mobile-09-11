using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages.Espacos;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<EspacoDto> Espacos { get; set; } = new();

    [BindProperty]
    public CreateEspacoDto NovoEspaco { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn) return RedirectToPage("/Login");

        var user = _apiClient.CurrentUser;
        var idClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        Espacos = await _apiClient.GetEspacosAsync(idClube);
        return Page();
    }

    public async Task<IActionResult> OnPostCriarAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoEspaco.Nome))
        {
            TempData["MensagemErro"] = "Nome do espaço é obrigatório.";
            return RedirectToPage();
        }

        var user = _apiClient.CurrentUser;
        NovoEspaco.IdClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        var sucesso = await _apiClient.CreateEspacoAsync(NovoEspaco);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Espaço cadastrado com sucesso!";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao cadastrar espaço.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDeletarAsync(int id)
    {
        var sucesso = await _apiClient.DeleteEspacoAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Espaço desativado.";
        }
        return RedirectToPage();
    }
}
