using System;
using System.Collections.Generic;

namespace AccessNote;

internal sealed class NotesActionsCoordinator
{
    private readonly CreateNoteAction _createNoteAction;
    private readonly SaveNoteAction _saveNoteAction;
    private readonly RenameNoteAction _renameNoteAction;
    private readonly DeleteNoteAction _deleteNoteAction;
    private readonly UnsavedNoteGuard _unsavedNoteGuard;

    public NotesActionsCoordinator(
        INotesDialogService dialogs,
        NotesSession session,
        NotesViewStateCoordinator state,
        Func<IEnumerable<NoteDocument>, NoteDocument?> preferredSelection,
        Func<bool> shouldConfirmDelete,
        Action refreshVisibleNotes,
        Action focusEditor,
        Action focusNotesList,
        Action restoreNotesFocus,
        Func<bool> tryPersistNotes,
        Action<Exception> handlePersistError,
        Action<string> announce)
    {
        ArgumentNullException.ThrowIfNull(dialogs);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(preferredSelection);
        ArgumentNullException.ThrowIfNull(shouldConfirmDelete);
        ArgumentNullException.ThrowIfNull(refreshVisibleNotes);
        ArgumentNullException.ThrowIfNull(focusEditor);
        ArgumentNullException.ThrowIfNull(focusNotesList);
        ArgumentNullException.ThrowIfNull(restoreNotesFocus);
        ArgumentNullException.ThrowIfNull(tryPersistNotes);
        ArgumentNullException.ThrowIfNull(handlePersistError);
        ArgumentNullException.ThrowIfNull(announce);

        _saveNoteAction = new SaveNoteAction(
            session: session,
            state: state,
            refreshVisibleNotes: refreshVisibleNotes,
            handlePersistError: handlePersistError,
            announce: announce);

        _unsavedNoteGuard = new UnsavedNoteGuard(
            dialogs: dialogs,
            session: session,
            state: state,
            refreshVisibleNotes: refreshVisibleNotes,
            restoreNotesFocus: restoreNotesFocus,
            saveActiveNoteAndAnnounce: () => _saveNoteAction.Execute(announce: true),
            announce: announce);

        _createNoteAction = new CreateNoteAction(
            session: session,
            state: state,
            refreshVisibleNotes: refreshVisibleNotes,
            focusEditor: focusEditor,
            tryPersistNotes: tryPersistNotes,
            announce: announce,
            ensureCanLeaveActiveNote: _unsavedNoteGuard.EnsureCanLeaveActiveNote);

        _renameNoteAction = new RenameNoteAction(
            dialogs: dialogs,
            session: session,
            state: state,
            refreshVisibleNotes: refreshVisibleNotes,
            restoreNotesFocus: restoreNotesFocus,
            announce: announce);

        _deleteNoteAction = new DeleteNoteAction(
            dialogs: dialogs,
            session: session,
            state: state,
            preferredSelection: preferredSelection,
            shouldConfirmDelete: shouldConfirmDelete,
            refreshVisibleNotes: refreshVisibleNotes,
            focusEditor: focusEditor,
            focusNotesList: focusNotesList,
            restoreNotesFocus: restoreNotesFocus,
            tryPersistNotes: tryPersistNotes,
            announce: announce);
    }

    public void CreateNewNote()
    {
        _createNoteAction.Execute();
    }

    public bool SaveActiveNote(bool announce)
    {
        return _saveNoteAction.Execute(announce);
    }

    public void RenameActiveNote()
    {
        _renameNoteAction.Execute();
    }

    public void DeleteActiveNote()
    {
        _deleteNoteAction.Execute();
    }

    public bool EnsureCanLeaveActiveNote()
    {
        return _unsavedNoteGuard.EnsureCanLeaveActiveNote();
    }
}
