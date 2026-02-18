using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AccessNote;

internal static class MainWindowCompositionRoot
{
    public static MainWindowComposition Create(MainWindowCompositionInputs inputs)
    {
        var core = inputs.Core;
        var shell = inputs.Shell;
        var notes = inputs.Notes;
        var settings = inputs.Settings;
        var menuActions = inputs.MenuActions;
        var navigation = inputs.Navigation;
        var startup = inputs.Startup;
        var exit = inputs.Exit;

        var statusAnnouncer = ShellFeatureFactory.CreateStatusAnnouncer(shell);
        var errorNotifier = new ErrorNotifier(core.Owner, statusAnnouncer.Announce);
        var shellDialogs = new ShellDialogService(core.Owner);
        var notesDialogs = new NotesDialogService(core.Owner);
        var settingsDialogs = new SettingsDialogService(core.Owner);
        var shellView = ShellFeatureFactory.CreateShellView(core, shell);
        var notesSortPolicy = new NotesSortPolicy();
        var hintAnnouncementPolicy = new HintAnnouncementPolicy();
        var settingsState = new SettingsStateCoordinator(
            settingsSession: core.SettingsSession,
            notesSortPolicy: notesSortPolicy,
            hintAnnouncementPolicy: hintAnnouncementPolicy,
            errorNotifier: errorNotifier,
            announce: statusAnnouncer.Announce);
        var notesModule = NotesFeatureFactory.CreateModule(core, shell, notes, navigation, statusAnnouncer, errorNotifier, notesDialogs, settingsState);
        var soundService = new SoundService(
            soundsDirectory: System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sounds"),
            isSoundEnabled: () => core.SettingsSession.Current.SoundsEnabled);
        var themesDirectory = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccessNote", "themes");
        var availableThemes = ThemeLoader.LoadAllThemes(themesDirectory);
        Action applyTheme = () =>
        {
            var themeName = core.SettingsSession.Current.ThemeName;
            var theme = ThemeLoader.Resolve(availableThemes, themeName);
            ThemeManager.Apply(theme, System.Windows.Application.Current.Resources);
        };
        var settingsModule = SettingsFeatureFactory.CreateModule(core, settings, navigation, statusAnnouncer, errorNotifier, settingsDialogs, applyTheme);
        IAppletRegistration[] builtInAppletRegistrations =
        {
            new NotesAppletRegistration(
                notesModule: notesModule,
                getInitialFocus: () => core.SettingsSession.Current.NotesInitialFocus),
            new SettingsAppletRegistration(settingsModule: settingsModule),
            new DateTimeAppletRegistration(
                screenView: inputs.DateTime.ScreenView,
                showMainMenu: () => navigation.ShowMainMenu(0, true)),
            new CalculatorAppletRegistration(screenView: inputs.Calculator.ScreenView),
            new MediaPlayerAppletRegistration(screenView: inputs.MediaPlayer.ScreenView),
            new MidiPlayerAppletRegistration(screenView: inputs.MidiPlayer.ScreenView),
            new SystemMonitorAppletRegistration(screenView: inputs.SystemMonitor.ScreenView),
            new AppLauncherAppletRegistration(
                screenView: inputs.AppLauncher.ScreenView,
                storage: new FavoriteAppStorage(core.DatabasePath),
                owner: core.Owner),
            new CalendarAppletRegistration(
                screenView: inputs.Calendar.ScreenView,
                module: new CalendarModule(new CalendarEventStorage(core.DatabasePath))),
            new ContactsAppletRegistration(
                screenView: inputs.Contacts.ScreenView,
                storage: new ContactStorage(core.DatabasePath),
                showMainMenu: () => navigation.ShowMainMenu(0, true)),
        };
        var pluginDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AccessNote",
            "applets");
        var allowlistManifestPath = Path.Combine(pluginDirectoryPath, TrustedAppletRegistrationLoader.DefaultAllowlistFileName);
        var pluginLoader = new TrustedAppletRegistrationLoader();
        var pluginDiscoveryResult = pluginLoader.Discover(pluginDirectoryPath, allowlistManifestPath);
        var startupWarnings = new List<string>(pluginDiscoveryResult.Warnings);
        var registrationContext = new AppletRegistrationContext
        {
            ShellView = shellView,
            AnnounceHint = settingsState.AnnounceHint,
            Dispatcher = core.Dispatcher,
        };

        AppletRegistry appletRegistry;
        if (pluginDiscoveryResult.Registrations.Count == 0)
        {
            appletRegistry = AppletRegistrationComposer.CreateRegistry(
                registrations: builtInAppletRegistrations,
                context: registrationContext);
        }
        else
        {
            try
            {
                appletRegistry = AppletRegistrationComposer.CreateRegistry(
                    registrations: builtInAppletRegistrations.Concat(pluginDiscoveryResult.Registrations),
                    context: registrationContext);
            }
            catch (Exception ex)
            {
                startupWarnings.Add($"Plugin applet composition failed. Falling back to built-in applets only: {ex.Message}");
                appletRegistry = AppletRegistrationComposer.CreateRegistry(
                    registrations: builtInAppletRegistrations,
                    context: registrationContext);
            }
        }

        if (startupWarnings.Count > 0)
        {
            foreach (var warning in startupWarnings)
            {
                Trace.WriteLine($"[AppletPlugin] {warning}");
            }

            statusAnnouncer.Announce($"Some applet plugins were skipped. {startupWarnings.Count} issue(s) found.");
        }

        var mainMenuEntries = MainMenuEntryBuilder.Build(appletRegistry);
        var helpTextProvider = new HelpTextProvider(appletRegistry);
        var mainMenuModule = ShellFeatureFactory.CreateMainMenuModule(shellView, mainMenuEntries, menuActions, statusAnnouncer);
        var screenRouter = FlowFeatureFactory.CreateScreenRouter(mainMenuModule, appletRegistry);
        var startupFlowCoordinator = FlowFeatureFactory.CreateStartupFlowCoordinator(
            startup,
            shell,
            notes,
            settings,
            mainMenuEntries,
            settingsState,
            appletRegistry,
            screenRouter,
            settingsModule,
            soundService,
            applyTheme);
        var exitFlowCoordinator = FlowFeatureFactory.CreateExitFlowCoordinator(exit, statusAnnouncer, screenRouter, shellDialogs);
        var shellNavigationController = new ShellNavigationController(
            helpTextProvider: helpTextProvider,
            screenRouter: screenRouter,
            exitFlowCoordinator: exitFlowCoordinator);
        var shellInputController = InputFeatureFactory.CreateController(
            statusAnnouncer,
            screenRouter,
            mainMenuModule,
            shellNavigationController);
        var notesEventController = new NotesEventController(
            notesModule: notesModule,
            getActiveAppletId: () => screenRouter.ActiveAppletId,
            showNotesLoadError: errorNotifier.ShowNotesLoadError);
        var settingsEventController = new SettingsEventController(
            settingsModule: settingsModule,
            getActiveAppletId: () => screenRouter.ActiveAppletId);

        return new MainWindowComposition(
            StartupFlowCoordinator: startupFlowCoordinator,
            InputController: shellInputController,
            NavigationController: shellNavigationController,
            NotesEventController: notesEventController,
            SettingsEventController: settingsEventController);
    }
}

internal readonly record struct MainWindowComposition(
    StartupFlowCoordinator StartupFlowCoordinator,
    ShellInputController InputController,
    ShellNavigationController NavigationController,
    NotesEventController NotesEventController,
    SettingsEventController SettingsEventController);
