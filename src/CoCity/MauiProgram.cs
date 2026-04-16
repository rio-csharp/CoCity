using CoCity.Foundation.Services;
using CoCity.ViewModels;
using Microsoft.Extensions.Logging;

namespace CoCity
{
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

            builder.Services.AddSingleton<ICoreDataFoundationService, SeedCoreDataFoundationService>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
     		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
