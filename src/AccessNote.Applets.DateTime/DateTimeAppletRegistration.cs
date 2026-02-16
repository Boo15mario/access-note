using System;

namespace AccessNote;

internal sealed class DateTimeAppletRegistration : IAppletRegistration
{
    private readonly DateTimeScreenView _screenView;

    public DateTimeAppletRegistration(DateTimeScreenView screenView)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new DateTimeApplet(
            shellView: context.ShellView,
            module: new DateTimeModule(),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
