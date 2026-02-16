using System;

namespace AccessNote;

internal sealed class CalendarAppletRegistration : IAppletRegistration
{
    private readonly CalendarScreenView _screenView;
    private readonly CalendarModule _module;

    public CalendarAppletRegistration(CalendarScreenView screenView, CalendarModule module)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _module = module ?? throw new ArgumentNullException(nameof(module));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new CalendarApplet(
            shellView: context.ShellView,
            module: _module,
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
