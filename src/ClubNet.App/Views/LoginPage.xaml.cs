using ClubNet.App.Services;
using ClubNet.Shared.DTOs;

namespace ClubNet.App.Views;

public partial class LoginPage : ContentPage
{
    private readonly IApiService _apiService;
    private List<ClubeDto> _clubes = new();

    public LoginPage(IApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _clubes = await _apiService.GetClubesAsync();
        PickerClubes.ItemsSource = _clubes;
        if (_clubes.Count > 0)
        {
            PickerClubes.SelectedIndex = 0;
        }
    }

    private void OnAbaLoginClicked(object sender, EventArgs e)
    {
        PanelLogin.IsVisible = true;
        PanelCadastro.IsVisible = false;
        BtnAbaLogin.BackgroundColor = Color.FromArgb("#0284c7");
        BtnAbaLogin.TextColor = Colors.White;
        BtnAbaCadastro.BackgroundColor = Color.FromArgb("#334155");
        BtnAbaCadastro.TextColor = Color.FromArgb("#94a3b8");
    }

    private void OnAbaCadastroClicked(object sender, EventArgs e)
    {
        PanelLogin.IsVisible = false;
        PanelCadastro.IsVisible = true;
        BtnAbaCadastro.BackgroundColor = Color.FromArgb("#0284c7");
        BtnAbaCadastro.TextColor = Colors.White;
        BtnAbaLogin.BackgroundColor = Color.FromArgb("#334155");
        BtnAbaLogin.TextColor = Color.FromArgb("#94a3b8");
    }

    private async void OnBtnLoginClicked(object sender, EventArgs e)
    {
        var email = TxtLoginEmail.Text?.Trim();
        var senha = TxtLoginSenha.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            await DisplayAlert("Atenção", "Preencha o e-mail e a senha.", "OK");
            return;
        }

        SetLoading(true);
        var result = await _apiService.LoginAsync(email, senha);
        SetLoading(false);

        if (result.Sucesso)
        {
            if (Application.Current?.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = new AppShell();
            }
            else if (Application.Current != null)
            {
#pragma warning disable CS0618
                Application.Current.MainPage = new AppShell();
#pragma warning restore CS0618
            }
        }
        else
        {
            await DisplayAlert("Falha no Acesso", result.Mensagem, "OK");
        }
    }

    private void OnQuickLoginCarlos(object sender, EventArgs e)
    {
        TxtLoginEmail.Text = "carlos.silva@email.com";
        TxtLoginSenha.Text = "User@123";
        OnBtnLoginClicked(sender, e);
    }

    private void OnQuickLoginMarcos(object sender, EventArgs e)
    {
        TxtLoginEmail.Text = "marcos.oliveira@email.com";
        TxtLoginSenha.Text = "User@123";
        OnBtnLoginClicked(sender, e);
    }

    private async void OnBtnCadastrarClicked(object sender, EventArgs e)
    {
        if (PickerClubes.SelectedItem is not ClubeDto clubeSelecionado)
        {
            await DisplayAlert("Atenção", "Selecione o clube ao qual deseja se associar.", "OK");
            return;
        }

        var nome = TxtCadNome.Text?.Trim();
        var cpf = TxtCadCpf.Text?.Trim();
        var email = TxtCadEmail.Text?.Trim();
        var senha = TxtCadSenha.Text;

        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(cpf) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            await DisplayAlert("Atenção", "Preencha todos os campos obrigatórios (*).", "OK");
            return;
        }

        var interesses = TxtCadInteresses.Text?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var dto = new RegisterMemberDto
        {
            IdClube = clubeSelecionado.Id,
            Nome = nome,
            Cpf = cpf,
            Email = email,
            Senha = senha,
            Telefone = TxtCadTelefone.Text?.Trim(),
            InteressesIniciais = interesses
        };

        SetLoading(true);
        var result = await _apiService.RegisterMemberAsync(dto);
        SetLoading(false);

        if (result.Sucesso)
        {
            await DisplayAlert("Solicitação Enviada", result.Mensagem, "OK");
            if (Application.Current?.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = new AppShell();
            }
            else if (Application.Current != null)
            {
#pragma warning disable CS0618
                Application.Current.MainPage = new AppShell();
#pragma warning restore CS0618
            }
        }
        else
        {
            await DisplayAlert("Erro no Cadastro", result.Mensagem, "OK");
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingSpinner.IsVisible = isLoading;
        LoadingSpinner.IsRunning = isLoading;
    }
}
