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
}
