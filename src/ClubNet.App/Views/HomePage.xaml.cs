using ClubNet.App.Services;

namespace ClubNet.App.Views;

public partial class HomePage : ContentPage
{
    private readonly IApiService _apiService;

    public HomePage(IApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var user = _apiService.CurrentUser;
        if (user != null)
        {
            LblNomeClube.Text = user.NomeClube ?? "Clube Social";
            LblNomeAssociado.Text = user.Nome;

            var perfil = await _apiService.GetMeuPerfilAsync();
            if (perfil != null)
            {
                LblSituacao.Text = perfil.SituacaoCadastro;
                if (perfil.SituacaoCadastro == "APROVADO")
                {
                    BadgeSituacao.BackgroundColor = Color.FromArgb("#065f46");
                    LblSituacao.TextColor = Color.FromArgb("#a7f3d0");
                }
                else if (perfil.SituacaoCadastro == "PENDENTE")
                {
                    BadgeSituacao.BackgroundColor = Color.FromArgb("#78350f");
                    LblSituacao.TextColor = Color.FromArgb("#fde68a");
                }
            }
        }
    }

    private async void OnNavMensalidades(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Mensalidades");
    }

    private async void OnNavReservas(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Reservas");
    }

    private async void OnNavEventos(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Eventos");
    }

    private async void OnNavAtendimento(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Atendimento");
    }

    private async void OnNavPerfil(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Perfil");
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sair", "Deseja realmente sair da sua conta?", "Sim", "Cancelar");
        if (confirm)
        {
            _apiService.Logout();
            if (Application.Current?.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = new LoginPage(_apiService);
            }
            else if (Application.Current != null)
            {
#pragma warning disable CS0618
                Application.Current.MainPage = new LoginPage(_apiService);
#pragma warning restore CS0618
            }
        }
    }
}
