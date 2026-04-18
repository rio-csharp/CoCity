namespace CoCity.Foundation.Services
{
    public sealed class DefaultThemeFoundationService : IThemeFoundationService
    {
        public PresentationTheme CurrentTheme { get; private set; } = PresentationTheme.Day;

        public PresentationTheme SetTheme(PresentationTheme theme)
        {
            CurrentTheme = theme;
            return CurrentTheme;
        }

        public PresentationTheme ToggleTheme()
            => SetTheme(CurrentTheme == PresentationTheme.Day
                ? PresentationTheme.Night
                : PresentationTheme.Day);
    }
}
