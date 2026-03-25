using System;

namespace AccessNote;

internal sealed class ContactsAppletRegistration : IAppletRegistration
{
    private readonly ContactsScreenView _screenView;
    private readonly ContactStorage _storage;
    private readonly Action _showHomeScreen;

    public ContactsAppletRegistration(
        ContactsScreenView screenView,
        ContactStorage storage,
        Action showHomeScreen)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _showHomeScreen = showHomeScreen ?? throw new ArgumentNullException(nameof(showHomeScreen));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var module = new ContactsModule(
            storage: _storage,
            screenView: _screenView,
            announce: context.AnnounceHint,
            dispatcher: context.Dispatcher,
            showHomeScreen: _showHomeScreen);

        return new ContactsApplet(
            shellView: context.ShellView,
            module: module,
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
