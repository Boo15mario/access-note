using System;

namespace AccessNote;

internal sealed class SettingsAppletRegistration : IAppletRegistration
{
    private readonly SettingsModule _settingsModule;

    public SettingsAppletRegistration(SettingsModule settingsModule)
    {
        _settingsModule = settingsModule ?? throw new ArgumentNullException(nameof(settingsModule));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new SettingsApplet(
            shellView: context.ShellView,
            settingsModule: _settingsModule,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
