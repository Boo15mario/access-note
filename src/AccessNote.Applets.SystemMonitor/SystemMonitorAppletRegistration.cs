using System;

namespace AccessNote;

internal sealed class SystemMonitorAppletRegistration : IAppletRegistration
{
    private readonly SystemMonitorScreenView _screenView;

    public SystemMonitorAppletRegistration(SystemMonitorScreenView screenView)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new SystemMonitorApplet(
            shellView: context.ShellView,
            module: new SystemMonitorModule(),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
