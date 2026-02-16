using System.Collections.Generic;

namespace AccessNote;

internal static class MainMenuEntryBuilder
{
    public static IReadOnlyList<MainMenuEntry> Build(AppletRegistry appletRegistry)
    {
        var entries = new List<MainMenuEntry>();

        foreach (var descriptor in appletRegistry.GetDescriptorsInRegistrationOrder())
        {
            entries.Add(MainMenuEntry.ForApplet(descriptor));
        }

        entries.Add(MainMenuEntry.Utilities());
        entries.Add(MainMenuEntry.Exit());
        return entries;
    }
}
