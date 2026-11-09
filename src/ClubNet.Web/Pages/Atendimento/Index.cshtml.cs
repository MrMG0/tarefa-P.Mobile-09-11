using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages.Atendimento;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<SolicitacaoDto> Solicitacoes { get; set; } = new();
    public List<ReclamacaoDto> Reclamacoes { get; set; } = new();

    [BindProperty]
    public int AtendimentoId { get; set; }

    [BindProperty]
    public string TipoRegistro { get; set; } = "SOLICITACAO"; // "SOLICITACAO" ou "RECLAMACAO"

    [BindProperty]
    public string NovoStatus { get; set; } = "RESPONDIDA";

    [BindProperty]
    public string RespostaTexto { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn) return RedirectToPage("/Login");

        var user = _apiClient.CurrentUser;
        var idClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        Solicitacoes = await _apiClient.GetSolicitacoesAsync(idClube);
        Reclamacoes = await _apiClient.GetReclamacoesAsync(idClube);

        return Page();
    }

    public async Task<IActionResult> OnPostResponderAsync()
    {
        if (AtendimentoId <= 0 || string.IsNullOrWhiteSpace(RespostaTexto))
        {
            TempData["MensagemErro"] = "Digite uma resposta para o atendimento.";
            return RedirectToPage();
        }

        bool sucesso;
        if (TipoRegistro == "RECLAMACAO")
        {
            sucesso = await _apiClient.ResponderReclamacaoAsync(AtendimentoId, NovoStatus, RespostaTexto);
        }
        else
        {
            sucesso = await _apiClient.ResponderSolicitacaoAsync(AtendimentoId, NovoStatus, RespostaTexto);
        }

        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Resposta registrada e enviada ao associado!";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao registrar resposta.";
        }

        return RedirectToPage();
    }
}
