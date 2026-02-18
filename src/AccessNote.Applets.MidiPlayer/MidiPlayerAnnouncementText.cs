namespace AccessNote;

internal static class MidiPlayerAnnouncementText
{
    public static string NoFileLoaded()
    {
        return "No MIDI file loaded. Press O to open a file.";
    }

    public static string PlaybackState(string state, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return $"{state}.";
        }

        return $"{state}. {fileName}.";
    }

    public static string FileLoaded(string fileName)
    {
        return $"Loaded {fileName}.";
    }

    public static string FileLoadFailed()
    {
        return "Could not load MIDI file.";
    }

    public static string PlaybackFinished()
    {
        return "Playback finished.";
    }
}
