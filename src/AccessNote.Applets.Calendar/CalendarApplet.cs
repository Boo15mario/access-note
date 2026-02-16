using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class CalendarApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly CalendarModule _module;
    private readonly CalendarScreenView _screenView;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public CalendarApplet(
        ShellViewAdapter shellView,
        CalendarModule module,
        CalendarScreenView screenView,
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
        id: AppletId.Calendar,
        label: "Calendar",
        screenHintText: "Calendar.",
        helpText: "Calendar. Arrow keys navigate days, PageUp PageDown change month, Enter view events, Control N new event, Delete remove event, F6 cycle focus, Escape to return to menu.",
        category: AppletCategory.TopLevel);

    public void Enter()
    {
        _shellView.ShowAppletScreen(AppletId.Calendar);
        _module.Enter(
            _screenView.MonthYearHeaderControl,
            _screenView.DayCellsListControl,
            _screenView.SelectedDateTextControl,
            _screenView.EventsListControl,
            _screenView.EventDetailTextControl);
        _announceHint(Descriptor.ScreenHintText);
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
