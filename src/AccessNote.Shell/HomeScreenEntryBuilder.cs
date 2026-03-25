using System.Collections.Generic;

namespace AccessNote;

internal static class HomeScreenEntryBuilder
{
    public static IReadOnlyList<HomeScreenEntry> Build(AppletRegistry appletRegistry)
    {
        var entries = new List<HomeScreenEntry>();
        var mediaChildren = new List<HomeScreenEntry>();
        var utilityChildren = new List<HomeScreenEntry>();

        foreach (var descriptor in appletRegistry.GetDescriptorsInRegistrationOrder())
        {
            switch (descriptor.Id)
            {
                case AppletId.MediaPlayer:
                case AppletId.MidiPlayer:
                    mediaChildren.Add(HomeScreenEntry.ForApplet(descriptor));
                    break;
                default:
                    if (descriptor.Category == AppletCategory.Utility)
                    {
                        utilityChildren.Add(HomeScreenEntry.ForApplet(descriptor));
                    }
                    else
                    {
                        entries.Add(HomeScreenEntry.ForApplet(descriptor));
                    }
                    break;
            }
        }

        if (mediaChildren.Count > 0)
        {
            entries.Add(HomeScreenEntry.Submenu("Media Center", mediaChildren));
        }

        if (utilityChildren.Count > 0)
        {
            entries.Add(HomeScreenEntry.Submenu("Utilities", utilityChildren));
        }

        entries.Add(HomeScreenEntry.Exit());
        return entries;
    }
}
