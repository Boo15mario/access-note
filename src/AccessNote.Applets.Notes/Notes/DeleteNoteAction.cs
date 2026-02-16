using System;
using System.Collections.Generic;

namespace AccessNote;

internal sealed class DeleteNoteAction
{
    private readonly INotesDialogService _dialogs;
    private readonly NotesSession _session;
    private readonly NotesViewStateCoordinator _state;
    private readonly Func<IEnumerable<NoteDocument>, NoteDocument?> _preferredSelection;
    private readonly Func<bool> _shouldConfirmDelete;
    private readonly Action _refreshVisibleNotes;
    private readonly Action _focusEditor;
    private readonly Action _focusNotesList;
    private readonly Action _restoreNotesFocus;
    private readonly Func<bool> _tryPersistNotes;
    private readonly Action<string> _announce;

    public DeleteNoteAction(
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
        Action<string> announce)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _preferredSelection = preferredSelection ?? throw new ArgumentNullException(nameof(preferredSelection));
        _shouldConfirmDelete = shouldConfirmDelete ?? throw new ArgumentNullException(nameof(shouldConfirmDelete));
        _refreshVisibleNotes = refreshVisibleNotes ?? throw new ArgumentNullException(nameof(refreshVisibleNotes));
        _focusEditor = focusEditor ?? throw new ArgumentNullException(nameof(focusEditor));
        _focusNotesList = focusNotesList ?? throw new ArgumentNullException(nameof(focusNotesList));
        _restoreNotesFocus = restoreNotesFocus ?? throw new ArgumentNullException(nameof(restoreNotesFocus));
        _tryPersistNotes = tryPersistNotes ?? throw new ArgumentNullException(nameof(tryPersistNotes));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public void Execute()
    {
        var activeNote = _session.ActiveNote;
        if (activeNote == null)
        {
            return;
        }

        var shouldDelete = !_shouldConfirmDelete() || _dialogs.ConfirmDelete(activeNote.Title);
        if (!shouldDelete)
        {
            _restoreNotesFocus();
            _announce("Delete canceled.");
            return;
        }

        var outcome = _session.DeleteActiveNote(_preferredSelection);
        if (outcome.DeletedNote == null)
        {
            return;
        }

        if (outcome.CreatedReplacementBlank)
        {
            _state.SetSearchText(string.Empty);
            _refreshVisibleNotes();
            _state.SelectActiveNoteInList();
            _state.LoadEditorFromActiveNote();
            _focusEditor();
            _tryPersistNotes();
            _announce($"{outcome.DeletedNote.Title} deleted.");
            return;
        }

        _refreshVisibleNotes();
        _state.SelectActiveNoteInList();
        _state.LoadEditorFromActiveNote();
        _focusNotesList();
        _tryPersistNotes();
        _announce($"{outcome.DeletedNote.Title} deleted.");
    }
}
