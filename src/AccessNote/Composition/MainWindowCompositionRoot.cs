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
        var settingsModule = SettingsFeatureFactory.CreateModule(core, settings, navigation, statusAnnouncer, errorNotifier, settingsDialogs);
        IAppletRegistration[] appletRegistrations =
        {
            new NotesAppletRegistration(
                notesModule: notesModule,
                getInitialFocus: () => core.SettingsSession.Current.NotesInitialFocus),
            new SettingsAppletRegistration(settingsModule: settingsModule),
        };
        var appletRegistry = AppletRegistrationComposer.CreateRegistry(
            registrations: appletRegistrations,
            context: new AppletRegistrationContext
            {
                ShellView = shellView,
                AnnounceHint = settingsState.AnnounceHint,
                Dispatcher = core.Dispatcher,
            });
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
            settingsModule);
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
