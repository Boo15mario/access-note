using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class SettingsApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly SettingsModule _settingsModule;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public SettingsApplet(
        ShellViewAdapter shellView,
        SettingsModule settingsModule,
        Action<string> announceHint,
        Dispatcher dispatcher)
    {
        _shellView = shellView ?? throw new ArgumentNullException(nameof(shellView));
        _settingsModule = settingsModule ?? throw new ArgumentNullException(nameof(settingsModule));
        _announceHint = announceHint ?? throw new ArgumentNullException(nameof(announceHint));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public AppletDescriptor Descriptor { get; } = new(
        id: AppletId.Settings,
        label: "Settings",
        screenHintText: "Settings. Use category list, options list, and actions.",
        helpText: "Settings. Up and Down navigate, Left and Right change options, Tab changes region, Control S saves, Escape returns.");

    public void Enter()
    {
        _shellView.ShowSettingsScreen();
        _settingsModule.PrepareScreen();
        _dispatcher.BeginInvoke(_settingsModule.FocusRegion, DispatcherPriority.Input);
        _announceHint(Descriptor.ScreenHintText);
    }

    public void RestoreFocus()
    {
        _dispatcher.BeginInvoke(_settingsModule.FocusRegion, DispatcherPriority.Input);
    }

    public bool CanLeave()
    {
        return _settingsModule.EnsureCanLeaveSettings();
    }

    public bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)
    {
        return _settingsModule.HandleInput(e);
    }
}
