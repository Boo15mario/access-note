using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NAudio.Wave;

namespace AccessNote;

internal sealed class MediaPlayerModule
{
    private TextBlock? _trackTitleText;
    private TextBlock? _artistText;
    private TextBlock? _progressText;
    private TextBlock? _volumeText;
    private TextBlock? _playbackStateText;
    private ListBox? _playlistList;

    private WaveOutEvent? _waveOut;
    private WaveStream? _audioStream;
    private DispatcherTimer? _timer;

    private readonly List<string> _playlist = new();
    private int _currentTrackIndex = -1;
    private float _volume = 1.0f;
    private float _volumeBeforeMute = 1.0f;
    private bool _isMuted;

    private Action<string>? _announce;

    public void Enter(
        TextBlock trackTitleText,
        TextBlock artistText,
        TextBlock progressText,
        TextBlock volumeText,
        TextBlock playbackStateText,
        ListBox playlistList,
        Action<string> announce)
    {
        _trackTitleText = trackTitleText;
        _artistText = artistText;
        _progressText = progressText;
        _volumeText = volumeText;
        _playbackStateText = playbackStateText;
        _playlistList = playlistList;
        _announce = announce;

        UpdatePlaylistDisplay();
        UpdateVolumeDisplay();
        UpdatePlaybackStateDisplay();
        UpdateTrackInfoDisplay();

        var status = _playlist.Count == 0
            ? "Media Player. No tracks loaded. Press O to open a file."
            : $"Media Player. {_playbackStateText.Text}. {_playlist.Count} tracks.";
        _announce(status);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    public void RestoreFocus()
    {
        _playlistList?.Focus();
    }

    public bool CanLeave()
    {
        return true;
    }

    public void Stop()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }

        StopPlayback();
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        if (modifiers != ModifierKeys.None)
            return false;

