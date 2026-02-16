using System;

namespace AccessNote;

internal sealed class AppLauncherAppletRegistration : IAppletRegistration
{
    private readonly AppLauncherScreenView _screenView;
    private readonly FavoriteAppStorage _storage;

    public AppLauncherAppletRegistration(AppLauncherScreenView screenView, FavoriteAppStorage storage)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new AppLauncherApplet(
            shellView: context.ShellView,
            module: new AppLauncherModule(_storage, context.AnnounceHint),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
