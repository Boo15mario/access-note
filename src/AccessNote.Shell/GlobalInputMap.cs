using System.Windows.Input;

namespace AccessNote;

internal enum GlobalInputCommand
{
    ShowExitPrompt,
    AnnounceHelp,
    ShowContextMenu
}

internal static class GlobalInputMap
{
    public static bool TryGetCommand(Key key, ModifierKeys modifiers, out GlobalInputCommand command)
    {
        if (IsAltF4(key, modifiers))
        {
            command = GlobalInputCommand.ShowExitPrompt;
            return true;
        }

        if (key == Key.F1)
        {
            command = GlobalInputCommand.AnnounceHelp;
            return true;
        }

        // Context menu: Shift+F10 or Applications key (menu key)
        if (key == Key.F10 && (modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            command = GlobalInputCommand.ShowContextMenu;
            return true;
        }

        if (key == Key.LWin || key == Key.RWin)
        {
            command = GlobalInputCommand.ShowContextMenu;
            return true;
        }

        command = default;
        return false;
    }

    private static bool IsAltF4(Key key, ModifierKeys modifiers)
    {
        return key == Key.F4 && (modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
    }
}
