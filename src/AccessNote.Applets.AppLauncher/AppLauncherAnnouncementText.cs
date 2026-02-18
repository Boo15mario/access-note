namespace AccessNote;

internal static class AppLauncherAnnouncementText
{
    public static string EnterScreen(bool isBrowseMode, int count)
    {
        var mode = isBrowseMode ? "Browse" : "Favorites";
        return $"App Launcher. {mode} mode. {count} items.";
    }

    public static string ModeChanged(bool isBrowseMode, int count)
    {
        var mode = isBrowseMode ? "Browse" : "Favorites";
        return $"{mode} mode. {count} items.";
    }

    public static string DirectoryChanged(string directoryName, int count)
    {
        return $"{directoryName}. {count} items.";
    }
}
