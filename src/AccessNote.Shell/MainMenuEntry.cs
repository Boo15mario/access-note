using System;
using System.Collections.Generic;

namespace AccessNote;

internal enum MainMenuEntryId
{
    Applet,
    Utilities,
    Exit,
    Submenu
}

internal sealed class MainMenuEntry
{
    private MainMenuEntry(MainMenuEntryId id, string label, AppletId? appletId, IReadOnlyList<MainMenuEntry>? children = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Main menu label cannot be empty.", nameof(label));
        }

        if (id == MainMenuEntryId.Applet && !appletId.HasValue)
        {
            throw new ArgumentException("Applet menu entry must include an applet id.", nameof(appletId));
        }

        if (id != MainMenuEntryId.Applet && appletId.HasValue)
        {
            throw new ArgumentException("Only applet menu entries can include an applet id.", nameof(appletId));
        }

        Id = id;
        Label = label;
        AppletId = appletId;
        Children = children ?? Array.Empty<MainMenuEntry>();
    }

    public static MainMenuEntry ForApplet(AppletDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        return new MainMenuEntry(MainMenuEntryId.Applet, descriptor.Label, descriptor.Id);
    }

    public static MainMenuEntry Submenu(string label, IReadOnlyList<MainMenuEntry> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        return new MainMenuEntry(MainMenuEntryId.Submenu, label, appletId: null, children: children);
    }

    public static MainMenuEntry Utilities(string label = "Utilities")
    {
        return new MainMenuEntry(MainMenuEntryId.Utilities, label, appletId: null);
    }

    public static MainMenuEntry Exit(string label = "Exit")
    {
        return new MainMenuEntry(MainMenuEntryId.Exit, label, appletId: null);
    }

    public MainMenuEntryId Id { get; }

    public string Label { get; }

    public AppletId? AppletId { get; }

    public IReadOnlyList<MainMenuEntry> Children { get; }

    public override string ToString()
    {
        return Label;
    }
}
