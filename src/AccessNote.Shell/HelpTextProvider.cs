using System;

namespace AccessNote;

internal sealed class HelpTextProvider
{
    private readonly AppletRegistry _appletRegistry;

    public HelpTextProvider(AppletRegistry appletRegistry)
    {
        _appletRegistry = appletRegistry ?? throw new ArgumentNullException(nameof(appletRegistry));
    }

    public string GetScreenHelpText(AppletId? activeAppletId)
    {
        if (!activeAppletId.HasValue)
        {
            return "Main menu. Use Up and Down to move, Enter to activate, Escape to exit.";
        }

        if (_appletRegistry.TryGetDescriptor(activeAppletId.Value, out var descriptor))
        {
            return descriptor.HelpText;
        }

        return "Help is not available.";
    }
}
