using ClubNet.App.Services;
using ClubNet.Shared.DTOs;

namespace ClubNet.App.Views;

public partial class ProfilePage : ContentPage
{
    private readonly IApiService _apiService;

    public ProfilePage(IApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarPerfil();
    }

    private async Task CarregarPerfil()
    {
        if (_apiService.CurrentUser?.Perfil == "DEPENDENTE")
        {
            var dependente = await _apiService.GetMeuDependenteAsync();
            if (dependente == null) return;

            LblNome.Text = dependente.Nome;
            LblCategoria.Text = $"Dependente de {dependente.NomeAssociado}";
            LblCpf.Text = dependente.Cpf ?? "Não informado";
            LblEmail.Text = dependente.Email ?? "Não informado";
            LblTelefone.Text = dependente.Telefone ?? "Não informado";
            LblSituacao.Text = dependente.Ativo ? "Ativo" : "Inativo";
            CardInteresses.IsVisible = false;
            CardDependentes.IsVisible = false;
            return;
        }

        CardInteresses.IsVisible = true;
        CardDependentes.IsVisible = true;
        var perfil = await _apiService.GetMeuPerfilAsync();
        if (perfil != null)
        {
            LblNome.Text = perfil.Nome;
            LblCategoria.Text = $"{perfil.TipoAssociado.Replace("_", " ")} - {(perfil.TemDireitoVoto ? "com direito a voto" : "sem direito a voto")}";
            LblCpf.Text = perfil.Cpf;
            LblEmail.Text = perfil.Email ?? "Não informado";
            LblTelefone.Text = perfil.Telefone ?? "Não informado";
            LblSituacao.Text = perfil.SituacaoCadastro;

            FlexInteresses.Children.Clear();
            if (perfil.Interesses.Count == 0)
            {
                FlexInteresses.Children.Add(new Label { Text = "Nenhum interesse informado.", TextColor = Color.FromArgb("#94a3b8"), FontSize = 12 });
            }
            else
            {
                foreach (var item in perfil.Interesses)
                {
                    var badge = new Border
                    {
                        BackgroundColor = Color.FromArgb("#e0f2fe"),
                        StrokeThickness = 0,
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                        Padding = new Thickness(8, 4),
                        Margin = new Thickness(0, 0, 6, 6),
                        Content = new Label { Text = item, TextColor = Color.FromArgb("#0369a1"), FontSize = 11, FontAttributes = FontAttributes.Bold }
                    };
                    FlexInteresses.Children.Add(badge);
                }
            }

            StackDependentes.Children.Clear();
            if (perfil.Dependentes.Count == 0)
            {
                StackDependentes.Children.Add(new Label { Text = "Nenhum dependente cadastrado.", TextColor = Color.FromArgb("#94a3b8"), FontSize = 12 });
            }
            else
            {
                foreach (var d in perfil.Dependentes)
                {
                    var depCard = new Border
                    {
                        BackgroundColor = Color.FromArgb("#f8fafc"),
                        Stroke = Color.FromArgb("#e2e8f0"),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                        Padding = new Thickness(10),
                        Content = new VerticalStackLayout
                        {
                            Spacing = 2,
                            Children =
                            {
                                new Label { Text = d.Nome, TextColor = Color.FromArgb("#0f172a"), FontAttributes = FontAttributes.Bold, FontSize = 13 },
                                new Label { Text = $"Parentesco: {d.Parentesco} | CPF: {d.Cpf ?? "Não informado"}", TextColor = Color.FromArgb("#64748b"), FontSize = 11 }
                            }
                        }
                    };
                    StackDependentes.Children.Add(depCard);
                }
            }
        }
    }

    private void OnDepAccessToggled(object? sender, ToggledEventArgs e)
    {
        PanelDepAcesso.IsVisible = e.Value;
    }

    private async void OnAddDependenteClicked(object sender, EventArgs e)
    {
        var nome = TxtDepNome.Text?.Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            await DisplayAlert("Atenção", "Informe o nome do dependente.", "OK");
            return;
        }
        if (SwitchDepAcesso.IsToggled &&
            (string.IsNullOrWhiteSpace(TxtDepEmail.Text) || string.IsNullOrWhiteSpace(TxtDepSenha.Text) || TxtDepSenha.Text.Length < 6))
        {
            await DisplayAlert("Atenção", "Informe um e-mail e uma senha de pelo menos 6 caracteres para o acesso ao app.", "OK");
            return;
        }

        var dto = new CreateDependenteDto
        {
            Nome = nome,
            Parentesco = TxtDepParentesco.Text?.Trim() ?? "Outro",
            Cpf = TxtDepCpf.Text?.Trim(),
            Email = TxtDepEmail.Text?.Trim(),
            CriarAcessoApp = SwitchDepAcesso.IsToggled,
            SenhaAcesso = SwitchDepAcesso.IsToggled ? TxtDepSenha.Text : null
        };

        var sucesso = await _apiService.AddDependenteAsync(dto);
        if (sucesso)
        {
            await DisplayAlert("Sucesso", SwitchDepAcesso.IsToggled
                ? "Dependente cadastrado. Ele já pode entrar no aplicativo com o e-mail e a senha informados."
                : "Dependente cadastrado com sucesso!", "OK");
            TxtDepNome.Text = "";
            TxtDepParentesco.Text = "";
            TxtDepCpf.Text = "";
            TxtDepEmail.Text = "";
            TxtDepSenha.Text = "";
            SwitchDepAcesso.IsToggled = false;
            await CarregarPerfil();
        }
        else
        {
            await DisplayAlert("Erro", "Não foi possível cadastrar o dependente.", "OK");
        }
    }
}
