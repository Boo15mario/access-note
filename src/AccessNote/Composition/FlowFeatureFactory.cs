using System.Collections.Generic;

namespace AccessNote;

internal static class FlowFeatureFactory
{
    public static ScreenRouter CreateScreenRouter(
        MainMenuModule mainMenuModule,
        AppletRegistry appletRegistry)
    {
        return new ScreenRouter(
            appletRegistry: appletRegistry,
            showMainMenu: mainMenuModule.ShowMainMenu,
            restoreMainMenuFocus: mainMenuModule.RestoreFocus);
    }

    public static StartupFlowCoordinator CreateStartupFlowCoordinator(
        MainWindowStartupInputs startup,
        MainWindowShellInputs shell,
        MainWindowNotesInputs notes,
        MainWindowSettingsInputs settings,
        IReadOnlyList<MainMenuEntry> mainMenuEntries,
        SettingsStateCoordinator settingsState,
        AppletRegistry appletRegistry,
        ScreenRouter screenRouter,
        SettingsModule settingsModule,
        ISoundService soundService,
        Action applyTheme)
    {
        var shellStartupBinder = new ShellStartupBinder(
            mainMenuList: shell.MainMenuList,
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
            showMainMenu: () => screenRouter.ShowMainMenu(0, shouldAnnounce: false),
            openApplet: screenRouter.OpenApplet,
            announceMainMenuHint: () => settingsState.AnnounceHint(GetMainMenuDefaultHint(mainMenuEntries)));
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
            showExitConfirmationDialog: shellDialogs.ShowExitConfirmationDialog,
            closeWindow: exit.CloseWindow);
        return new ExitFlowCoordinator(host);
    }

    private static string GetMainMenuDefaultHint(IReadOnlyList<MainMenuEntry> mainMenuEntries)
    {
        foreach (var entry in mainMenuEntries)
        {
            if (entry.AppletId.HasValue)
            {
                return $"Main menu. {entry.Label} selected.";
            }
        }

        return "Main menu.";
    }
}
