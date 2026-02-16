using System.Collections.Generic;

namespace AccessNote;

internal static class MainMenuEntryBuilder
{
    public static IReadOnlyList<MainMenuEntry> Build(AppletRegistry appletRegistry)
    {
        var entries = new List<MainMenuEntry>();
        var utilityChildren = new List<MainMenuEntry>();

        foreach (var descriptor in appletRegistry.GetDescriptorsInRegistrationOrder())
        {
            if (descriptor.Category == AppletCategory.Utility)
            {
                utilityChildren.Add(MainMenuEntry.ForApplet(descriptor));
            }
            else
            {
                entries.Add(MainMenuEntry.ForApplet(descriptor));
            }
        }

        if (utilityChildren.Count > 0)
        {
            entries.Add(MainMenuEntry.Submenu("Utilities", utilityChildren));
        }

        entries.Add(MainMenuEntry.Exit());
        return entries;
    }
}
