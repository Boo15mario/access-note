using System;

namespace AccessNote;

internal sealed class SaveNoteAction
{
    private readonly NotesSession _session;
    private readonly NotesViewStateCoordinator _state;
    private readonly Action _refreshVisibleNotes;
    private readonly Action<Exception> _handlePersistError;
    private readonly Action<string> _announce;

    public SaveNoteAction(
        NotesSession session,
        NotesViewStateCoordinator state,
        Action refreshVisibleNotes,
        Action<Exception> handlePersistError,
        Action<string> announce)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _refreshVisibleNotes = refreshVisibleNotes ?? throw new ArgumentNullException(nameof(refreshVisibleNotes));
        _handlePersistError = handlePersistError ?? throw new ArgumentNullException(nameof(handlePersistError));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public bool Execute(bool announce)
    {
        if (_session.ActiveNote == null)
        {
            return false;
        }

        var outcome = _session.SaveActiveNote();
        if (!outcome.Success)
        {
            if (outcome.Error != null)
            {
                _handlePersistError(outcome.Error);
            }

            _refreshVisibleNotes();
            _state.SelectActiveNoteInList();
            return false;
        }

        _refreshVisibleNotes();
        _state.SelectActiveNoteInList();

        if (announce)
        {
            _announce("Saved.");
        }

        return true;
    }
}
