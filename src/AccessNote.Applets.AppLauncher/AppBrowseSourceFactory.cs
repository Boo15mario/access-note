using System.Collections.Generic;

namespace AccessNote;

internal static class AppBrowseSourceFactory
{
    public static IReadOnlyList<IAppBrowseSource> CreateDefault()
    {
        return new IAppBrowseSource[]
        {
            new StartMenuBrowseSource(),
            new SteamBrowseSource(),
            new XboxBrowseSource(),
            new HeroicBrowseSource(),
        };
    }
}
