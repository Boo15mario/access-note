using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class NotesApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly NotesModule _notesModule;
    private readonly Func<NotesInitialFocusOption> _getInitialFocus;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public NotesApplet(
        ShellViewAdapter shellView,
        NotesModule notesModule,
        Func<NotesInitialFocusOption> getInitialFocus,
        Action<string> announceHint,
        Dispatcher dispatcher)
    {
        _shellView = shellView ?? throw new ArgumentNullException(nameof(shellView));
        _notesModule = notesModule ?? throw new ArgumentNullException(nameof(notesModule));
        _getInitialFocus = getInitialFocus ?? throw new ArgumentNullException(nameof(getInitialFocus));
        _announceHint = announceHint ?? throw new ArgumentNullException(nameof(announceHint));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public AppletDescriptor Descriptor { get; } = new(
        id: AppletId.Notes,
        label: "Notes",
        screenHintText: "Notes.",
        helpText: "Notes. Control N new note, Control S save, F6 cycle focus, Escape to return to menu.",
        startScreenOption: StartScreenOption.Notes);

    public void Enter()
    {
        _shellView.ShowNotesScreen();
        _notesModule.EnterWorkspace(_getInitialFocus());
        _announceHint(Descriptor.ScreenHintText);
    }

    public void RestoreFocus()
    {
        _dispatcher.BeginInvoke(_notesModule.RestoreFocus, DispatcherPriority.Input);
    }

    public bool CanLeave()
    {
        return _notesModule.EnsureCanLeaveActiveNote();
    }

    public bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)
    {
        return _notesModule.HandleInput(key, modifiers);
    }
}
