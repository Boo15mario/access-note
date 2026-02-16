using System;

namespace AccessNote;

internal sealed class MediaPlayerAppletRegistration : IAppletRegistration
{
    private readonly MediaPlayerScreenView _screenView;

    public MediaPlayerAppletRegistration(MediaPlayerScreenView screenView)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new MediaPlayerApplet(
            shellView: context.ShellView,
            module: new MediaPlayerModule(),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
