namespace AccessNote;

public partial class MainWindow
{
    private MainWindowCompositionInputs CreateCompositionInputs(string databasePath)
    {
        return new MainWindowCompositionInputs
        {
            Core = new MainWindowCoreInputs
            {
                Owner = this,
                DatabasePath = databasePath,
                SettingsSession = _settingsSession,
                Dispatcher = Dispatcher,
            },
            Shell = new MainWindowShellInputs
            {
                MainMenuScreen = MainMenuScreen,
                NotesScreen = NotesScreen,
                SettingsScreen = SettingsScreen,
                MainMenuList = MainMenuList,
                StatusRegion = StatusRegion,
                StatusText = StatusText,
                ShouldAnnounceStatusMessages = () => _settingsSession.Current.AnnounceStatusMessages,
            },
            Notes = new MainWindowNotesInputs
            {
                VisibleNotes = NotesScreen.VisibleNotes,
                NotesList = NotesScreen.NotesListControl,
                NoteSearchBox = NotesScreen.NoteSearchBoxControl,
                EditorTextBox = NotesScreen.EditorTextBoxControl,
                EditorTitleText = NotesScreen.EditorTitleTextControl,
                ShouldConfirmDeleteNote = () => _settingsSession.Current.ConfirmBeforeDeleteNote,
                GetStatusText = () => StatusText.Text,
            },
            Settings = new MainWindowSettingsInputs
            {
                SettingsCategories = _settingsCategories,
                VisibleSettingsOptions = SettingsScreen.VisibleSettingsOptions,
                SettingsCategoryList = SettingsScreen.SettingsCategoryListControl,
                SettingsOptionsList = SettingsScreen.SettingsOptionsListControl,
                SettingsCategoryTitleText = SettingsScreen.SettingsCategoryTitleTextControl,
                SettingsOptionHintText = SettingsScreen.SettingsOptionHintTextControl,
                SettingsSaveButton = SettingsScreen.SettingsSaveButtonControl,
                SettingsResetButton = SettingsScreen.SettingsResetButtonControl,
                SettingsBackButton = SettingsScreen.SettingsBackButtonControl,
            },
            MenuActions = new MainWindowMenuActionsInputs
            {
                OpenApplet = appletId => _navigationController.OpenApplet(appletId),
                ShowExitPrompt = () => _navigationController.ShowExitPrompt(),
            },
            Navigation = new MainWindowNavigationInputs
            {
                ShowMainMenu = (index, announce) => _navigationController.ShowMainMenu(index, announce),
                ReturnToMainMenuFromSettings = () => _navigationController.ReturnToMainMenuFromSettings(),
            },
            Startup = new MainWindowStartupInputs
            {
                GetStartScreen = () => _settingsSession.Current.StartScreen,
                TryLoadPersistedNotes = () => _notesEventController.TryLoadPersistedNotes(),
            },
            Exit = new MainWindowExitInputs
            {
                TryPersistNotes = () => _notesEventController.TryPersistNotes(),
                RestoreFocusForActiveScreen = () => _navigationController.RestoreFocusForActiveScreen(),
                CloseWindow = Close,
            },
        };
    }
}
