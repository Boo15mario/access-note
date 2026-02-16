using System;

namespace AccessNote;

internal sealed class NotesAppletRegistration : IAppletRegistration
{
    private readonly NotesModule _notesModule;
    private readonly Func<NotesInitialFocusOption> _getInitialFocus;

    public NotesAppletRegistration(
        NotesModule notesModule,
        Func<NotesInitialFocusOption> getInitialFocus)
    {
        _notesModule = notesModule ?? throw new ArgumentNullException(nameof(notesModule));
        _getInitialFocus = getInitialFocus ?? throw new ArgumentNullException(nameof(getInitialFocus));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new NotesApplet(
            shellView: context.ShellView,
            notesModule: _notesModule,
            getInitialFocus: _getInitialFocus,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
