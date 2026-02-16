using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace AccessNote;

internal sealed class StatusAnnouncer
{
    private readonly TextBlock _statusText;
    private readonly Func<bool> _shouldRaiseLiveRegion;

    public StatusAnnouncer(TextBlock statusText, Func<bool> shouldRaiseLiveRegion)
    {
        _statusText = statusText ?? throw new ArgumentNullException(nameof(statusText));
        _shouldRaiseLiveRegion = shouldRaiseLiveRegion ?? throw new ArgumentNullException(nameof(shouldRaiseLiveRegion));
    }

    public void Announce(string message)
    {
        _statusText.Text = message;
        AutomationProperties.SetName(_statusText, message);

        if (!_shouldRaiseLiveRegion())
        {
            return;
        }

        var peer = FrameworkElementAutomationPeer.FromElement(_statusText)
                   ?? FrameworkElementAutomationPeer.CreatePeerForElement(_statusText);
        peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }
}
