namespace AccessNote;

public enum UnsavedChangesChoice
{
    Save,
    Discard,
    Cancel
}

public interface INotesDialogService
{
    bool TryPromptRename(string currentTitle, out string renamedTitle);

    bool ConfirmDelete(string noteTitle);

    UnsavedChangesChoice ShowUnsavedChanges(string noteTitle);
}

public interface ISettingsDialogService
{
    UnsavedChangesChoice ShowUnsavedSettingsDialog();
}
