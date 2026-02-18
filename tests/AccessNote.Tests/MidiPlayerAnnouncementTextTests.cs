namespace AccessNote.Tests;

public sealed class MidiPlayerAnnouncementTextTests
{
    [Fact]
    public void NoFileLoaded_ReturnsActionablePrompt()
    {
        var text = MidiPlayerAnnouncementText.NoFileLoaded();

        Assert.Equal("No MIDI file loaded. Press O to open a file.", text);
    }

    [Fact]
    public void PlaybackState_WithFileName_IncludesStateAndFile()
    {
        var text = MidiPlayerAnnouncementText.PlaybackState("Playing", "song.mid");

        Assert.Equal("Playing. song.mid.", text);
    }

    [Fact]
    public void FileLoadFailed_ReturnsConsistentFailureMessage()
    {
        var text = MidiPlayerAnnouncementText.FileLoadFailed();

        Assert.Equal("Could not load MIDI file.", text);
    }
}
