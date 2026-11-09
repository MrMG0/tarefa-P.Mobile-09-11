using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages.Mensalidades;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<MensalidadeDto> Mensalidades { get; set; } = new();
    public List<AssociadoDto> Associados { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Situacao { get; set; }

    [BindProperty]
    public CreateMensalidadeDto NovaMensalidade { get; set; } = new()
    {
        Valor = 180.00m,
        ReferenciaMes = DateTime.Today.ToString("yyyy-MM"),
        DataVencimento = DateTime.Today.AddDays(10)
    };

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn) return RedirectToPage("/Login");

        var user = _apiClient.CurrentUser;
        var idClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        Mensalidades = await _apiClient.GetMensalidadesAsync(idClube, null, Situacao);
        Associados = await _apiClient.GetAssociadosAsync(idClube, "APROVADO");

        return Page();
    }

    public async Task<IActionResult> OnGetDarBaixaAsync(int id)
    {
        var sucesso = await _apiClient.UpdateSituacaoMensalidadeAsync(id, "PAGA");
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Pagamento registrado! Mensalidade marcada como PAGA.";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao registrar baixa do pagamento.";
        }
        return RedirectToPage(new { Situacao });
    }

    public async Task<IActionResult> OnPostCriarAsync()
    {
        if (NovaMensalidade.IdAssociado <= 0 || NovaMensalidade.Valor <= 0)
        {
            TempData["MensagemErro"] = "Informe o associado e o valor da mensalidade.";
            return RedirectToPage();
        }

        var sucesso = await _apiClient.CreateMensalidadeAsync(NovaMensalidade);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Mensalidade gerada com sucesso!";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao gerar mensalidade.";
        }

        return RedirectToPage(new { Situacao });
    }
}
