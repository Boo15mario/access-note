using System;
using System.ComponentModel;

namespace AccessNote;

internal interface IExitHost
{
    bool CanLeaveActiveScreen();
    bool TryPersistNotes();
    void RestoreFocusForActiveScreen();
    void Announce(string message);
    bool? ShowExitConfirmationDialog();
    void CloseWindow();
}

internal sealed class ExitFlowCoordinator
{
    private readonly IExitHost _host;

    private bool _allowClose;
    private bool _isPromptingForExit;

    public ExitFlowCoordinator(IExitHost host)
    {
        _host = host;
    }

    public void HandleClosing(CancelEventArgs e)
    {
        if (_allowClose || _isPromptingForExit)
        {
            return;
        }

        e.Cancel = true;
        ShowExitPrompt();
    }

    public void ShowExitPrompt()
    {
        if (_isPromptingForExit)
        {
            return;
        }

        if (!_host.CanLeaveActiveScreen())
        {
            return;
        }

        _isPromptingForExit = true;
        try
        {
            var shouldExit = _host.ShowExitConfirmationDialog();
            if (shouldExit == true)
            {
                if (!_host.TryPersistNotes())
                {
                    _host.RestoreFocusForActiveScreen();
                    return;
                }

                _allowClose = true;
                _host.CloseWindow();
                return;
            }

            _host.RestoreFocusForActiveScreen();
            _host.Announce("Exit canceled.");
        }
        finally
        {
            _isPromptingForExit = false;
        }
    }
}
