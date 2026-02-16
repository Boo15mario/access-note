using System;
using System.Collections.Generic;
using System.Linq;

namespace AccessNote;

internal static class SettingsOptionCatalog
{
    public static IReadOnlyList<string> Categories { get; } =
    [
        "General",
        "Notes Applet",
        "Accessibility",
        "Advanced"
    ];

    public static IEnumerable<SettingsOptionEntry> GetOptionsForCategory(
        int categoryIndex,
        AppSettings draft,
        Action resetToDefaults)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(resetToDefaults);

        return categoryIndex switch
        {
            0 => GetGeneralOptions(draft),
            1 => GetNotesAppletOptions(draft),
            2 => GetAccessibilityOptions(draft),
            3 => GetAdvancedOptions(resetToDefaults),
            _ => Enumerable.Empty<SettingsOptionEntry>()
        };
    }

    public static bool AreEqual(AppSettings left, AppSettings right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left.StartScreen == right.StartScreen &&
               left.NotesInitialFocus == right.NotesInitialFocus &&
               left.ConfirmBeforeDeleteNote == right.ConfirmBeforeDeleteNote &&
               left.NotesSortOrder == right.NotesSortOrder &&
               left.AnnounceStatusMessages == right.AnnounceStatusMessages &&
               left.AnnounceHintsOnScreenOpen == right.AnnounceHintsOnScreenOpen;
    }

    private static IEnumerable<SettingsOptionEntry> GetGeneralOptions(AppSettings draft)
    {
        return
        [
            new SettingsOptionEntry
            {
                Label = "Start screen",
                Hint = "Left and Right to change startup destination.",
                GetValue = () => draft.StartScreen == StartScreenOption.Notes ? "Notes" : "Main Menu",
                ChangeBy = delta => draft.StartScreen = CycleEnum(draft.StartScreen, delta)
            }
        ];
    }

    private static IEnumerable<SettingsOptionEntry> GetNotesAppletOptions(AppSettings draft)
    {
        return
        [
            new SettingsOptionEntry
            {
                Label = "Auto focus in Notes",
                Hint = "Choose whether Notes opens on list or editor.",
                GetValue = () => draft.NotesInitialFocus == NotesInitialFocusOption.Editor ? "Editor" : "List",
                ChangeBy = delta => draft.NotesInitialFocus = CycleEnum(draft.NotesInitialFocus, delta)
            },
            new SettingsOptionEntry
            {
                Label = "Confirm before deleting note",
                Hint = "Toggle confirmation dialog for note deletion.",
                GetValue = () => draft.ConfirmBeforeDeleteNote ? "On" : "Off",
                ChangeBy = _ => draft.ConfirmBeforeDeleteNote = !draft.ConfirmBeforeDeleteNote
            },
            new SettingsOptionEntry
            {
                Label = "Notes sort order",
                Hint = "Choose how notes are sorted in the list.",
                GetValue = () => draft.NotesSortOrder switch
                {
                    NotesSortOrderOption.LastModifiedOldest => "Last modified (oldest)",
                    NotesSortOrderOption.TitleAscending => "Title (A-Z)",
                    _ => "Last modified (newest)"
                },
                ChangeBy = delta => draft.NotesSortOrder = CycleEnum(draft.NotesSortOrder, delta)
            }
        ];
    }

    private static IEnumerable<SettingsOptionEntry> GetAccessibilityOptions(AppSettings draft)
    {
        return
        [
            new SettingsOptionEntry
            {
                Label = "Announce status messages",
                Hint = "Toggle status region speech announcements.",
                GetValue = () => draft.AnnounceStatusMessages ? "On" : "Off",
                ChangeBy = _ => draft.AnnounceStatusMessages = !draft.AnnounceStatusMessages
            },
            new SettingsOptionEntry
            {
                Label = "Announce hints on screen open",
                Hint = "Toggle introductory hints when a screen opens.",
                GetValue = () => draft.AnnounceHintsOnScreenOpen ? "On" : "Off",
                ChangeBy = _ => draft.AnnounceHintsOnScreenOpen = !draft.AnnounceHintsOnScreenOpen
            }
        ];
    }

    private static IEnumerable<SettingsOptionEntry> GetAdvancedOptions(Action resetToDefaults)
    {
        return
        [
            new SettingsOptionEntry
            {
                Label = "Reset settings to defaults",
                Hint = "Use Enter, Right, or the Reset button to restore defaults.",
                GetValue = () => "Action",
                ChangeBy = _ => resetToDefaults()
            }
        ];
    }

    private static T CycleEnum<T>(T current, int delta) where T : struct, Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        var currentIndex = Array.IndexOf(values, current);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        var nextIndex = ((currentIndex + delta) % values.Length + values.Length) % values.Length;
        return values[nextIndex];
    }
}
