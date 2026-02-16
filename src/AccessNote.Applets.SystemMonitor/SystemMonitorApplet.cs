using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class SystemMonitorApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly SystemMonitorModule _module;
    private readonly SystemMonitorScreenView _screenView;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public SystemMonitorApplet(
        ShellViewAdapter shellView,
        SystemMonitorModule module,
        SystemMonitorScreenView screenView,
        Action<string> announceHint,
        Dispatcher dispatcher)
    {
        _shellView = shellView ?? throw new ArgumentNullException(nameof(shellView));
        _module = module ?? throw new ArgumentNullException(nameof(module));
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _announceHint = announceHint ?? throw new ArgumentNullException(nameof(announceHint));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public AppletDescriptor Descriptor { get; } = new(
        id: AppletId.SystemMonitor,
        label: "System Monitor",
        screenHintText: "System Monitor.",
        helpText: "System Monitor. F5 to refresh, Escape to return to menu.",
        category: AppletCategory.Utility);

    public void Enter()
    {
        _shellView.ShowAppletScreen(AppletId.SystemMonitor);
        _module.Enter(
            _screenView.CpuTextControl,
            _screenView.MemoryTextControl,
            _screenView.DiskListControl);
        _announceHint(Descriptor.ScreenHintText);
    }

    public void RestoreFocus()
    {
        _dispatcher.BeginInvoke(_module.RestoreFocus, DispatcherPriority.Input);
    }

    public bool CanLeave()
    {
        _module.Stop();
        return _module.CanLeave();
    }

    public bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)
    {
        return _module.HandleInput(key, modifiers);
    }
}
