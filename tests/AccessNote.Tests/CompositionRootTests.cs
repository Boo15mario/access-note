using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AccessNote.Tests;

public class CompositionRootTests
{
    [Fact]
    public void Create_WithValidInputs_DoesNotThrow()
    {
        StaTestRunner.Run(() =>
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"accessnote_test_{Guid.NewGuid()}");
            try
            {
                var inputs = new MainWindowCompositionInputs
                {
                    Core = new MainWindowCoreInputs
                    {
                        Owner = new Window(),
                        DatabasePath = tempPath,
                        SettingsSession = new SettingsSession(new SettingsStorage(tempPath)),
                        Dispatcher = Dispatcher.CurrentDispatcher,
                    },
                    Shell = new MainWindowShellInputs
                    {
                        MainMenuScreen = new Grid(),
                        NotesScreen = new Grid(),
                        SettingsScreen = new Grid(),
                        MainMenuList = new ListBox(),
                        StatusRegion = new Border(),
                        StatusText = new TextBlock(),
                        ShouldAnnounceStatusMessages = () => false,
                    },
                    Notes = new MainWindowNotesInputs
                    {
                        VisibleNotes = new ObservableCollection<NoteDocument>(),
                        NotesList = new ListBox(),
                        NoteSearchBox = new TextBox(),
                        EditorTextBox = new TextBox(),
                        EditorTitleText = new TextBlock(),
                        ShouldConfirmDeleteNote = () => false,
                        GetStatusText = () => "",
                    },
                    Settings = new MainWindowSettingsInputs
                    {
                        SettingsCategories = SettingsOptionCatalog.Categories,
                        VisibleSettingsOptions = new ObservableCollection<string>(),
                        SettingsCategoryList = new ListBox(),
                        SettingsOptionsList = new ListBox(),
                        SettingsCategoryTitleText = new TextBlock(),
                        SettingsOptionHintText = new TextBlock(),
                        SettingsSaveButton = new Button(),
                        SettingsResetButton = new Button(),
                        SettingsBackButton = new Button(),
                    },
                    MenuActions = new MainWindowMenuActionsInputs
                    {
                        OpenApplet = _ => { },
                        ShowExitPrompt = () => { },
                    },
                    Navigation = new MainWindowNavigationInputs
                    {
                        ShowMainMenu = (_, _) => { },
                        ReturnToMainMenuFromSettings = () => { },
                    },
                    Startup = new MainWindowStartupInputs
                    {
                        GetStartScreen = () => StartScreenOption.MainMenu,
                        TryLoadPersistedNotes = () => { },
                    },
                    Exit = new MainWindowExitInputs
                    {
                        TryPersistNotes = () => true,
                        RestoreFocusForActiveScreen = () => { },
                        CloseWindow = () => { },
                    },
                };

                var ex = Record.Exception(() => MainWindowCompositionRoot.Create(inputs));
                Assert.Null(ex);
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, recursive: true);
            }
        });
    }
}
