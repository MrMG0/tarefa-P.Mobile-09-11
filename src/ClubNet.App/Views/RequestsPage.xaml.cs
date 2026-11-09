using ClubNet.App.Services;

namespace ClubNet.App.Views;

public partial class RequestsPage : ContentPage
{
    private readonly IApiService _apiService;

    public RequestsPage(IApiService apiService)
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
        var solic = await _apiService.GetMinhasSolicitacoesAsync();
        CvSolic.ItemsSource = solic;

        var reclam = await _apiService.GetMinhasReclamacoesAsync();
        CvReclam.ItemsSource = reclam;
    }

    private void OnAbaSolicClicked(object sender, EventArgs e)
    {
        PanelSolic.IsVisible = true;
        PanelReclam.IsVisible = false;
        BtnAbaSolic.BackgroundColor = Color.FromArgb("#0284c7");
        BtnAbaSolic.TextColor = Colors.White;
        BtnAbaReclam.BackgroundColor = Color.FromArgb("#cbd5e1");
        BtnAbaReclam.TextColor = Color.FromArgb("#334155");
    }

    private void OnAbaReclamClicked(object sender, EventArgs e)
    {
        PanelSolic.IsVisible = false;
        PanelReclam.IsVisible = true;
        BtnAbaReclam.BackgroundColor = Color.FromArgb("#dc2626");
        BtnAbaReclam.TextColor = Colors.White;
        BtnAbaSolic.BackgroundColor = Color.FromArgb("#cbd5e1");
        BtnAbaSolic.TextColor = Color.FromArgb("#334155");
    }

    private async void OnEnviarSolicClicked(object sender, EventArgs e)
    {
        var tipo = TxtSolicTipo.Text?.Trim();
        var desc = TxtSolicDesc.Text?.Trim();

        if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(desc))
        {
            await DisplayAlert("Atenção", "Preencha o tipo e a descrição da solicitação.", "OK");
            return;
        }

        var sucesso = await _apiService.EnviarSolicitacaoAsync(tipo, desc);
        if (sucesso)
        {
            await DisplayAlert("Sucesso", "Solicitação enviada com sucesso!", "OK");
            TxtSolicTipo.Text = "";
            TxtSolicDesc.Text = "";
            await CarregarDados();
        }
        else
        {
            await DisplayAlert("Erro", "Não foi possível enviar a solicitação.", "OK");
        }
    }

    private async void OnEnviarReclamClicked(object sender, EventArgs e)
    {
        var assunto = TxtReclamAssunto.Text?.Trim();
        var desc = TxtReclamDesc.Text?.Trim();

        if (string.IsNullOrWhiteSpace(desc))
        {
            await DisplayAlert("Atenção", "Preencha a descrição da reclamação.", "OK");
            return;
        }

        var sucesso = await _apiService.EnviarReclamacaoAsync(assunto ?? "Geral", desc);
        if (sucesso)
        {
            await DisplayAlert("Sucesso", "Reclamação registrada na ouvidoria.", "OK");
            TxtReclamAssunto.Text = "";
            TxtReclamDesc.Text = "";
            await CarregarDados();
        }
        else
        {
            await DisplayAlert("Erro", "Não foi possível registrar a reclamação.", "OK");
        }
    }
}
