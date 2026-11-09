using ClubNet.App.Services;

namespace ClubNet.App.Views;

public partial class EventsPage : ContentPage
{
    private readonly IApiService _apiService;

    public EventsPage(IApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarEventos();
    }

    private async Task CarregarEventos()
    {
        RefreshViewEventos.IsRefreshing = true;
        var list = await _apiService.GetEventosAsync();
        CvEventos.ItemsSource = list;
        RefreshViewEventos.IsRefreshing = false;
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await CarregarEventos();
    }
}
