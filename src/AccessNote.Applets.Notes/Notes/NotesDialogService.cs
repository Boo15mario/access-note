using System;
using System.Windows;

namespace AccessNote;

internal sealed class NotesDialogService : INotesDialogService
{
    private readonly Window _owner;

    public NotesDialogService(Window owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public bool TryPromptRename(string currentTitle, out string renamedTitle)
    {
        var renameDialog = new RenameNoteDialog(currentTitle)
        {
            Owner = _owner
        };

        var result = renameDialog.ShowDialog();
        if (result == true)
        {
            renamedTitle = renameDialog.NoteTitle;
            return true;
        }

        renamedTitle = string.Empty;
        return false;
    }

    public bool ConfirmDelete(string noteTitle)
    {
        var result = MessageBox.Show(
            _owner,
            $"Delete \"{noteTitle}\"?",
            "Delete Note",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        return result == MessageBoxResult.Yes;
    }

    public UnsavedChangesChoice ShowUnsavedChanges(string noteTitle)
    {
        var dialog = new UnsavedChangesDialog(noteTitle)
        {
            Owner = _owner
        };

        dialog.ShowDialog();
        return dialog.Choice;
    }
}
