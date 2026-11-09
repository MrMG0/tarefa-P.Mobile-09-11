using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages.Usuarios;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<UserInfoDto> Usuarios { get; set; } = new();
    public UserInfoDto? CurrentUser => _apiClient.CurrentUser;

    [BindProperty]
    public CreateUsuarioDto NovoUsuario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn) return RedirectToPage("/Login");

        var user = _apiClient.CurrentUser;
        if (user?.Perfil != "SUPERADMIN" && user?.Perfil != "PRESIDENTE")
        {
            return RedirectToPage("/Index");
        }

        var idClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;
        Usuarios = await _apiClient.GetUsuariosAsync(idClube);

        return Page();
    }

    public async Task<IActionResult> OnPostCriarAsync()
    {
        var user = _apiClient.CurrentUser;
        if (user?.Perfil != "SUPERADMIN" && user?.Perfil != "PRESIDENTE")
        {
            return Forbid();
        }

        NovoUsuario.IdClube = user?.Perfil == "SUPERADMIN" ? NovoUsuario.IdClube : user?.IdClube;

        if (string.IsNullOrWhiteSpace(NovoUsuario.Nome) || string.IsNullOrWhiteSpace(NovoUsuario.Email) || string.IsNullOrWhiteSpace(NovoUsuario.Senha))
        {
            TempData["MensagemErro"] = "Preencha todos os campos obrigatórios.";
            return RedirectToPage();
        }

        var sucesso = await _apiClient.CreateUsuarioAsync(NovoUsuario);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Novo acesso administrativo criado com sucesso!";
        }
        else
        {
            TempData["MensagemErro"] = "Falha ao criar usuário administrativo (verifique se o e-mail já existe).";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetToggleStatusAsync(int id)
    {
        var sucesso = await _apiClient.ToggleUsuarioStatusAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Status de acesso atualizado.";
        }
        return RedirectToPage();
    }
}
