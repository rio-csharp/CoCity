using CoCity.Foundation.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace CoCity
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IThemeFoundationService _themeService;

        public App(IServiceProvider serviceProvider, IThemeFoundationService themeService)
        {
            Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml(this, typeof(App));
            _serviceProvider = serviceProvider;
            _themeService = themeService;
            ApplyPresentationTheme(_themeService.CurrentTheme);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_serviceProvider.GetRequiredService<MainPage>());
        }

        public void ApplyPresentationTheme(PresentationTheme theme)
            => UserAppTheme = theme == PresentationTheme.Day
                ? AppTheme.Light
                : AppTheme.Dark;
    }
}
