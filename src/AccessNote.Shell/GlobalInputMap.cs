using System.Windows.Input;

namespace AccessNote;

internal enum GlobalInputCommand
{
    ShowExitPrompt,
    AnnounceHelp
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

        command = default;
        return false;
    }

    private static bool IsAltF4(Key key, ModifierKeys modifiers)
    {
        return key == Key.F4 && (modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
    }
}
