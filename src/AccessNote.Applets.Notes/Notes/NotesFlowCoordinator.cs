using System;
using System.Windows.Input;

namespace AccessNote;

internal sealed class NotesFlowCoordinator
{
    private readonly Action _focusList;
    private readonly Action _focusEditor;
    private readonly Action _createNewNote;
    private readonly Action _saveActiveNote;
    private readonly Action _focusSearch;
    private readonly Action _cycleFocus;
    private readonly Action _renameActiveNote;
    private readonly Action _deleteActiveNote;
    private readonly Action _handleEscape;

    public NotesFlowCoordinator(
        Action focusList,
        Action focusEditor,
        Action createNewNote,
        Action saveActiveNote,
        Action focusSearch,
        Action cycleFocus,
        Action renameActiveNote,
        Action deleteActiveNote,
        Action handleEscape)
    {
        _focusList = focusList;
        _focusEditor = focusEditor;
        _createNewNote = createNewNote;
        _saveActiveNote = saveActiveNote;
        _focusSearch = focusSearch;
        _cycleFocus = cycleFocus;
        _renameActiveNote = renameActiveNote;
        _deleteActiveNote = deleteActiveNote;
        _handleEscape = handleEscape;
    }

    public bool HandleInput(Key key, ModifierKeys modifiers, bool isFocusInListRegion)
    {
        if (!NotesInputMap.TryGetCommand(key, modifiers, isFocusInListRegion, out var command))
        {
            return false;
        }

        switch (command)
        {
            case NotesInputCommand.FocusList:
                _focusList();
                return true;
            case NotesInputCommand.FocusEditor:
                _focusEditor();
                return true;
            case NotesInputCommand.CreateNewNote:
                _createNewNote();
                return true;
            case NotesInputCommand.SaveActiveNote:
                _saveActiveNote();
                return true;
            case NotesInputCommand.FocusSearch:
                _focusSearch();
                return true;
            case NotesInputCommand.CycleFocus:
                _cycleFocus();
                return true;
            case NotesInputCommand.RenameActiveNote:
                _renameActiveNote();
                return true;
            case NotesInputCommand.DeleteActiveNote:
                _deleteActiveNote();
                return true;
            case NotesInputCommand.Escape:
                _handleEscape();
                return true;
            default:
                return false;
        }
    }
}
