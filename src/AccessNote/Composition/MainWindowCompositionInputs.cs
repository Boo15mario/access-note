using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class MainWindowCompositionInputs
{
    public required MainWindowCoreInputs Core { get; init; }
    public required MainWindowShellInputs Shell { get; init; }
    public required MainWindowNotesInputs Notes { get; init; }
    public required MainWindowSettingsInputs Settings { get; init; }
    public required MainWindowMenuActionsInputs MenuActions { get; init; }
    public required MainWindowNavigationInputs Navigation { get; init; }
    public required MainWindowStartupInputs Startup { get; init; }
    public required MainWindowExitInputs Exit { get; init; }
    public required MainWindowDateTimeInputs DateTime { get; init; }
    public required MainWindowCalculatorInputs Calculator { get; init; }
    public required MainWindowMediaPlayerInputs MediaPlayer { get; init; }
    public required MainWindowMidiPlayerInputs MidiPlayer { get; init; }
    public required MainWindowSystemMonitorInputs SystemMonitor { get; init; }
    public required MainWindowCalendarInputs Calendar { get; init; }
    public required MainWindowAppLauncherInputs AppLauncher { get; init; }
    public required MainWindowContactsInputs Contacts { get; init; }
}

internal sealed class MainWindowCoreInputs
{
    public required Window Owner { get; init; }
    public required string DatabasePath { get; init; }
    public required SettingsSession SettingsSession { get; init; }
    public required Dispatcher Dispatcher { get; init; }
}

internal sealed class MainWindowShellInputs
{
    public required FrameworkElement MainMenuScreen { get; init; }
    public required Dictionary<AppletId, FrameworkElement> AppletScreens { get; init; }
    public required ListBox MainMenuList { get; init; }
    public required FrameworkElement StatusRegion { get; init; }
    public required TextBlock StatusText { get; init; }
    public required Func<bool> ShouldAnnounceStatusMessages { get; init; }
}

internal sealed class MainWindowNotesInputs
{
    public required IList<NoteDocument> VisibleNotes { get; init; }
    public required ListBox NotesList { get; init; }
    public required TextBox NoteSearchBox { get; init; }
    public required TextBox EditorTextBox { get; init; }
    public required TextBlock EditorTitleText { get; init; }
    public required Func<bool> ShouldConfirmDeleteNote { get; init; }
    public required Func<string> GetStatusText { get; init; }
}

internal sealed class MainWindowSettingsInputs
{
    public required IReadOnlyList<string> SettingsCategories { get; init; }
    public required ObservableCollection<string> VisibleSettingsOptions { get; init; }
    public required ListBox SettingsCategoryList { get; init; }
    public required ListBox SettingsOptionsList { get; init; }
    public required TextBlock SettingsCategoryTitleText { get; init; }
    public required TextBlock SettingsOptionHintText { get; init; }
    public required Button SettingsSaveButton { get; init; }
    public required Button SettingsResetButton { get; init; }
    public required Button SettingsBackButton { get; init; }
}

internal sealed class MainWindowMenuActionsInputs
{
    public required Action<AppletId> OpenApplet { get; init; }
    public required Action ShowExitPrompt { get; init; }
}

internal sealed class MainWindowNavigationInputs
{
    public required Action<int, bool> ShowMainMenu { get; init; }
    public required Action ReturnToMainMenuFromSettings { get; init; }
}

internal sealed class MainWindowStartupInputs
{
    public required Func<StartScreenOption> GetStartScreen { get; init; }
    public required Action TryLoadPersistedNotes { get; init; }
}

internal sealed class MainWindowExitInputs
{
    public required Func<bool> TryPersistNotes { get; init; }
    public required Action RestoreFocusForActiveScreen { get; init; }
    public required Action CloseWindow { get; init; }
}

internal sealed class MainWindowDateTimeInputs
{
    public required DateTimeScreenView ScreenView { get; init; }
}

internal sealed class MainWindowCalculatorInputs
{
    public required CalculatorScreenView ScreenView { get; init; }
}

internal sealed class MainWindowMediaPlayerInputs
{
    public required MediaPlayerScreenView ScreenView { get; init; }
}

internal sealed class MainWindowMidiPlayerInputs
{
    public required MidiPlayerScreenView ScreenView { get; init; }
}

internal sealed class MainWindowSystemMonitorInputs
{
    public required SystemMonitorScreenView ScreenView { get; init; }
}

internal sealed class MainWindowCalendarInputs
{
    public required CalendarScreenView ScreenView { get; init; }
}

internal sealed class MainWindowAppLauncherInputs
{
    public required AppLauncherScreenView ScreenView { get; init; }
}

internal sealed class MainWindowContactsInputs
{
    public required ContactsScreenView ScreenView { get; init; }
}
