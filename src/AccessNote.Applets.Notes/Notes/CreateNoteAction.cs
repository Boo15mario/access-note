using System;

namespace AccessNote;

internal sealed class CreateNoteAction
{
    private readonly NotesSession _session;
    private readonly NotesViewStateCoordinator _state;
    private readonly Action _refreshVisibleNotes;
    private readonly Action _focusEditor;
    private readonly Func<bool> _tryPersistNotes;
    private readonly Action<string> _announce;
    private readonly Func<bool> _ensureCanLeaveActiveNote;

    public CreateNoteAction(
        NotesSession session,
        NotesViewStateCoordinator state,
        Action refreshVisibleNotes,
        Action focusEditor,
        Func<bool> tryPersistNotes,
        Action<string> announce,
        Func<bool> ensureCanLeaveActiveNote)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _refreshVisibleNotes = refreshVisibleNotes ?? throw new ArgumentNullException(nameof(refreshVisibleNotes));
        _focusEditor = focusEditor ?? throw new ArgumentNullException(nameof(focusEditor));
        _tryPersistNotes = tryPersistNotes ?? throw new ArgumentNullException(nameof(tryPersistNotes));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
        _ensureCanLeaveActiveNote = ensureCanLeaveActiveNote ?? throw new ArgumentNullException(nameof(ensureCanLeaveActiveNote));
    }

    public void Execute()
    {
        if (!_ensureCanLeaveActiveNote())
        {
            return;
        }

        _session.CreateAndActivateNewNote();
        _state.SetSearchText(string.Empty);
        _refreshVisibleNotes();
        _state.SelectActiveNoteInList();
        _state.LoadEditorFromActiveNote();
        _focusEditor();
        _tryPersistNotes();
        _announce("New note created.");
    }
}
