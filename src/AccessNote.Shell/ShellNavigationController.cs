using System;
using System.ComponentModel;

namespace AccessNote;

internal sealed class ShellNavigationController
{
    private readonly HelpTextProvider _helpTextProvider;
    private readonly ScreenRouter _screenRouter;
    private readonly ExitFlowCoordinator _exitFlowCoordinator;

    public ShellNavigationController(
        HelpTextProvider helpTextProvider,
        ScreenRouter screenRouter,
        ExitFlowCoordinator exitFlowCoordinator)
    {
        _helpTextProvider = helpTextProvider ?? throw new ArgumentNullException(nameof(helpTextProvider));
        _screenRouter = screenRouter ?? throw new ArgumentNullException(nameof(screenRouter));
        _exitFlowCoordinator = exitFlowCoordinator ?? throw new ArgumentNullException(nameof(exitFlowCoordinator));
    }

    public void OpenApplet(AppletId appletId)
    {
        _screenRouter.OpenApplet(appletId);
    }

    public void ReturnToMainMenuFromSettings()
    {
        _screenRouter.ShowMainMenu(1, shouldAnnounce: true);
    }

    public void ShowMainMenu(int focusIndex, bool shouldAnnounce)
    {
        _screenRouter.ShowMainMenu(focusIndex, shouldAnnounce);
    }

    public void HandleClosing(CancelEventArgs e)
    {
        _exitFlowCoordinator.HandleClosing(e);
    }

    public void ShowExitPrompt()
    {
        _exitFlowCoordinator.ShowExitPrompt();
    }

    public void RestoreFocusForActiveScreen()
    {
        _screenRouter.RestoreFocusForActiveScreen();
    }

    public string GetHelpText()
    {
        return _helpTextProvider.GetScreenHelpText(_screenRouter.ActiveAppletId);
    }
}
