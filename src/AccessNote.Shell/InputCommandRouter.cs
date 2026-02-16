using System.Windows.Input;

namespace AccessNote;

internal static class InputCommandRouter
{
    public static Key NormalizeKey(Key key, Key systemKey)
    {
        return key == Key.System ? systemKey : key;
    }

    public static bool TryGetGlobalCommand(Key key, ModifierKeys modifiers, out GlobalInputCommand command)
    {
        return GlobalInputMap.TryGetCommand(key, modifiers, out command);
    }

    public static bool TryGetMainMenuCommand(Key key, out MainMenuInputCommand command)
    {
        return MainMenuInputMap.TryGetCommand(key, out command);
    }

    public static bool ShouldAnnounceIgnoredCommand(Key key, ModifierKeys modifiers)
    {
        return IgnoredCommandPolicy.ShouldAnnounce(key, modifiers);
    }
}
