using AccessNote;

namespace AccessNote.Tests;

public sealed class HintAnnouncementPolicyTests
{
    [Fact]
    public void AnnounceIfEnabled_WhenDisabled_DoesNotAnnounce()
    {
        var policy = new HintAnnouncementPolicy();
        var announced = new List<string>();

        policy.AnnounceIfEnabled(
            shouldAnnounceHints: false,
            announce: announced.Add,
            message: "Settings.");

        Assert.Empty(announced);
    }

    [Fact]
    public void AnnounceIfEnabled_WhenEnabled_AnnouncesMessage()
    {
        var policy = new HintAnnouncementPolicy();
        var announced = new List<string>();

        policy.AnnounceIfEnabled(
            shouldAnnounceHints: true,
            announce: announced.Add,
            message: "Notes.");

        Assert.Equal(new[] { "Notes." }, announced);
    }
}
