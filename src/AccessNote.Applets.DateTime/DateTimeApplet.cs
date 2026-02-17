using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class DateTimeApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly DateTimeModule _module;
    private readonly DateTimeScreenView _screenView;
    private readonly Action<string> _announceHint;
    private readonly Action _returnToMainMenu;
    private readonly Dispatcher _dispatcher;

    public DateTimeApplet(
        ShellViewAdapter shellView,
        DateTimeModule module,
        DateTimeScreenView screenView,
        Action<string> announceHint,
        Action returnToMainMenu,
        Dispatcher dispatcher)
    {
        _shellView = shellView ?? throw new ArgumentNullException(nameof(shellView));
        _module = module ?? throw new ArgumentNullException(nameof(module));
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _announceHint = announceHint ?? throw new ArgumentNullException(nameof(announceHint));
        _returnToMainMenu = returnToMainMenu ?? throw new ArgumentNullException(nameof(returnToMainMenu));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public AppletDescriptor Descriptor { get; } = new(
        id: AppletId.DateTime,
        label: "Date & Time",
        screenHintText: "Date and Time.",
        helpText: "Date and Time. Enter to speak current time, Escape to return to menu.",
        category: AppletCategory.Utility);

    public void Enter()
    {
        _shellView.ShowAppletScreen(AppletId.DateTime);
        _module.Enter(
            _screenView.DateTextControl,
            _screenView.TimeTextControl,
            _screenView.WeekTextControl,
            _announceHint,
            _returnToMainMenu);
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
