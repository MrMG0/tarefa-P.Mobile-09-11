using ClubNet.App.Services;
using ClubNet.App.Views;

namespace ClubNet.App;

public partial class App : Application
{
    private readonly IApiService _apiService;

	public App(IApiService apiService)
	{
		InitializeComponent();
        _apiService = apiService;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
        Page initialPage = _apiService.IsLoggedIn ? new AppShell() : new LoginPage(_apiService);
		return new Window(initialPage);
	}
}
