using System;

namespace AccessNote;

internal sealed class ContactsAppletRegistration : IAppletRegistration
{
    private readonly ContactsScreenView _screenView;
    private readonly ContactStorage _storage;
    private readonly Action _showMainMenu;

    public ContactsAppletRegistration(
        ContactsScreenView screenView,
        ContactStorage storage,
        Action showMainMenu)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _showMainMenu = showMainMenu ?? throw new ArgumentNullException(nameof(showMainMenu));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var module = new ContactsModule(
            storage: _storage,
            screenView: _screenView,
            announce: context.AnnounceHint,
            dispatcher: context.Dispatcher,
            showMainMenu: _showMainMenu);

        return new ContactsApplet(
            shellView: context.ShellView,
            module: module,
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
