namespace CoCity.Foundation.Services
{
    public enum PresentationTheme
    {
        Day,
        Night
    }

    public interface IThemeFoundationService
    {
        PresentationTheme CurrentTheme { get; }

        PresentationTheme SetTheme(PresentationTheme theme);

        PresentationTheme ToggleTheme();
    }
}
