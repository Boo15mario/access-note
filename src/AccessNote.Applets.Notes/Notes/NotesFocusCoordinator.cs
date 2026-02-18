using System;

namespace AccessNote;

internal sealed class NotesFocusCoordinator
{
    private enum FocusRegion
    {
        List,
        Editor,
        Status
    }

    private readonly NotesViewAdapter _view;
    private readonly NotesViewStateCoordinator _state;
    private readonly Action _refreshVisibleNotes;
    private readonly Func<bool> _ensureCanLeaveActiveNote;
    private readonly Action _showMainMenu;
    private readonly Func<string> _getStatusText;
    private readonly Action<string> _announce;

    private FocusRegion _lastFocusRegion = FocusRegion.List;

    public NotesFocusCoordinator(
        NotesViewAdapter view,
        NotesViewStateCoordinator state,
        Action refreshVisibleNotes,
        Func<bool> ensureCanLeaveActiveNote,
        Action showMainMenu,
        Func<string> getStatusText,
        Action<string> announce)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _refreshVisibleNotes = refreshVisibleNotes ?? throw new ArgumentNullException(nameof(refreshVisibleNotes));
        _ensureCanLeaveActiveNote = ensureCanLeaveActiveNote ?? throw new ArgumentNullException(nameof(ensureCanLeaveActiveNote));
        _showMainMenu = showMainMenu ?? throw new ArgumentNullException(nameof(showMainMenu));
        _getStatusText = getStatusText ?? throw new ArgumentNullException(nameof(getStatusText));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public bool IsFocusInListRegion()
    {
        return _view.IsFocusInListRegion;
    }

    public void FocusNotesList()
    {
        _lastFocusRegion = FocusRegion.List;
        _view.FocusNotesListSelectionOrList();
    }

    public void FocusEditor()
    {
        _lastFocusRegion = FocusRegion.Editor;
        _view.FocusEditorToEnd();
    }

    public void FocusSearchBox()
    {
        _lastFocusRegion = FocusRegion.List;
        _view.FocusSearchSelectAll();
        _announce("Search.");
    }

    public void RestoreNotesFocus()
    {
        FocusRegionInternal(_lastFocusRegion);
    }

    public void CycleNotesFocus()
    {
        var current = GetCurrentFocusRegion();
        var next = current switch
        {
            FocusRegion.List => FocusRegion.Editor,
            FocusRegion.Editor => FocusRegion.Status,
            _ => FocusRegion.List,
        };

        FocusRegionInternal(next);

        if (next == FocusRegion.List)
        {
            _announce(NotesAnnouncementText.FocusNotesList());
        }
        else if (next == FocusRegion.Editor)
        {
            _announce(NotesAnnouncementText.FocusEditor());
        }
    }

    public void HandleEscape()
    {
        if (_view.IsSearchFocusedWithin && !string.IsNullOrWhiteSpace(_view.SearchText))
        {
            _state.SetSearchText(string.Empty);
            _refreshVisibleNotes();
            FocusNotesList();
            _announce("Search cleared.");
            return;
        }

        if (!_ensureCanLeaveActiveNote())
        {
            return;
        }

        _showMainMenu();
    }

    private FocusRegion GetCurrentFocusRegion()
    {
        if (_view.IsEditorFocusedWithin)
        {
            return FocusRegion.Editor;
        }

        if (_view.IsStatusFocusedWithin)
        {
            return FocusRegion.Status;
        }

        if (IsFocusInListRegion())
        {
            return FocusRegion.List;
        }

        return _lastFocusRegion;
    }

    private void FocusRegionInternal(FocusRegion region)
    {
        switch (region)
        {
            case FocusRegion.List:
                FocusNotesList();
                return;
            case FocusRegion.Editor:
                FocusEditor();
                return;
            case FocusRegion.Status:
                _lastFocusRegion = FocusRegion.Status;
                _view.FocusStatusRegion();
                _announce(_getStatusText());
                return;
            default:
                return;
        }
    }
}
