using System;

namespace AccessNote;

internal sealed class RenameNoteAction
{
    private readonly INotesDialogService _dialogs;
    private readonly NotesSession _session;
    private readonly NotesViewStateCoordinator _state;
    private readonly Action _refreshVisibleNotes;
    private readonly Action _restoreNotesFocus;
    private readonly Action<string> _announce;

    public RenameNoteAction(
        INotesDialogService dialogs,
        NotesSession session,
        NotesViewStateCoordinator state,
        Action refreshVisibleNotes,
        Action restoreNotesFocus,
        Action<string> announce)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _refreshVisibleNotes = refreshVisibleNotes ?? throw new ArgumentNullException(nameof(refreshVisibleNotes));
        _restoreNotesFocus = restoreNotesFocus ?? throw new ArgumentNullException(nameof(restoreNotesFocus));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public void Execute()
    {
        if (_session.ActiveNote == null)
        {
            return;
        }

        if (!_dialogs.TryPromptRename(_session.ActiveNote.Title, out var renamedTitle))
        {
            _restoreNotesFocus();
            _announce("Rename canceled.");
            return;
        }

        _session.ActiveNote.Title = renamedTitle;
        _refreshVisibleNotes();
        _state.SelectActiveNoteInList();
        _state.LoadEditorFromActiveNote();
        _restoreNotesFocus();
        _announce("Renamed.");
    }
}
