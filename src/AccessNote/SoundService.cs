using System;
using System.IO;
using System.Media;

namespace AccessNote;

internal sealed class SoundService : ISoundService
{
    private readonly string _soundsDirectory;
    private readonly Func<bool> _isSoundEnabled;

    public SoundService(string soundsDirectory, Func<bool> isSoundEnabled)
    {
        _soundsDirectory = soundsDirectory ?? throw new ArgumentNullException(nameof(soundsDirectory));
        _isSoundEnabled = isSoundEnabled ?? throw new ArgumentNullException(nameof(isSoundEnabled));
    }

    public void PlayStartup()
    {
        PlaySound("login");
    }

    public void PlayVolumeChange()
    {
        PlaySound("vol");
    }

    public void PlaySound(string name)
    {
        if (!_isSoundEnabled())
        {
            return;
        }

        var path = Path.Combine(_soundsDirectory, name + ".wav");
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            using var player = new SoundPlayer(path);
            player.Play();
        }
        catch (Exception)
        {
            // Silently ignore sound playback errors.
        }
    }
}
