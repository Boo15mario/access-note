using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class AppLauncherApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly AppLauncherModule _module;
    private readonly AppLauncherScreenView _screenView;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public AppLauncherApplet(
        ShellViewAdapter shellView,
        AppLauncherModule module,
        AppLauncherScreenView screenView,
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
        id: AppletId.AppLauncher,
        label: "App Launcher",
        screenHintText: "App Launcher.",
        helpText: "App Launcher. Enter to launch, Tab to switch modes, Ctrl+N to add favorite, Delete to remove, Escape to return.",
        category: AppletCategory.TopLevel);

    public void Enter()
    {
        _shellView.ShowAppletScreen(AppletId.AppLauncher);
        _module.Enter(
            _screenView.ModeTextControl,
            _screenView.AppListControl,
            _screenView.DetailNameControl,
            _screenView.DetailPathControl,
            _screenView.DetailArgumentsControl);
    }

    public void RestoreFocus()
    {
        _dispatcher.BeginInvoke(_module.RestoreFocus, DispatcherPriority.Input);
    }

    public bool CanLeave()
    {
        return _module.CanLeave();
    }

    public bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)
    {
        return _module.HandleInput(key, modifiers);
    }
}
