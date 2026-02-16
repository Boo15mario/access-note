using System;

namespace AccessNote;

internal sealed class CalculatorAppletRegistration : IAppletRegistration
{
    private readonly CalculatorScreenView _screenView;

    public CalculatorAppletRegistration(CalculatorScreenView screenView)
    {
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
    }

    public IApplet Create(AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new CalculatorApplet(
            shellView: context.ShellView,
            module: new CalculatorModule(),
            screenView: _screenView,
            announceHint: context.AnnounceHint,
            dispatcher: context.Dispatcher);
    }
}
