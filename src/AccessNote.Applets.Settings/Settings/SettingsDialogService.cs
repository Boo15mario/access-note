using System;
using System.Windows;

namespace AccessNote;

internal sealed class SettingsDialogService : ISettingsDialogService
{
    private readonly Window _owner;

    public SettingsDialogService(Window owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public UnsavedChangesChoice ShowUnsavedSettingsDialog()
    {
        var dialog = new UnsavedSettingsDialog
        {
            Owner = _owner
        };

        dialog.ShowDialog();
        return dialog.Choice;
    }
}
