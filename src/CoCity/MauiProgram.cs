using CoCity.Foundation.Services;
using CoCity.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

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
            builder.Services.AddSingleton<IMortalRealmSimulationService, DefaultMortalRealmSimulationService>();
            builder.Services.AddSingleton<IMortalIndustrySimulationService, DefaultMortalIndustrySimulationService>();
            builder.Services.AddSingleton<ISectAutonomousOperationsService, DefaultSectAutonomousOperationsService>();
            builder.Services.AddSingleton<IBuildingSystemService, DefaultBuildingSystemService>();
            builder.Services.AddSingleton<IMortalTaxationSimulationService, DefaultMortalTaxationSimulationService>();
            builder.Services.AddSingleton<IMinistryFrameworkService, DefaultMinistryFrameworkService>();
            builder.Services.AddSingleton<IPlayerActionService, DefaultPlayerActionService>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
