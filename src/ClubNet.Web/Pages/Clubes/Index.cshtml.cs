using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages.Clubes;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<ClubeDto> Clubes { get; set; } = new();

    [BindProperty]
    public CreateClubeDto NovoClube { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn || _apiClient.CurrentUser?.Perfil != "SUPERADMIN")
        {
            return RedirectToPage("/Login");
        }

        Clubes = await _apiClient.GetClubesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCriarAsync()
    {
        if (!ModelState.IsValid)
        {
            TempData["MensagemErro"] = "Preencha corretamente os dados do clube.";
            return RedirectToPage();
        }

        var sucesso = await _apiClient.CreateClubeAsync(NovoClube);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Clube cadastrado com sucesso na rede!";
        }
        else
        {
            TempData["MensagemErro"] = "Não foi possível cadastrar o clube.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetToggleStatusAsync(int id)
    {
        var sucesso = await _apiClient.ToggleClubeStatusAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Status do clube atualizado.";
        }
        return RedirectToPage();
    }
}
