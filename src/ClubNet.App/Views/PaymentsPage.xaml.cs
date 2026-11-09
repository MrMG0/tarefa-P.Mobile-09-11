using ClubNet.App.Services;

namespace ClubNet.App.Views;

public partial class PaymentsPage : ContentPage
{
    private readonly IApiService _apiService;

    public PaymentsPage(IApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarMensalidades();
    }

    private async Task CarregarMensalidades()
    {
        RefreshViewMensalidades.IsRefreshing = true;
        var list = await _apiService.GetMinhasMensalidadesAsync();
        CvMensalidades.ItemsSource = list;
        RefreshViewMensalidades.IsRefreshing = false;
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await CarregarMensalidades();
    }
}
