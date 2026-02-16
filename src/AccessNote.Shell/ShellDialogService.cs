using System;
using System.Windows;

namespace AccessNote;

internal sealed class ShellDialogService
{
    private readonly Window _owner;

    public ShellDialogService(Window owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public bool? ShowExitConfirmationDialog()
    {
        var dialog = new ExitConfirmationDialog
        {
            Owner = _owner
        };

        return dialog.ShowDialog();
    }
}
