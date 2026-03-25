using System;
using System.Collections.Generic;

namespace AccessNote;

internal enum HomeScreenEntryId
{
    Applet,
    Utilities,
    Exit,
    Submenu
}

internal sealed class HomeScreenEntry
{
    private HomeScreenEntry(HomeScreenEntryId id, string label, AppletId? appletId, IReadOnlyList<HomeScreenEntry>? children = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Home screen label cannot be empty.", nameof(label));
        }

        if (id == HomeScreenEntryId.Applet && !appletId.HasValue)
        {
            throw new ArgumentException("Applet menu entry must include an applet id.", nameof(appletId));
        }

        if (id != HomeScreenEntryId.Applet && appletId.HasValue)
        {
            throw new ArgumentException("Only applet menu entries can include an applet id.", nameof(appletId));
        }

        Id = id;
        Label = label;
        AppletId = appletId;
        Children = children ?? Array.Empty<HomeScreenEntry>();
    }

    public static HomeScreenEntry ForApplet(AppletDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        return new HomeScreenEntry(HomeScreenEntryId.Applet, descriptor.Label, descriptor.Id);
    }

    public static HomeScreenEntry Submenu(string label, IReadOnlyList<HomeScreenEntry> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        return new HomeScreenEntry(HomeScreenEntryId.Submenu, label, appletId: null, children: children);
    }

    public static HomeScreenEntry Utilities(string label = "Utilities")
    {
        return new HomeScreenEntry(HomeScreenEntryId.Utilities, label, appletId: null);
    }

    public static HomeScreenEntry Exit(string label = "Exit")
    {
        return new HomeScreenEntry(HomeScreenEntryId.Exit, label, appletId: null);
    }

    public HomeScreenEntryId Id { get; }

    public string Label { get; }

    public AppletId? AppletId { get; }

    public IReadOnlyList<HomeScreenEntry> Children { get; }

    public override string ToString()
    {
        return Label;
    }
}
