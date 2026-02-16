using System;

namespace AccessNote;

internal sealed class UnsavedNoteGuard
{
    private readonly INotesDialogService _dialogs;
    private readonly NotesSession _session;
    private readonly NotesViewStateCoordinator _state;
    private readonly Action _refreshVisibleNotes;
    private readonly Action _restoreNotesFocus;
    private readonly Func<bool> _saveActiveNoteAndAnnounce;
    private readonly Action<string> _announce;

    public UnsavedNoteGuard(
        INotesDialogService dialogs,
        NotesSession session,
        NotesViewStateCoordinator state,
        Action refreshVisibleNotes,
        Action restoreNotesFocus,
        Func<bool> saveActiveNoteAndAnnounce,
        Action<string> announce)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _refreshVisibleNotes = refreshVisibleNotes ?? throw new ArgumentNullException(nameof(refreshVisibleNotes));
        _restoreNotesFocus = restoreNotesFocus ?? throw new ArgumentNullException(nameof(restoreNotesFocus));
        _saveActiveNoteAndAnnounce = saveActiveNoteAndAnnounce ?? throw new ArgumentNullException(nameof(saveActiveNoteAndAnnounce));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public bool EnsureCanLeaveActiveNote()
    {
        var activeNote = _session.ActiveNote;
        if (activeNote == null || !activeNote.IsDirty)
        {
            return true;
        }

        switch (_dialogs.ShowUnsavedChanges(activeNote.Title))
        {
            case UnsavedChangesChoice.Save:
                if (_saveActiveNoteAndAnnounce())
                {
                    return true;
                }

                _restoreNotesFocus();
                return false;
            case UnsavedChangesChoice.Discard:
                activeNote.DiscardChanges();
                _state.LoadEditorFromActiveNote();
                _refreshVisibleNotes();
                _announce("Changes discarded.");
                return true;
            case UnsavedChangesChoice.Cancel:
            default:
                _restoreNotesFocus();
                _announce("Navigation canceled.");
                return false;
        }
    }
}
