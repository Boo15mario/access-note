using System;

namespace AccessNote;

internal sealed class DateTimeAppletRegistration : IAppletRegistration
{
    private readonly DateTimeScreenView _screenView;
    private readonly Action _showHomeScreen;

    public DateTimeAppletRegistration(DateTimeScreenView screenView, Action showHomeScreen)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _showHomeScreen = showHomeScreen ?? throw new ArgumentNullException(nameof(showHomeScreen));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new DateTimeApplet(
            shellView: context.ShellView,
            module: new DateTimeModule(),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            returnToHomeScreen: _showHomeScreen,
            dispatcher: context.Dispatcher);
    }
}
