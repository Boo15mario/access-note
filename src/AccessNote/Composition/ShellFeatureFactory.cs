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
            shell.MainMenuScreen,
            shell.MainMenuList,
            core.Dispatcher);
        // Register the existing applet screens
        foreach (var (appletId, screen) in shell.AppletScreens)
        {
            adapter.RegisterScreen(appletId, screen);
        }
        return adapter;
    }

    public static MainMenuModule CreateMainMenuModule(
        ShellViewAdapter shellView,
        IReadOnlyList<MainMenuEntry> mainMenuEntries,
        MainWindowMenuActionsInputs menuActions,
        StatusAnnouncer statusAnnouncer)
    {
        return new MainMenuModule(
            shellView,
            mainMenuEntries,
            openApplet: menuActions.OpenApplet,
            showExitPrompt: menuActions.ShowExitPrompt,
            announce: statusAnnouncer.Announce);
    }
}
