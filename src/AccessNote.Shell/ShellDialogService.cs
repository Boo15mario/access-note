using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public ExitOption? ShowExitOptionsDialog()
    {
        var dialog = new ExitOptionsDialog
        {
            Owner = _owner
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            return dialog.SelectedOption;
        }

        return null;
    }

    public string? ShowContextMenuDialog(string appletName, IEnumerable<string> menuItems, Action<string> onCommand)
    {
        string? selectedCommand = null;

        var dialog = new ContextMenuDialog(appletName, menuItems, cmd => selectedCommand = cmd)
        {
            Owner = _owner
        };

        dialog.ShowDialog();
        return selectedCommand;
    }

    public void ShutDownComputer()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/s /t 0",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to shut down: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