        switch (key)
        {
            case Key.Space:
                if (_playlist.Count == 0 && _waveOut == null)
                {
                    _announce?.Invoke(MediaPlayerAnnouncementText.NoTracksLoaded());
                    return true;
                }

                TogglePlayPause();
                AnnouncePlaybackState();
                return true;

            case Key.S:
                StopPlayback();
                UpdatePlaybackStateDisplay();
                UpdateTrackInfoDisplay();
                UpdateProgressDisplay();
                AnnouncePlaybackState();
                return true;

            case Key.N:
                if (_playlist.Count == 0)
                {
                    _announce?.Invoke(MediaPlayerAnnouncementText.NoTracksLoaded());
                    return true;
                }

                NextTrack();
                AnnounceTrackChanged();
                return true;

            case Key.P:
                if (_playlist.Count == 0)
                {
                    _announce?.Invoke(MediaPlayerAnnouncementText.NoTracksLoaded());
                    return true;
                }

                PreviousTrack();
                AnnounceTrackChanged();
                return true;

            case Key.Add:
            case Key.OemPlus:
                AdjustVolume(0.1f);
                _announce?.Invoke(_volumeText?.Text ?? "");
                return true;

            case Key.Subtract:
            case Key.OemMinus:
                AdjustVolume(-0.1f);
                _announce?.Invoke(_volumeText?.Text ?? "");
                return true;

            case Key.M:
                ToggleMute();
                _announce?.Invoke(_volumeText?.Text ?? "");
                return true;

            case Key.Left:
                Seek(-5);
                _announce?.Invoke(_progressText?.Text ?? "");
                return true;

            case Key.Right:
                Seek(5);
                _announce?.Invoke(_progressText?.Text ?? "");
                return true;

            case Key.T:
                AnnounceTrackInfo();
                return true;

            case Key.O:
                OpenFileDialog();
                return true;

            case Key.U:
                AddStreamUrl();
                return true;

            default:
                return false;
        }
    }

    private void TogglePlayPause()
    {
        if (_waveOut == null)
        {
            if (_playlist.Count == 0) return;

            if (_currentTrackIndex < 0)
                _currentTrackIndex = 0;

            PlayCurrentTrack();
            return;
        }

        if (_waveOut.PlaybackState == PlaybackState.Playing)
        {
            _waveOut.Pause();
        }
        else if (_waveOut.PlaybackState == PlaybackState.Paused)
        {
            _waveOut.Play();
        }
        else
        {
            if (_playlist.Count > 0)
            {
                if (_currentTrackIndex < 0)
                    _currentTrackIndex = 0;
                PlayCurrentTrack();
            }
        }

        UpdatePlaybackStateDisplay();
    }

    private void StopPlayback()
    {
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }

        if (_audioStream != null)
        {
            _audioStream.Dispose();
            _audioStream = null;
        }
    }

    private void PlayCurrentTrack()
    {
        if (_currentTrackIndex < 0 || _currentTrackIndex >= _playlist.Count)
            return;

        StopPlayback();

        var path = _playlist[_currentTrackIndex];

        try
        {
            if (IsStreamUrl(path))
            {
                _audioStream = new MediaFoundationReader(path);
            }
            else
            {
                _audioStream = new AudioFileReader(path);
            }

            _waveOut = new WaveOutEvent();
            _waveOut.Init(_audioStream);
            _waveOut.Volume = _isMuted ? 0f : _volume;
            _waveOut.PlaybackStopped += OnPlaybackStopped;
            _waveOut.Play();
        }
        catch
        {
            StopPlayback();
        }

        UpdateTrackInfoDisplay();
        UpdatePlaybackStateDisplay();
        UpdateProgressDisplay();
        UpdatePlaylistSelection();
    }

    private void NextTrack()
    {
        if (_playlist.Count == 0) return;

        _currentTrackIndex = (_currentTrackIndex + 1) % _playlist.Count;
        PlayCurrentTrack();
    }

    private void PreviousTrack()
    {
        if (_playlist.Count == 0) return;

        _currentTrackIndex--;
        if (_currentTrackIndex < 0)
            _currentTrackIndex = _playlist.Count - 1;

        PlayCurrentTrack();
    }

    private void AdjustVolume(float delta)
    {
        _volume = Math.Clamp(_volume + delta, 0f, 1f);
        _isMuted = false;

        if (_waveOut != null)
            _waveOut.Volume = _volume;

        UpdateVolumeDisplay();
    }

    private void ToggleMute()
    {
        if (_isMuted)
        {
            _isMuted = false;
            _volume = _volumeBeforeMute;
        }
        else
        {
            _volumeBeforeMute = _volume;
            _isMuted = true;
        }

        if (_waveOut != null)
            _waveOut.Volume = _isMuted ? 0f : _volume;

        UpdateVolumeDisplay();
    }

    private void Seek(int seconds)
    {
        if (_audioStream == null || !_audioStream.CanSeek) return;

        var newPosition = _audioStream.CurrentTime.Add(TimeSpan.FromSeconds(seconds));

        if (newPosition < TimeSpan.Zero)
            newPosition = TimeSpan.Zero;

        if (newPosition > _audioStream.TotalTime)
            newPosition = _audioStream.TotalTime;

        _audioStream.CurrentTime = newPosition;
        UpdateProgressDisplay();
    }

    private void OpenFileDialog()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Audio Files|*.mp3;*.wav;*.wma;*.aac;*.ogg;*.flac;*.m4a|All Files|*.*",
            Multiselect = true,
            Title = "Select audio files",
        };

        if (dialog.ShowDialog() == true)
        {
            var addedCount = 0;
            foreach (var file in dialog.FileNames)
            {
                _playlist.Add(file);
                addedCount++;
            }

            UpdatePlaylistDisplay();

            if (_currentTrackIndex < 0 && _playlist.Count > 0)
            {
                _currentTrackIndex = 0;
                PlayCurrentTrack();
            }

            if (addedCount > 0)
            {
                _announce?.Invoke(MediaPlayerAnnouncementText.TracksAdded(
                    count: addedCount,
                    trackTitle: GetCurrentTrackTitle(),
                    playbackState: GetPlaybackStateText()));
            }
        }
    }

    private void AddStreamUrl()
    {
        var dialog = new System.Windows.Window
        {
            Title = "Add Stream URL",
            Width = 500,
            Height = 150,
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
            ResizeMode = System.Windows.ResizeMode.NoResize,
        };

        var panel = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(10) };
        panel.Children.Add(new TextBlock { Text = "Enter stream URL:", Margin = new System.Windows.Thickness(0, 0, 0, 8) });
        var textBox = new System.Windows.Controls.TextBox();
        panel.Children.Add(textBox);
        var button = new System.Windows.Controls.Button { Content = "OK", Width = 80, Margin = new System.Windows.Thickness(0, 10, 0, 0), HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
        button.Click += (_, _) => { dialog.DialogResult = true; dialog.Close(); };
        panel.Children.Add(button);
        dialog.Content = panel;
        textBox.Focus();

        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            _playlist.Add(textBox.Text.Trim());
            UpdatePlaylistDisplay();

            if (_currentTrackIndex < 0 && _playlist.Count > 0)
            {
                _currentTrackIndex = 0;
                PlayCurrentTrack();
            }

            _announce?.Invoke(MediaPlayerAnnouncementText.TracksAdded(
                count: 1,
                trackTitle: GetCurrentTrackTitle(),
                playbackState: GetPlaybackStateText()));
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        _playbackStateText?.Dispatcher.BeginInvoke(() =>
        {
            if (_currentTrackIndex >= 0 && _currentTrackIndex < _playlist.Count - 1)
            {
                _currentTrackIndex++;
                PlayCurrentTrack();
                AnnounceTrackChanged();
            }
            else
            {
                UpdatePlaybackStateDisplay();
                _announce?.Invoke(MediaPlayerAnnouncementText.PlaybackFinished());
            }
        });
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateProgressDisplay();
    }

    private void UpdateTrackInfoDisplay()
    {
        if (_currentTrackIndex >= 0 && _currentTrackIndex < _playlist.Count)
        {
            var path = _playlist[_currentTrackIndex];

            if (IsStreamUrl(path))
            {
                if (_trackTitleText != null) _trackTitleText.Text = "Stream";
                if (_artistText != null) _artistText.Text = path;
            }
            else
            {
                if (_trackTitleText != null) _trackTitleText.Text = Path.GetFileNameWithoutExtension(path);
                if (_artistText != null) _artistText.Text = Path.GetDirectoryName(path) ?? string.Empty;
            }
        }
        else
        {
            if (_trackTitleText != null) _trackTitleText.Text = "No track loaded";
            if (_artistText != null) _artistText.Text = string.Empty;
        }
    }

    private void UpdateProgressDisplay()
    {
        if (_audioStream != null)
        {
            var current = FormatTime(_audioStream.CurrentTime);
            var total = FormatTime(_audioStream.TotalTime);
            if (_progressText != null)
                _progressText.Text = $"{current} / {total}";
        }
        else
        {
            if (_progressText != null)
                _progressText.Text = "0:00 / 0:00";
        }
    }

    private void UpdateVolumeDisplay()
    {
        if (_volumeText != null)
        {
            if (_isMuted)
                _volumeText.Text = "Volume: Muted";
            else
                _volumeText.Text = $"Volume: {(int)(_volume * 100)}%";
        }
    }

    private void UpdatePlaybackStateDisplay()
    {
        if (_playbackStateText == null) return;

        if (_waveOut == null)
        {
            _playbackStateText.Text = "Stopped";
            return;
        }

        _playbackStateText.Text = _waveOut.PlaybackState switch
        {
            PlaybackState.Playing => "Playing",
            PlaybackState.Paused => "Paused",
            _ => "Stopped",
        };
    }

    private void UpdatePlaylistDisplay()
    {
        if (_playlistList == null) return;

        _playlistList.Items.Clear();
        foreach (var item in _playlist)
        {
            _playlistList.Items.Add(IsStreamUrl(item) ? item : Path.GetFileName(item));
        }

        UpdatePlaylistSelection();
    }

    private void UpdatePlaylistSelection()
    {
        if (_playlistList == null) return;

        if (_currentTrackIndex >= 0 && _currentTrackIndex < _playlistList.Items.Count)
        {
            _playlistList.SelectedIndex = _currentTrackIndex;
            _playlistList.ScrollIntoView(_playlistList.SelectedItem);
        }
    }

    private static bool IsStreamUrl(string path)
    {
        return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    private void AnnounceTrackInfo()
    {
        var title = _trackTitleText?.Text ?? "No track loaded";
        var state = _playbackStateText?.Text ?? "Stopped";

        if (_audioStream != null)
        {
            var current = _audioStream.CurrentTime;
            var total = _audioStream.TotalTime;
            var remaining = total - current;
            _announce?.Invoke($"{title}. {state}. {FormatTime(current)} of {FormatTime(total)}. {FormatTime(remaining)} remaining.");
        }
        else
        {
            _announce?.Invoke($"{title}. {state}.");
        }
    }

    private void AnnouncePlaybackState()
    {
        _announce?.Invoke(MediaPlayerAnnouncementText.PlaybackState(
            state: GetPlaybackStateText(),
            trackTitle: GetCurrentTrackTitle()));
    }

    private void AnnounceTrackChanged()
    {
        _announce?.Invoke(MediaPlayerAnnouncementText.TrackChanged(
            trackTitle: GetCurrentTrackTitle(),
            playbackState: GetPlaybackStateText()));
    }

    private string GetPlaybackStateText()
    {
        return _playbackStateText?.Text ?? "Stopped";
    }

    private string GetCurrentTrackTitle()
    {
        if (_currentTrackIndex < 0 || _currentTrackIndex >= _playlist.Count)
        {
            return string.Empty;
        }

        var path = _playlist[_currentTrackIndex];
        if (IsStreamUrl(path))
        {
            return "Stream";
        }

        return Path.GetFileNameWithoutExtension(path);
    }

    private static string FormatTime(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}";

        return $"{(int)time.TotalMinutes}:{time.Seconds:D2}";
    }
}
