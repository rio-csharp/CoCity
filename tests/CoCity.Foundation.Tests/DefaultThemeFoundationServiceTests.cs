using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultThemeFoundationServiceTests
{
    [Fact]
    public void ToggleTheme_switches_between_day_and_night()
    {
        var service = new DefaultThemeFoundationService();

        Assert.Equal(PresentationTheme.Day, service.CurrentTheme);
        Assert.Equal(PresentationTheme.Night, service.ToggleTheme());
        Assert.Equal(PresentationTheme.Night, service.CurrentTheme);
        Assert.Equal(PresentationTheme.Day, service.ToggleTheme());
        Assert.Equal(PresentationTheme.Day, service.CurrentTheme);
    }

    [Fact]
    public void SetTheme_updates_current_theme()
    {
        var service = new DefaultThemeFoundationService();

        Assert.Equal(PresentationTheme.Night, service.SetTheme(PresentationTheme.Night));
        Assert.Equal(PresentationTheme.Night, service.CurrentTheme);
    }
}
