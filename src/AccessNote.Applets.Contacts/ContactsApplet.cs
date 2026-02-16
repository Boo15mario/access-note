using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class ContactsApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly ContactsModule _module;
    private readonly ContactsScreenView _screenView;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public ContactsApplet(
        ShellViewAdapter shellView,
        ContactsModule module,
        ContactsScreenView screenView,
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
        id: AppletId.Contacts,
        label: "Contacts",
        screenHintText: "Contacts.",
        helpText: "Contacts. Control N new contact, Control S save, Control I import, Control E export, Delete remove, Escape to return to menu.",
        category: AppletCategory.TopLevel);

    public void Enter()
    {
        _shellView.ShowAppletScreen(AppletId.Contacts);
        _module.Enter();
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
