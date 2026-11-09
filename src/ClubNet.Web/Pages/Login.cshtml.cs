using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages;

public class LoginModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public LoginModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Senha { get; set; } = string.Empty;

    public string? MensagemErro { get; set; }

    public IActionResult OnGet()
    {
        if (_apiClient.IsLoggedIn)
        {
            return RedirectToPage("/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Preencha o e-mail e a senha.";
            return Page();
        }

        var result = await _apiClient.LoginAsync(Email, Senha);
        if (!result.Sucesso)
        {
            MensagemErro = result.Mensagem;
            return Page();
        }

        return RedirectToPage("/Index");
    }

    public IActionResult OnGetLogout()
    {
        _apiClient.Logout();
        return RedirectToPage("/Login");
    }
}
