using System;
using System.Collections.Generic;
using System.Linq;

namespace AccessNote;

internal sealed class NotesViewStateCoordinator
{
    private readonly NotesSession _session;
    private readonly NotesViewAdapter _view;
    private readonly IList<NoteDocument> _visibleNotes;
    private readonly Func<IEnumerable<NoteDocument>, IEnumerable<NoteDocument>> _applySort;
    private readonly Action<string> _announce;

    public NotesViewStateCoordinator(
        NotesSession session,
        NotesViewAdapter view,
        IList<NoteDocument> visibleNotes,
        Func<IEnumerable<NoteDocument>, IEnumerable<NoteDocument>> applySort,
        Action<string> announce)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _visibleNotes = visibleNotes ?? throw new ArgumentNullException(nameof(visibleNotes));
        _applySort = applySort ?? throw new ArgumentNullException(nameof(applySort));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
    }

    public bool IsSelectionChangeSuppressed { get; private set; }

    public bool IsEditorTextChangeSuppressed { get; private set; }

    public bool IsSearchTextChangeSuppressed { get; private set; }

    public void RefreshVisibleNotes(bool announceCount)
    {
        var search = _view.SearchText.Trim();

        var filtered = _applySort(_session.Notes)
            .Where(note => string.IsNullOrWhiteSpace(search) ||
                           note.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                           note.Content.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _visibleNotes.Clear();
        foreach (var note in filtered)
        {
            _visibleNotes.Add(note);
        }

        SelectActiveNoteInList();

        if (announceCount)
        {
            _announce(filtered.Count == 0 ? "No matching notes." : $"{filtered.Count} notes found.");
        }
    }

    public void SelectActiveNoteInList()
    {
        WithSelectionChangeSuppressed(() =>
        {
            var activeNote = _session.ActiveNote;
            if (activeNote != null && _visibleNotes.Contains(activeNote))
            {
                _view.SelectNote(activeNote);
                return;
            }

            _view.ClearNoteSelection();
        });
    }

    public void RevertSelectionToActiveNote()
    {
        WithSelectionChangeSuppressed(() =>
        {
            var activeNote = _session.ActiveNote;
            if (activeNote != null)
            {
                _view.SelectNote(activeNote);
                return;
            }

            _view.ClearNoteSelection();
        });
    }

    public void LoadEditorFromActiveNote()
    {
        WithEditorTextChangeSuppressed(() =>
        {
            var activeNote = _session.ActiveNote;
            if (activeNote == null)
            {
                _view.SetEditorTitle("Editor");
                _view.SetEditorText(string.Empty);
                return;
            }

            _view.SetEditorTitle($"Editor - {activeNote.Title}");
            _view.SetEditorText(activeNote.Content);
        });
    }

    public void SetSearchText(string text)
    {
        WithSearchTextChangeSuppressed(() => _view.SetSearchText(text));
    }

    public void SyncEditorTextToActiveNote()
    {
        if (_session.ActiveNote == null)
        {
            return;
        }

        _session.ActiveNote.Content = _view.EditorText;
    }

    private void WithSelectionChangeSuppressed(Action action)
    {
        IsSelectionChangeSuppressed = true;
        try
        {
            action();
        }
        finally
        {
            IsSelectionChangeSuppressed = false;
        }
    }

    private void WithEditorTextChangeSuppressed(Action action)
    {
        IsEditorTextChangeSuppressed = true;
        try
        {
            action();
        }
        finally
        {
            IsEditorTextChangeSuppressed = false;
        }
    }

    private void WithSearchTextChangeSuppressed(Action action)
    {
        IsSearchTextChangeSuppressed = true;
        try
        {
            action();
        }
        finally
        {
            IsSearchTextChangeSuppressed = false;
        }
    }
}
