using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class CalculatorApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly CalculatorModule _module;
    private readonly CalculatorScreenView _screenView;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public CalculatorApplet(
        ShellViewAdapter shellView,
        CalculatorModule module,
        CalculatorScreenView screenView,
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
        id: AppletId.Calculator,
        label: "Calculator",
        screenHintText: "Calculator.",
        helpText: "Calculator. Type numbers and operators, Enter to evaluate, Ctrl+M to toggle mode, Escape to return.",
        category: AppletCategory.Utility);

    public void Enter()
    {
        _shellView.ShowAppletScreen(AppletId.Calculator);
        _module.Enter(
            _screenView.ModeTextControl,
            _screenView.ExpressionBoxControl,
            _screenView.ResultTextControl,
            _screenView.HistoryListControl);
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
