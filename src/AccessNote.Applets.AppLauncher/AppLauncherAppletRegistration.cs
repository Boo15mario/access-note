using System;
using System.Windows;

namespace AccessNote;

internal sealed class AppLauncherAppletRegistration : IAppletRegistration
{
    private readonly AppLauncherScreenView _screenView;
    private readonly FavoriteAppStorage _storage;
    private readonly Window _owner;

    public AppLauncherAppletRegistration(AppLauncherScreenView screenView, FavoriteAppStorage storage, Window owner)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new AppLauncherApplet(
            shellView: context.ShellView,
            module: new AppLauncherModule(
                storage: _storage,
                dialogs: new AppLauncherDialogService(_owner),
                announce: context.AnnounceHint),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
