using ClubNet.App.Services;
using ClubNet.App.Views;
using Microsoft.Extensions.Logging;

namespace ClubNet.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Serviços
		builder.Services.AddSingleton<IApiService, ApiService>();

		// Páginas
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<PaymentsPage>();
		builder.Services.AddTransient<EventsPage>();
		builder.Services.AddTransient<ReservationsPage>();
		builder.Services.AddTransient<RequestsPage>();
		builder.Services.AddTransient<ProfilePage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
