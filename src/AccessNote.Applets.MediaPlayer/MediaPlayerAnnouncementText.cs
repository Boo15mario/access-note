using System;

namespace AccessNote;

internal static class MediaPlayerAnnouncementText
{
    public static string NoTracksLoaded()
    {
        return "No tracks loaded. Press O to open a file.";
    }

    public static string PlaybackState(string state, string trackTitle)
    {
        if (string.IsNullOrWhiteSpace(trackTitle))
        {
            return $"{state}.";
        }

        return $"{state}. {trackTitle}.";
    }

    public static string TrackChanged(string trackTitle, string playbackState)
    {
        return $"Track changed to {trackTitle}. {playbackState}.";
    }

    public static string TracksAdded(int count, string trackTitle, string playbackState)
    {
        var noun = count == 1 ? "track" : "tracks";
        if (string.IsNullOrWhiteSpace(trackTitle))
        {
            return $"Added {count} {noun}.";
        }

        return $"Added {count} {noun}. {trackTitle}. {playbackState}.";
    }

    public static string PlaybackFinished()
    {
        return "Playback finished.";
    }
}
