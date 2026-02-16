using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class MediaPlayerApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly MediaPlayerModule _module;
    private readonly MediaPlayerScreenView _screenView;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public MediaPlayerApplet(
        ShellViewAdapter shellView,
        MediaPlayerModule module,
        MediaPlayerScreenView screenView,
        Action<string> announceHint,
        Dispatcher dispatcher)
    {
        _shellView = shellView ?? throw new ArgumentNullException(nameof(shellView));
        _module = module ?? throw new ArgumentNullException(nameof(module));
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _announceHint = announceHint ?? throw new ArgumentNullException(nameof(announceHint));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public AppletDescriptor Descriptor { get; } = new(
        id: AppletId.MediaPlayer,
        label: "Media Player",
        screenHintText: "Media Player.",
        helpText: "Media Player. Space play or pause, S stop, N next, P previous, Plus volume up, Minus volume down, M mute, O open file, U add stream URL, Left seek back, Right seek forward, Escape to return.",
        category: AppletCategory.TopLevel);

    public void Enter()
    {
        _shellView.ShowAppletScreen(AppletId.MediaPlayer);
        _module.Enter(
            _screenView.TrackTitleTextControl,
            _screenView.ArtistTextControl,
            _screenView.ProgressTextControl,
            _screenView.VolumeTextControl,
            _screenView.PlaybackStateTextControl,
            _screenView.PlaylistListControl);
        _announceHint(Descriptor.ScreenHintText);
    }

    public void RestoreFocus()
    {
        _dispatcher.BeginInvoke(_module.RestoreFocus, DispatcherPriority.Input);
    }

    public bool CanLeave()
    {
        _module.Stop();
        return _module.CanLeave();
    }

    public bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)
    {
        return _module.HandleInput(key, modifiers);
    }
}
