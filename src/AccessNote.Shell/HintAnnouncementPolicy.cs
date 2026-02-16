using System;

namespace AccessNote;

internal sealed class HintAnnouncementPolicy
{
    public void AnnounceIfEnabled(bool shouldAnnounceHints, Action<string> announce, string message)
    {
        if (!shouldAnnounceHints)
        {
            return;
        }

        announce(message);
    }
}
