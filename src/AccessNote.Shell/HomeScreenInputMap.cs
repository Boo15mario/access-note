using System.Windows.Input;

namespace AccessNote;

internal enum HomeScreenInputCommand
{
    MoveUp,
    MoveDown,
    MoveHome,
    MoveEnd,
    ActivateSelection,
    ShowExitPrompt
}

internal static class HomeScreenInputMap
{
    public static bool TryGetCommand(Key key, out HomeScreenInputCommand command)
    {
        switch (key)
        {
            case Key.Up:
                command = HomeScreenInputCommand.MoveUp;
                return true;
            case Key.Down:
                command = HomeScreenInputCommand.MoveDown;
                return true;
            case Key.Home:
                command = HomeScreenInputCommand.MoveHome;
                return true;
            case Key.End:
                command = HomeScreenInputCommand.MoveEnd;
                return true;
            case Key.Enter:
                command = HomeScreenInputCommand.ActivateSelection;
                return true;
            case Key.Escape:
                command = HomeScreenInputCommand.ShowExitPrompt;
                return true;
            default:
                command = default;
                return false;
        }
    }
}
