using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace AccessNote;

public interface IAppletRegistration
{
    IApplet Create(AppletRegistrationContext context);
}

public sealed class AppletRegistrationContext
{
    public required ShellViewAdapter ShellView { get; init; }
    public required Action<string> AnnounceHint { get; init; }
    public required Dispatcher Dispatcher { get; init; }
}

internal static class AppletRegistrationComposer
{
    public static AppletRegistry CreateRegistry(
        IEnumerable<IAppletRegistration> registrations,
        AppletRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        ArgumentNullException.ThrowIfNull(context);

        var applets = new List<IApplet>();
        foreach (var registration in registrations)
        {
            ArgumentNullException.ThrowIfNull(registration);
            applets.Add(registration.Create(context));
        }

        return new AppletRegistry(applets);
    }
}
