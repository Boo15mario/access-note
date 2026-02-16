using System;

namespace AccessNote;

internal sealed class NotesEventController
{
    private readonly NotesModule _notesModule;
    private readonly Func<AppletId?> _getActiveAppletId;
    private readonly Action<Exception> _showNotesLoadError;

    public NotesEventController(
        NotesModule notesModule,
        Func<AppletId?> getActiveAppletId,
        Action<Exception> showNotesLoadError)
    {
        _notesModule = notesModule ?? throw new ArgumentNullException(nameof(notesModule));
        _getActiveAppletId = getActiveAppletId ?? throw new ArgumentNullException(nameof(getActiveAppletId));
        _showNotesLoadError = showNotesLoadError ?? throw new ArgumentNullException(nameof(showNotesLoadError));
    }

    public void HandleSearchTextChanged()
    {
        _notesModule.HandleSearchTextChanged(IsNotesScreen());
    }

    public void HandleSelectionChanged()
    {
        _notesModule.HandleSelectionChanged(IsNotesScreen());
    }

    public void HandleEditorTextChanged()
    {
        _notesModule.HandleEditorTextChanged(IsNotesScreen());
    }

    public void TryLoadPersistedNotes()
    {
        var error = _notesModule.LoadPersistedNotes();
        if (error != null)
        {
            _showNotesLoadError(error);
        }
    }

    public bool TryPersistNotes()
    {
        return _notesModule.TryPersistNotes();
    }

    private bool IsNotesScreen()
    {
        return _getActiveAppletId() == AppletId.Notes;
    }
}
