using System.Windows.Input;

namespace AccessNote;

internal enum MainMenuInputCommand
{
    MoveUp,
    MoveDown,
    MoveHome,
    MoveEnd,
    ActivateSelection,
    ShowExitPrompt
}

internal static class MainMenuInputMap
{
    public static bool TryGetCommand(Key key, out MainMenuInputCommand command)
    {
        switch (key)
        {
            case Key.Up:
                command = MainMenuInputCommand.MoveUp;
                return true;
            case Key.Down:
                command = MainMenuInputCommand.MoveDown;
                return true;
            case Key.Home:
                command = MainMenuInputCommand.MoveHome;
                return true;
            case Key.End:
                command = MainMenuInputCommand.MoveEnd;
                return true;
            case Key.Enter:
                command = MainMenuInputCommand.ActivateSelection;
                return true;
            case Key.Escape:
                command = MainMenuInputCommand.ShowExitPrompt;
                return true;
            default:
                command = default;
                return false;
        }
    }
}
