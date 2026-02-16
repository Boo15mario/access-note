namespace AccessNote;

internal static class NotesFeatureFactory
{
    public static NotesModule CreateModule(
        MainWindowCoreInputs core,
        MainWindowShellInputs shell,
        MainWindowNotesInputs notes,
        MainWindowNavigationInputs navigation,
        StatusAnnouncer statusAnnouncer,
        ErrorNotifier errorNotifier,
        INotesDialogService notesDialogs,
        SettingsStateCoordinator settingsState)
    {
        return new NotesModule(
            dialogs: notesDialogs,
            session: new NotesSession(new NoteStorage(core.DatabasePath)),
            applySort: settingsState.ApplyNotesSort,
            preferredSelection: settingsState.GetPreferredNoteSelection,
            shouldConfirmDelete: notes.ShouldConfirmDeleteNote,
            visibleNotes: notes.VisibleNotes,
            notesList: notes.NotesList,
            searchBox: notes.NoteSearchBox,
            editorTextBox: notes.EditorTextBox,
            editorTitleText: notes.EditorTitleText,
            statusRegion: shell.StatusRegion,
            statusText: shell.StatusText,
            dispatcher: core.Dispatcher,
            getStatusText: notes.GetStatusText,
            showMainMenu: () => navigation.ShowMainMenu(0, true),
            handlePersistError: errorNotifier.ShowNotesSaveError,
            announce: statusAnnouncer.Announce);
    }
}
