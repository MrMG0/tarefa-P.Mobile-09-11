using ClubNet.Shared.DTOs;
using ClubNet.Web.Services;
using System.Globalization;
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

    [BindProperty(SupportsGet = true)]
    public string? Mes { get; set; }

    public DateTime MesAtual { get; private set; }
    public string MesAnterior => MesAtual.AddMonths(-1).ToString("yyyy-MM");
    public string ProximoMes => MesAtual.AddMonths(1).ToString("yyyy-MM");
    public List<CalendarDay> DiasCalendario { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_apiClient.IsLoggedIn) return RedirectToPage("/Login");

        if (string.IsNullOrWhiteSpace(Mes))
        {
            MesAtual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }
        else if (DateTime.TryParseExact(Mes, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mesSelecionado))
        {
            MesAtual = new DateTime(mesSelecionado.Year, mesSelecionado.Month, 1);
        }
        else
        {
            return BadRequest("Mês inválido. Use o formato AAAA-MM.");
        }
        Mes = MesAtual.ToString("yyyy-MM");

        var user = _apiClient.CurrentUser;
        var idClube = user?.Perfil == "SUPERADMIN" ? null : user?.IdClube;

        Reservas = await _apiClient.GetReservasAsync(idClube, Status);
        var primeiroDiaGrade = MesAtual.AddDays(-(int)MesAtual.DayOfWeek);
        DiasCalendario = Enumerable.Range(0, 42)
            .Select(offset =>
            {
                var data = primeiroDiaGrade.AddDays(offset);
                return new CalendarDay
                {
                    Data = data,
                    NoMesAtual = data.Month == MesAtual.Month,
                    Reservas = Reservas
                        .Where(r => r.DataHoraInicio.Date <= data.Date && r.DataHoraFim.Date >= data.Date)
                        .OrderBy(r => r.DataHoraInicio)
                        .ToList()
                };
            })
            .ToList();
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
        return RedirectToPage(new { Status, Mes });
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
        return RedirectToPage(new { Status, Mes });
    }

    public async Task<IActionResult> OnGetCancelarAsync(int id)
    {
        var sucesso = await _apiClient.CancelarReservaAsync(id);
        if (sucesso)
        {
            TempData["MensagemSucesso"] = "Reserva cancelada.";
        }
        return RedirectToPage(new { Status, Mes });
    }

    public sealed class CalendarDay
    {
        public DateTime Data { get; init; }
        public bool NoMesAtual { get; init; }
        public List<ReservaDto> Reservas { get; init; } = new();
    }
}
