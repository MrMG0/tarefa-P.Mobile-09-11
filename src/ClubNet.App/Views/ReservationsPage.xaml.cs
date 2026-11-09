using ClubNet.App.Services;
using ClubNet.Shared.DTOs;

namespace ClubNet.App.Views;

public partial class ReservationsPage : ContentPage
{
    private readonly IApiService _apiService;
    private List<EspacoDto> _espacos = new();

    public ReservationsPage(IApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarDados();
    }

    private async Task CarregarDados()
    {
        PanelMinhasReservas.IsRefreshing = true;
        var reservas = await _apiService.GetMinhasReservasAsync();
        CvReservas.ItemsSource = reservas;
        PanelMinhasReservas.IsRefreshing = false;

        _espacos = await _apiService.GetEspacosAsync();
        PickerEspacos.ItemsSource = _espacos;
        if (_espacos.Count > 0)
        {
            PickerEspacos.SelectedIndex = 0;
        }
    }

    private void OnAbaMinhasReservasClicked(object sender, EventArgs e)
    {
        PanelMinhasReservas.IsVisible = true;
        PanelNovaReserva.IsVisible = false;
        BtnAbaMinhasReservas.BackgroundColor = Color.FromArgb("#0284c7");
        BtnAbaMinhasReservas.TextColor = Colors.White;
        BtnAbaNovaReserva.BackgroundColor = Color.FromArgb("#cbd5e1");
        BtnAbaNovaReserva.TextColor = Color.FromArgb("#334155");
    }

    private void OnAbaNovaReservaClicked(object sender, EventArgs e)
    {
        PanelMinhasReservas.IsVisible = false;
        PanelNovaReserva.IsVisible = true;
        BtnAbaNovaReserva.BackgroundColor = Color.FromArgb("#0284c7");
        BtnAbaNovaReserva.TextColor = Colors.White;
        BtnAbaMinhasReservas.BackgroundColor = Color.FromArgb("#cbd5e1");
        BtnAbaMinhasReservas.TextColor = Color.FromArgb("#334155");
    }

    private async void OnRefreshReservas(object sender, EventArgs e)
    {
        await CarregarDados();
    }

    private async void OnEnviarReservaClicked(object? sender, EventArgs e)
    {
        if (PickerEspacos.SelectedItem is not EspacoDto espaco)
        {
            await DisplayAlert("Atenção", "Selecione o espaço que deseja reservar.", "OK");
            return;
        }

        var data = DpDataReserva.Date ?? DateTime.Today;
        var inicio = data.Add(TpInicio.Time ?? TimeSpan.FromHours(12));
        var fim = data.Add(TpFim.Time ?? TimeSpan.FromHours(18));

        if (fim <= inicio)
        {
            await DisplayAlert("Atenção", "O horário de término deve ser posterior ao horário de início.", "OK");
            return;
        }

        var finalidade = TxtFinalidade.Text?.Trim();
        if (string.IsNullOrWhiteSpace(finalidade))
        {
            await DisplayAlert("Atenção", "Informe a finalidade da reserva.", "OK");
            return;
        }

        var dto = new CreateReservaDto
        {
            IdEspaco = espaco.Id,
            DataHoraInicio = inicio,
            DataHoraFim = fim,
            Finalidade = finalidade,
            Titulo = $"Reserva {espaco.Nome}"
        };

        var (sucesso, mensagem) = await _apiService.SolicitarReservaAsync(dto);
        if (sucesso)
        {
            await DisplayAlert("Sucesso", mensagem, "OK");
            OnAbaMinhasReservasClicked(sender, e);
            await CarregarDados();
        }
        else
        {
            await DisplayAlert("Não foi possível reservar", mensagem, "OK");
        }
    }
}
