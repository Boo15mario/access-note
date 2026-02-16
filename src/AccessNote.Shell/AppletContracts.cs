using System;
using System.Windows.Input;

namespace AccessNote;

public enum AppletId
{
    Notes,
    Settings
}

public sealed class AppletDescriptor
{
    public AppletDescriptor(AppletId id, string label)
        : this(
            id: id,
            label: label,
            screenHintText: $"{label}.",
            helpText: "Help is not available.")
    {
    }

    public AppletDescriptor(
        AppletId id,
        string label,
        string screenHintText,
        string helpText,
        StartScreenOption? startScreenOption = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Applet label cannot be empty.", nameof(label));
        }

        if (string.IsNullOrWhiteSpace(screenHintText))
        {
            throw new ArgumentException("Applet screen hint cannot be empty.", nameof(screenHintText));
        }

        if (string.IsNullOrWhiteSpace(helpText))
        {
            throw new ArgumentException("Applet help text cannot be empty.", nameof(helpText));
        }

        Id = id;
        Label = label;
        ScreenHintText = screenHintText;
        HelpText = helpText;
        StartScreenOption = startScreenOption;
    }

    public AppletId Id { get; }

    public string Label { get; }

    public string ScreenHintText { get; }

    public string HelpText { get; }

    public StartScreenOption? StartScreenOption { get; }
}

public interface IApplet
{
    AppletDescriptor Descriptor { get; }

    void Enter();

    void RestoreFocus();

    bool CanLeave();

    bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers);
}
