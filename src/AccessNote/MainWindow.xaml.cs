using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AccessNote;

public partial class MainWindow : Window
{
    private readonly List<string> _settingsCategories = new(SettingsOptionCatalog.Categories);
    private readonly StartupFlowCoordinator _startupFlowCoordinator;
    private readonly ShellInputController _inputController;
    private readonly ShellNavigationController _navigationController;
    private readonly NotesEventController _notesEventController;
    private readonly SettingsEventController _settingsEventController;
    private readonly SettingsSession _settingsSession;

    public MainWindow()
    {
        var databasePath = NoteStorage.GetDefaultDatabasePath();
        _settingsSession = new SettingsSession(new SettingsStorage(databasePath));
        InitializeComponent();
        var composition = MainWindowCompositionRoot.Create(CreateCompositionInputs(databasePath));
        _startupFlowCoordinator = composition.StartupFlowCoordinator;
        _inputController = composition.InputController;
        _navigationController = composition.NavigationController;
        _notesEventController = composition.NotesEventController;
        _settingsEventController = composition.SettingsEventController;
        NotesScreen.SearchTextChanged = _notesEventController.HandleSearchTextChanged;
        NotesScreen.NotesSelectionChanged = _notesEventController.HandleSelectionChanged;
        NotesScreen.EditorTextChanged = _notesEventController.HandleEditorTextChanged;
        SettingsScreen.CategorySelectionChanged = _settingsEventController.HandleCategorySelectionChanged;
        SettingsScreen.OptionSelectionChanged = _settingsEventController.HandleOptionSelectionChanged;
        SettingsScreen.SaveClick = _settingsEventController.HandleSaveClick;
        SettingsScreen.ResetClick = _settingsEventController.HandleResetClick;
        SettingsScreen.BackClick = _settingsEventController.HandleBackClick;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _startupFlowCoordinator.HandleLoaded();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        _navigationController.HandleClosing(e);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        _inputController.HandlePreviewKeyDown(e);
    }
}
