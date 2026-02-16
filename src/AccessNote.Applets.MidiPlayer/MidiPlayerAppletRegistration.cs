using System;

namespace AccessNote;

internal sealed class MidiPlayerAppletRegistration : IAppletRegistration
{
    private readonly MidiPlayerScreenView _screenView;

    public MidiPlayerAppletRegistration(MidiPlayerScreenView screenView)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new MidiPlayerApplet(
            shellView: context.ShellView,
            module: new MidiPlayerModule(),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
