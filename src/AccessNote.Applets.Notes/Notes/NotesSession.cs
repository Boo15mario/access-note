using System;
using System.Collections.Generic;

namespace AccessNote;

internal readonly record struct NotesSaveOutcome(bool Success, Exception? Error = null);

internal readonly record struct NotesDeleteOutcome(NoteDocument? DeletedNote, bool CreatedReplacementBlank);

internal sealed class NotesSession
{
    private readonly INoteStorage _storage;
    private readonly List<NoteDocument> _notes = new();
    private int _newNoteCounter = 1;

    public NotesSession(INoteStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public IReadOnlyList<NoteDocument> Notes => _notes;

    public NoteDocument? ActiveNote { get; private set; }

    public Exception? Load(Func<IEnumerable<NoteDocument>, NoteDocument?> preferredSelector)
    {
        ArgumentNullException.ThrowIfNull(preferredSelector);

        try
        {
            var loaded = _storage.LoadNotes();
            _notes.Clear();
            _notes.AddRange(loaded);
            ActiveNote = SelectPreferredNote(preferredSelector);
            _newNoteCounter = ComputeNextNewNoteCounter();
            return null;
        }
        catch (Exception ex)
        {
            _notes.Clear();
            ActiveNote = null;
            _newNoteCounter = 1;
            return ex;
        }
    }

    public Exception? Persist()
    {
        try
        {
            _storage.SaveNotes(_notes);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public void SetActiveNote(NoteDocument? note)
    {
        ActiveNote = note;
    }

    public void EnsureActiveNoteIsValid(Func<IEnumerable<NoteDocument>, NoteDocument?> preferredSelector)
    {
        ArgumentNullException.ThrowIfNull(preferredSelector);

        if (ActiveNote != null && _notes.Contains(ActiveNote))
        {
            return;
        }

        ActiveNote = SelectPreferredNote(preferredSelector);
    }

    public NoteDocument CreateAndActivateNewNote()
    {
        var note = new NoteDocument($"New note {_newNoteCounter++}", string.Empty);
        _notes.Add(note);
        ActiveNote = note;
        return note;
    }

    public bool EnsureAtLeastOneNote()
    {
        if (_notes.Count > 0)
        {
            return false;
        }

        CreateAndActivateNewNote();
        return true;
    }

    public NotesSaveOutcome SaveActiveNote()
    {
        if (ActiveNote == null)
        {
            return new NotesSaveOutcome(false);
        }

        var previousPersistedTitle = ActiveNote.PersistedTitle;
        var previousPersistedContent = ActiveNote.PersistedContent;
        var previousPersistedLastModifiedUtc = ActiveNote.PersistedLastModifiedUtc;

        ActiveNote.Save();

        var error = Persist();
        if (error != null)
        {
            ActiveNote.RestorePersistedState(
                previousPersistedTitle,
                previousPersistedContent,
                previousPersistedLastModifiedUtc);
            return new NotesSaveOutcome(false, error);
        }

        return new NotesSaveOutcome(true);
    }

    public NotesDeleteOutcome DeleteActiveNote(Func<IEnumerable<NoteDocument>, NoteDocument?> preferredSelector)
    {
        ArgumentNullException.ThrowIfNull(preferredSelector);

        if (ActiveNote == null)
        {
            return new NotesDeleteOutcome(null, false);
        }

        var deleted = ActiveNote;
        _notes.Remove(deleted);

        if (_notes.Count == 0)
        {
            CreateAndActivateNewNote();
            return new NotesDeleteOutcome(deleted, true);
        }

        ActiveNote = SelectPreferredNote(preferredSelector);
        return new NotesDeleteOutcome(deleted, false);
    }

    private NoteDocument? SelectPreferredNote(Func<IEnumerable<NoteDocument>, NoteDocument?> preferredSelector)
    {
        var preferred = preferredSelector(_notes);
        if (preferred != null)
        {
            return preferred;
        }

        return _notes.Count > 0 ? _notes[0] : null;
    }

    private int ComputeNextNewNoteCounter()
    {
        var maxCounter = 0;

        foreach (var note in _notes)
        {
            if (!note.Title.StartsWith("New note ", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var suffix = note.Title["New note ".Length..];
            if (int.TryParse(suffix, out var parsed) && parsed > maxCounter)
            {
                maxCounter = parsed;
            }
        }

        return maxCounter + 1;
    }
}
