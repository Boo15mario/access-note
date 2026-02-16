using System.Windows.Input;

namespace AccessNote;

internal enum NotesInputCommand
{
    FocusList,
    FocusEditor,
    CreateNewNote,
    SaveActiveNote,
    FocusSearch,
    CycleFocus,
    RenameActiveNote,
    DeleteActiveNote,
    Escape
}

internal static class NotesInputMap
{
    public static bool TryGetCommand(
        Key key,
        ModifierKeys modifiers,
        bool isFocusInListRegion,
        out NotesInputCommand command)
    {
        if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            switch (key)
            {
                case Key.L:
                    command = NotesInputCommand.FocusList;
                    return true;
                case Key.E:
                    command = NotesInputCommand.FocusEditor;
                    return true;
                case Key.N:
                    command = NotesInputCommand.CreateNewNote;
                    return true;
                case Key.S:
                    command = NotesInputCommand.SaveActiveNote;
                    return true;
                case Key.F:
                    command = NotesInputCommand.FocusSearch;
                    return true;
            }
        }

        switch (key)
        {
            case Key.F6:
                command = NotesInputCommand.CycleFocus;
                return true;
            case Key.F2 when isFocusInListRegion:
                command = NotesInputCommand.RenameActiveNote;
                return true;
            case Key.Delete when isFocusInListRegion:
                command = NotesInputCommand.DeleteActiveNote;
                return true;
            case Key.Escape:
                command = NotesInputCommand.Escape;
                return true;
            default:
                command = default;
                return false;
        }
    }
}
