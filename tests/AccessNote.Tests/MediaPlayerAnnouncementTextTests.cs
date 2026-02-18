namespace AccessNote.Tests;

public sealed class MediaPlayerAnnouncementTextTests
{
    [Fact]
    public void NoTracksLoaded_ReturnsActionablePrompt()
    {
        var text = MediaPlayerAnnouncementText.NoTracksLoaded();

        Assert.Equal("No tracks loaded. Press O to open a file.", text);
    }

    [Fact]
    public void PlaybackState_WithTrackTitle_IncludesStateAndTrack()
    {
        var text = MediaPlayerAnnouncementText.PlaybackState("Playing", "Song A");

        Assert.Equal("Playing. Song A.", text);
    }

    [Fact]
    public void TrackChanged_UsesConsistentContractWording()
    {
        var text = MediaPlayerAnnouncementText.TrackChanged("Song B", "Playing");

        Assert.Equal("Track changed to Song B. Playing.", text);
    }
}
