using System.Collections.Generic;

namespace AccessNote;

internal static class ShellFeatureFactory
{
    public static StatusAnnouncer CreateStatusAnnouncer(MainWindowShellInputs shell)
    {
        return new StatusAnnouncer(
            shell.StatusText,
            shell.ShouldAnnounceStatusMessages);
    }

    public static ShellViewAdapter CreateShellView(
        MainWindowCoreInputs core,
        MainWindowShellInputs shell)
    {
        var adapter = new ShellViewAdapter(
            shell.HomeScreenScreen,
            shell.HomeScreenList,
            core.Dispatcher);
        // Register the existing applet screens
        foreach (var (appletId, screen) in shell.AppletScreens)
        {
            adapter.RegisterScreen(appletId, screen);
        }
        return adapter;
    }

    public static HomeScreenModule CreateHomeScreenModule(
        ShellViewAdapter shellView,
        IReadOnlyList<HomeScreenEntry> mainMenuEntries,
        MainWindowMenuActionsInputs menuActions,
        StatusAnnouncer statusAnnouncer)
    {
        return new HomeScreenModule(
            shellView,
            mainMenuEntries,
            openApplet: menuActions.OpenApplet,
            showExitPrompt: menuActions.ShowExitPrompt,
            announce: statusAnnouncer.Announce);
    }
}
