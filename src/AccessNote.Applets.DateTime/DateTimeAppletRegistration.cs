using System;

namespace AccessNote;

internal sealed class DateTimeAppletRegistration : IAppletRegistration
{
    private readonly DateTimeScreenView _screenView;
    private readonly Action _showMainMenu;

    public DateTimeAppletRegistration(DateTimeScreenView screenView, Action showMainMenu)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _showMainMenu = showMainMenu ?? throw new ArgumentNullException(nameof(showMainMenu));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new DateTimeApplet(
            shellView: context.ShellView,
            module: new DateTimeModule(),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            returnToMainMenu: _showMainMenu,
            dispatcher: context.Dispatcher);
    }
}
