using System;
using System.Windows;

namespace AccessNote;

internal sealed class ErrorNotifier
{
    private readonly Window _owner;
    private readonly Action<string> _announce;

    public ErrorNotifier(Window owner, Action<string> announce)
    {
        _owner = owner;
        _announce = announce;
    }

    public void ShowSettingsLoadError(Exception error)
    {
        ShowWarning($"Could not load app settings.\n\n{error.Message}");
    }

    public void ShowNotesLoadError(Exception error)
    {
        ShowWarning($"Could not load saved notes.\n\n{error.Message}");
    }

    public void ShowNotesSaveError(Exception error)
    {
        ShowWarning($"Could not save notes.\n\n{error.Message}");
        _announce("Failed to save notes.");
    }

    public void ShowSettingsSaveError(Exception error)
    {
        ShowWarning($"Could not save app settings.\n\n{error.Message}");
        _announce("Failed to save settings.");
    }

    private void ShowWarning(string message)
    {
        MessageBox.Show(
            _owner,
            message,
            "Access Note",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
