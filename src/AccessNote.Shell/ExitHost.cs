using System;

namespace AccessNote;

internal sealed class ExitHost : IExitHost
{
    private readonly Func<bool> _canLeaveActiveScreen;
    private readonly Func<bool> _tryPersistNotes;
    private readonly Action _restoreFocusForActiveScreen;
    private readonly Action<string> _announce;
    private readonly Func<bool?> _showExitConfirmationDialog;
    private readonly Action _closeWindow;

    public ExitHost(
        Func<bool> canLeaveActiveScreen,
        Func<bool> tryPersistNotes,
        Action restoreFocusForActiveScreen,
        Action<string> announce,
        Func<bool?> showExitConfirmationDialog,
        Action closeWindow)
    {
        _canLeaveActiveScreen = canLeaveActiveScreen;
        _tryPersistNotes = tryPersistNotes;
        _restoreFocusForActiveScreen = restoreFocusForActiveScreen;
        _announce = announce;
        _showExitConfirmationDialog = showExitConfirmationDialog;
        _closeWindow = closeWindow;
    }

    public bool CanLeaveActiveScreen() => _canLeaveActiveScreen();

    public bool TryPersistNotes() => _tryPersistNotes();

    public void RestoreFocusForActiveScreen() => _restoreFocusForActiveScreen();

    public void Announce(string message) => _announce(message);

    public bool? ShowExitConfirmationDialog() => _showExitConfirmationDialog();

    public void CloseWindow() => _closeWindow();
}
