using System;
using System.Windows.Input;

namespace AccessNote;

internal sealed class ShellInputController
{
    private readonly Func<AppletId?> _getActiveAppletId;
    private readonly Func<Key, bool> _handleHomeScreen;
    private readonly Func<KeyEventArgs, Key, ModifierKeys, bool> _handleActiveAppletInput;
    private readonly Action _showExitPrompt;
    private readonly Func<string> _getHelpText;
    private readonly Action<string> _announce;
    private readonly Action _returnToHomeScreen;
    private readonly Action _showContextMenu;

    public ShellInputController(
        Func<AppletId?> getActiveAppletId,
        Func<Key, bool> handleHomeScreen,
        Func<KeyEventArgs, Key, ModifierKeys, bool> handleActiveAppletInput,
        Action showExitPrompt,
        Func<string> getHelpText,
        Action<string> announce,
        Action returnToHomeScreen,
        Action showContextMenu)
    {
        _getActiveAppletId = getActiveAppletId ?? throw new ArgumentNullException(nameof(getActiveAppletId));
        _handleHomeScreen = handleHomeScreen ?? throw new ArgumentNullException(nameof(handleHomeScreen));
        _handleActiveAppletInput = handleActiveAppletInput ?? throw new ArgumentNullException(nameof(handleActiveAppletInput));
        _showExitPrompt = showExitPrompt ?? throw new ArgumentNullException(nameof(showExitPrompt));
        _getHelpText = getHelpText ?? throw new ArgumentNullException(nameof(getHelpText));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
        _returnToHomeScreen = returnToHomeScreen ?? throw new ArgumentNullException(nameof(returnToHomeScreen));
        _showContextMenu = showContextMenu ?? throw new ArgumentNullException(nameof(showContextMenu));
    }

    public void HandlePreviewKeyDown(KeyEventArgs e)
    {
        var key = InputCommandRouter.NormalizeKey(e.Key, e.SystemKey);
        var modifiers = Keyboard.Modifiers;

        if (InputCommandRouter.TryGetGlobalCommand(key, modifiers, out var globalCommand))
        {
            e.Handled = true;
            ExecuteGlobalCommand(globalCommand);
            return;
        }

        var activeAppletId = _getActiveAppletId();
        var handledByActiveScreen = !activeAppletId.HasValue
            ? _handleHomeScreen(key)
            : _handleActiveAppletInput(e, key, modifiers);

        if (handledByActiveScreen)
        {
            e.Handled = true;
            return;
        }

        if (activeAppletId.HasValue && key == Key.Escape)
        {
            e.Handled = true;
            _returnToHomeScreen();
            return;
        }

        if (InputCommandRouter.ShouldAnnounceIgnoredCommand(key, modifiers))
        {
            e.Handled = true;
            _announce("Command not available.");
        }
    }

    private void ExecuteGlobalCommand(GlobalInputCommand command)
    {
        switch (command)
        {
            case GlobalInputCommand.ShowExitPrompt:
                _showExitPrompt();
                return;
            case GlobalInputCommand.AnnounceHelp:
                _announce(_getHelpText());
                return;
            case GlobalInputCommand.ShowContextMenu:
                _showContextMenu();
                return;
        }
    }
}
