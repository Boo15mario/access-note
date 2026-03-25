using System.Collections.Generic;

namespace AccessNote;

internal static class FlowFeatureFactory
{
    public static ScreenRouter CreateScreenRouter(
        HomeScreenModule mainMenuModule,
        AppletRegistry appletRegistry)
    {
        return new ScreenRouter(
            appletRegistry: appletRegistry,
            showHomeScreen: mainMenuModule.ShowHomeScreen,
            restoreHomeScreenFocus: mainMenuModule.RestoreFocus);
    }

    public static StartupFlowCoordinator CreateStartupFlowCoordinator(
        MainWindowStartupInputs startup,
        MainWindowShellInputs shell,
        MainWindowNotesInputs notes,
        MainWindowSettingsInputs settings,
        IReadOnlyList<HomeScreenEntry> mainMenuEntries,
        SettingsStateCoordinator settingsState,
        AppletRegistry appletRegistry,
        ScreenRouter screenRouter,
        SettingsModule settingsModule,
        ISoundService soundService,
        Action applyTheme)
    {
        var shellStartupBinder = new ShellStartupBinder(
            mainMenuList: shell.HomeScreenList,
            notesList: notes.NotesList,
            settingsCategoryList: settings.SettingsCategoryList,
            settingsOptionsList: settings.SettingsOptionsList,
            mainMenuEntries: mainMenuEntries,
            visibleNotes: notes.VisibleNotes,
            settingsCategories: settings.SettingsCategories,
            visibleSettingsOptions: settings.VisibleSettingsOptions,
            rebuildSettingsOptions: settingsModule.RebuildOptions);
        var host = new StartupHost(
            loadSettings: settingsState.TryLoadSettings,
            applyTheme: applyTheme,
            loadPersistedNotes: startup.TryLoadPersistedNotes,
            prepareShellUi: shellStartupBinder.PrepareShellUi,
            playStartupSound: soundService.PlayStartup,
            getStartScreen: startup.GetStartScreen,
            resolveStartApplet: appletRegistry.ResolveStartAppletId,
            showHomeScreen: () => screenRouter.ShowHomeScreen(0, shouldAnnounce: false),
            openApplet: screenRouter.OpenApplet,
            announceHomeScreenHint: () => settingsState.AnnounceHint(GetHomeScreenDefaultHint(mainMenuEntries)));
        return new StartupFlowCoordinator(host);
    }

    public static ExitFlowCoordinator CreateExitFlowCoordinator(
        MainWindowExitInputs exit,
        StatusAnnouncer statusAnnouncer,
        ScreenRouter screenRouter,
        ShellDialogService shellDialogs)
    {
        var host = new ExitHost(
            canLeaveActiveScreen: screenRouter.CanLeaveActiveScreen,
            tryPersistNotes: exit.TryPersistNotes,
            restoreFocusForActiveScreen: exit.RestoreFocusForActiveScreen,
            announce: statusAnnouncer.Announce,
            showExitOptionsDialog: shellDialogs.ShowExitOptionsDialog,
            shutDownComputer: shellDialogs.ShutDownComputer,
            closeWindow: exit.CloseWindow);
        return new ExitFlowCoordinator(host);
    }

    private static string GetHomeScreenDefaultHint(IReadOnlyList<HomeScreenEntry> mainMenuEntries)
    {
        foreach (var entry in mainMenuEntries)
        {
            if (entry.AppletId.HasValue)
            {
                return $"Home screen. {entry.Label} selected.";
            }
        }

        return "Home screen.";
    }
}
