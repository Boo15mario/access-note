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
        return new ShellViewAdapter(
            shell.MainMenuScreen,
            shell.NotesScreen,
            shell.SettingsScreen,
            shell.MainMenuList,
            core.Dispatcher);
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
