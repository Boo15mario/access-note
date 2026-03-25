namespace AccessNote;

internal static class NotesAnnouncementText
{
    public static string FocusNotesList()
    {
        return "Notes list.";
    }

    public static string FocusNotesList(int noteCount)
    {
        return noteCount == 0
            ? "Notes list. No notes."
            : $"Notes list. {noteCount} note(s).";
    }

    public static string FocusEditor()
    {
        return "Editor.";
    }

    public static string FocusEditorWithNote(string noteTitle)
    {
        return string.IsNullOrWhiteSpace(noteTitle)
            ? "Editor."
            : $"Editor. {noteTitle}.";
    }

    public static string NoteCreated(string noteTitle)
    {
        return string.IsNullOrWhiteSpace(noteTitle)
            ? "Note created."
            : $"Note created. {noteTitle}.";
    }

    public static string NoteSaved(string noteTitle)
    {
        return string.IsNullOrWhiteSpace(noteTitle)
            ? "Note saved."
            : $"Note saved. {noteTitle}.";
    }

    public static string NoteRenamed(string newTitle)
    {
        return string.IsNullOrWhiteSpace(newTitle)
            ? "Note renamed."
            : $"Note renamed. {newTitle}.";
    }

    public static string NoteDeleted(string noteTitle)
    {
        return string.IsNullOrWhiteSpace(noteTitle)
            ? "Note deleted."
            : $"{noteTitle} deleted.";
    }

    public static string NoteSelected(string noteTitle)
    {
        return string.IsNullOrWhiteSpace(noteTitle)
            ? "Note selected."
            : $"{noteTitle} selected.";
    }

    public static string SearchCleared()
    {
        return "Search cleared.";
    }

    public static string DeleteCanceled()
    {
        return "Delete canceled.";
    }

    public static string RenameCanceled()
    {
        return "Rename canceled.";
    }

    public static string ChangesDiscarded()
    {
        return "Changes discarded.";
    }

    public static string NavigationCanceled()
    {
        return "Navigation canceled.";
    }
}
