using System.Windows.Input;

namespace AccessNote;

internal static class IgnoredCommandPolicy
{
    public static bool ShouldAnnounce(Key key, ModifierKeys modifiers)
    {
        if (IsModifierKey(key))
        {
            return false;
        }

        if ((modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.None)
        {
            return true;
        }

        return key >= Key.F1 && key <= Key.F24;
    }

    private static bool IsModifierKey(Key key)
    {
        return key is Key.LeftCtrl
            or Key.RightCtrl
            or Key.LeftAlt
            or Key.RightAlt
            or Key.LeftShift
            or Key.RightShift
            or Key.LWin
            or Key.RWin;
    }
}
