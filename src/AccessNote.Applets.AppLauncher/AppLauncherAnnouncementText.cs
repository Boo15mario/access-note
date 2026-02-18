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

    public static string FilterApplied(string query, int count)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return $"Filter cleared. {count} items.";
        }

        return $"Filter {query}. {count} items.";
    }

    public static string AddedToFavorites(string displayName)
    {
        return $"Added {displayName} to favorites.";
    }

    public static string AlreadyInFavorites(string displayName)
    {
        return $"{displayName} is already in favorites.";
    }

    public static string CurrentSelectionUnavailable()
    {
        return "Current selection is not a launchable app.";
    }

    public static string InvalidFavoritePath()
    {
        return "Selected app is no longer available.";
    }

    public static string UnsupportedFavoriteSelection()
    {
        return "Selected item cannot be added to favorites.";
    }

    public static string NoFavoriteSelected()
    {
        return "No favorite selected.";
    }

    public static string RemovedFromFavorites(string displayName)
    {
        return $"Removed {displayName} from favorites.";
    }

    public static string Launched(string displayName)
    {
        return $"Launched {displayName}.";
    }

    public static string LaunchFailed(string reason)
    {
        return $"Failed to launch. {reason}";
    }
}
