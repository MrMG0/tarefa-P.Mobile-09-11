using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages.Associados;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<AssociadoDto> Associados { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Situacao { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Busca { get; set; }

    [BindProperty]
    public CreateDependenteDto NovoDependente { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn) return RedirectToPage("/Login");

        var user = _apiClient.CurrentUser;
        var idClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        Associados = await _apiClient.GetAssociadosAsync(idClube, Situacao, Busca);
        return Page();
    }

    public async Task<IActionResult> OnGetAprovarAsync(int id)
    {
        var sucesso = await _apiClient.AprovarAssociadoAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Cadastro de associado APROVADO com sucesso!";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao aprovar o associado.";
        }
        return RedirectToPage(new { Situacao, Busca });
    }

    public async Task<IActionResult> OnGetRejeitarAsync(int id)
    {
        var sucesso = await _apiClient.RejeitarAssociadoAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Solicitação de associação rejeitada.";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao rejeitar o associado.";
        }
        return RedirectToPage(new { Situacao, Busca });
    }

    public async Task<IActionResult> OnPostAddDependenteAsync()
    {
        if (NovoDependente.IdAssociado <= 0 || string.IsNullOrWhiteSpace(NovoDependente.Nome))
        {
            TempData["MensagemErro"] = "Informe o nome e o associado para vincular o dependente.";
            return RedirectToPage();
        }

        var sucesso = await _apiClient.CreateDependenteAsync(NovoDependente.IdAssociado, NovoDependente);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Dependente adicionado com sucesso!";
        }
        else
        {
            TempData["MensagemErro"] = "Não foi possível cadastrar o dependente.";
        }

        return RedirectToPage(new { Situacao, Busca });
    }
}
