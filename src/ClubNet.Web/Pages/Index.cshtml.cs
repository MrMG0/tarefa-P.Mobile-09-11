using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubNet.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IClubNetApiClient _apiClient;

    public IndexModel(IClubNetApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public DashboardStatsDto? Stats { get; set; }
    public UserInfoDto? CurrentUser => _apiClient.CurrentUser;
    public List<ClubeDto> Clubes { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn)
        {
            return RedirectToPage("/Login");
        }

        var user = _apiClient.CurrentUser;
        Stats = await _apiClient.GetDashboardStatsAsync(user?.Perfil == "SUPERADMIN" ? null : user?.IdClube);

        if (user?.Perfil == "SUPERADMIN")
        {
            Clubes = await _apiClient.GetClubesAsync();
        }

        return Page();
    }
}
