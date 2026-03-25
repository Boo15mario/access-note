using System;
using System.ComponentModel;

namespace AccessNote;

internal interface IExitHost
{
    bool CanLeaveActiveScreen();
    bool TryPersistNotes();
    void RestoreFocusForActiveScreen();
    void Announce(string message);
    ExitOption? ShowExitOptionsDialog();
    void ShutDownComputer();
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
            var option = _host.ShowExitOptionsDialog();

            if (option == ExitOption.Cancel || !option.HasValue)
            {
                _host.RestoreFocusForActiveScreen();
                _host.Announce("Exit canceled.");
                return;
            }

            if (option == ExitOption.ShutDownComputer)
            {
                _host.ShutDownComputer();
                return;
            }

            // ExitOption.ExitApplication
            if (!_host.TryPersistNotes())
            {
                _host.RestoreFocusForActiveScreen();
                return;
            }

            _allowClose = true;
            _host.CloseWindow();
        }
        finally
        {
            _isPromptingForExit = false;
        }
    }
}
