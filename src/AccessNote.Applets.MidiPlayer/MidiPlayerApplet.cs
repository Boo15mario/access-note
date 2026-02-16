using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class MidiPlayerApplet : IApplet
{
    private readonly ShellViewAdapter _shellView;
    private readonly MidiPlayerModule _module;
    private readonly MidiPlayerScreenView _screenView;
    private readonly Action<string> _announceHint;
    private readonly Dispatcher _dispatcher;

    public MidiPlayerApplet(
        ShellViewAdapter shellView,
        MidiPlayerModule module,
        MidiPlayerScreenView screenView,
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
        id: AppletId.MidiPlayer,
        label: "MIDI Player",
        screenHintText: "MIDI Player.",
        helpText: "MIDI Player. O to open file, Space to play or pause, S to stop, Plus/Minus to adjust tempo, F3 to load SoundFont, Escape to return.",
        category: AppletCategory.TopLevel);

    public void Enter()
    {
        _shellView.ShowAppletScreen(AppletId.MidiPlayer);
        _module.Enter(
            _screenView.FileNameTextControl,
            _screenView.PlaybackStateTextControl,
            _screenView.ProgressTextControl,
            _screenView.TempoTextControl,
            _screenView.SoundFontTextControl);
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
