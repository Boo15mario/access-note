namespace AccessNote.Tests;

public sealed class AppLauncherAnnouncementTextTests
{
    [Fact]
    public void EnterScreen_IncludesModeAndCount()
    {
        var text = AppLauncherAnnouncementText.EnterScreen(isBrowseMode: false, count: 3);

        Assert.Equal("App Launcher. Favorites mode. 3 items.", text);
    }

    [Fact]
    public void ModeChanged_Browse_UsesConsistentWording()
    {
        var text = AppLauncherAnnouncementText.ModeChanged(isBrowseMode: true, count: 8);

        Assert.Equal("Browse mode. 8 items.", text);
    }

    [Fact]
    public void DirectoryChanged_IncludesDirectoryAndCount()
    {
        var text = AppLauncherAnnouncementText.DirectoryChanged("Music", 12);

        Assert.Equal("Music. 12 items.", text);
    }

    [Fact]
    public void AddedToFavorites_UsesStandardWording()
    {
        var text = AppLauncherAnnouncementText.AddedToFavorites("Calculator");

        Assert.Equal("Added Calculator to favorites.", text);
    }

    [Fact]
    public void AlreadyInFavorites_UsesStandardWording()
    {
        var text = AppLauncherAnnouncementText.AlreadyInFavorites("Calculator");

        Assert.Equal("Calculator is already in favorites.", text);
    }

    [Fact]
    public void NoFavoriteSelected_UsesStandardWording()
    {
        var text = AppLauncherAnnouncementText.NoFavoriteSelected();

        Assert.Equal("No favorite selected.", text);
    }

    [Fact]
    public void RemovedFromFavorites_UsesStandardWording()
    {
        var text = AppLauncherAnnouncementText.RemovedFromFavorites("Calculator");

        Assert.Equal("Removed Calculator from favorites.", text);
    }

    [Fact]
    public void FilterApplied_WithQuery_UsesStandardWording()
    {
        var text = AppLauncherAnnouncementText.FilterApplied("steam", 2);

        Assert.Equal("Filter steam. 2 items.", text);
    }

    [Fact]
    public void FilterApplied_WithoutQuery_UsesClearedWording()
    {
        var text = AppLauncherAnnouncementText.FilterApplied(string.Empty, 8);

        Assert.Equal("Filter cleared. 8 items.", text);
    }
}
