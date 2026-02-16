using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NAudio.Midi;
using NAudio.Wave;
using MeltySynthLib = MeltySynth;

namespace AccessNote;

internal sealed class MidiPlayerModule
{
    private TextBlock? _fileNameText;
    private TextBlock? _playbackStateText;
    private TextBlock? _progressText;
    private TextBlock? _tempoText;
    private TextBlock? _soundFontText;

    private DispatcherTimer? _timer;

    private string? _midiFilePath;
    private int _tempoBpm = 120;
    private PlaybackState _state = PlaybackState.Stopped;

    // Windows MIDI mode
    private MidiOut? _midiOut;
    private MidiFile? _loadedMidiFile;

    // SoundFont mode
    private string? _soundFontPath;
    private MeltySynthLib.Synthesizer? _synthesizer;
    private MeltySynthLib.MidiFileSequencer? _sequencer;
    private WaveOutEvent? _waveOut;

    private DateTime _playbackStartTime;
    private TimeSpan _totalDuration;
    private TimeSpan _pausedElapsed;

    private bool UseSoundFont => _soundFontPath != null && _synthesizer != null;

    private MeltySynthLib.MidiFile? _loadedMeltySynthMidi;

    public void Enter(
        TextBlock fileNameText,
        TextBlock playbackStateText,
        TextBlock progressText,
        TextBlock tempoText,
        TextBlock soundFontText)
    {
        _fileNameText = fileNameText;
        _playbackStateText = playbackStateText;
        _progressText = progressText;
        _tempoText = tempoText;
        _soundFontText = soundFontText;

        UpdateDisplay();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    public void RestoreFocus()
    {
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        switch (key)
        {
            case Key.Space:
                TogglePlayPause();
                return true;

            case Key.S when modifiers == ModifierKeys.None:
                Stop();
                return true;

            case Key.Add:
            case Key.OemPlus:
                AdjustTempo(10);
                return true;

            case Key.Subtract:
            case Key.OemMinus:
                AdjustTempo(-10);
                return true;

            case Key.F3:
                OpenSoundFontDialog();
                return true;

            case Key.O when modifiers == ModifierKeys.None:
                OpenMidiFileDialog();
                return true;

            default:
                return false;
        }
    }

    public bool CanLeave()
    {
        return true;
    }

    public void Stop()
    {
        StopPlayback();
        _state = PlaybackState.Stopped;
        _pausedElapsed = TimeSpan.Zero;
        UpdateDisplay();

        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }

    private void TogglePlayPause()
    {
        if (_midiFilePath == null)
            return;

        if (_state == PlaybackState.Playing)
        {
            PausePlayback();
            _state = PlaybackState.Paused;
        }
        else
        {
            StartPlayback();
            _state = PlaybackState.Playing;
        }

        UpdateDisplay();
    }

    private void StartPlayback()
    {
        if (_midiFilePath == null)
            return;

        if (UseSoundFont)
        {
            StartSoundFontPlayback();
        }
        else
        {
            StartWindowsMidiPlayback();
        }
    }

    private void StartWindowsMidiPlayback()
    {
        try
        {
            _midiOut ??= new MidiOut(0);
            _playbackStartTime = DateTime.Now - _pausedElapsed;
        }
        catch
        {
            // Windows MIDI device unavailable
        }
    }

    private void StartSoundFontPlayback()
    {
        try
        {
            if (_sequencer == null || _synthesizer == null)
                return;

            if (_state == PlaybackState.Paused && _waveOut != null)
            {
                _waveOut.Play();
                _playbackStartTime = DateTime.Now - _pausedElapsed;
                return;
            }

            _sequencer.Play(_loadedMeltySynthMidi!, false);
            _sequencer.Speed = _tempoBpm / 120.0;

            _waveOut?.Dispose();
            _waveOut = new WaveOutEvent();
            var provider = new SynthWaveProvider(_synthesizer, _sequencer);
            _waveOut.Init(provider);
            _waveOut.Play();
            _playbackStartTime = DateTime.Now;
            _pausedElapsed = TimeSpan.Zero;
        }
        catch
        {
            // SoundFont playback error
        }
    }

    private void PausePlayback()
    {
        _pausedElapsed = DateTime.Now - _playbackStartTime;

        if (UseSoundFont && _waveOut != null)
        {
            _waveOut.Pause();
        }
    }

    private void StopPlayback()
    {
        if (_waveOut != null)
        {
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }

        if (_midiOut != null)
        {
            _midiOut.Dispose();
            _midiOut = null;
        }
    }

    private void AdjustTempo(int delta)
    {
        _tempoBpm = Math.Clamp(_tempoBpm + delta, 20, 400);

        if (_sequencer != null)
        {
            _sequencer.Speed = _tempoBpm / 120.0;
        }

        UpdateDisplay();
    }

    private void OpenMidiFileDialog()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "MIDI files (*.mid;*.midi)|*.mid;*.midi",
            Title = "Open MIDI File",
        };

        if (dialog.ShowDialog() == true)
        {
            StopPlayback();
            _state = PlaybackState.Stopped;
            _pausedElapsed = TimeSpan.Zero;
            LoadMidiFile(dialog.FileName);
            UpdateDisplay();
        }
    }

    private void OpenSoundFontDialog()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "SoundFont files (*.sf2)|*.sf2",
            Title = "Load SoundFont",
        };

        if (dialog.ShowDialog() == true)
        {
            LoadSoundFont(dialog.FileName);
            UpdateDisplay();
        }
    }

    private void LoadMidiFile(string path)
    {
        try
        {
            _midiFilePath = path;
            _loadedMidiFile = new MidiFile(path, false);
            _tempoBpm = ExtractTempo(_loadedMidiFile);
            _totalDuration = EstimateDuration(_loadedMidiFile);

            if (_synthesizer != null)
            {
                _loadedMeltySynthMidi = new MeltySynthLib.MidiFile(path);
            }
        }
        catch
        {
            _midiFilePath = null;
            _loadedMidiFile = null;
        }
    }

    private void LoadSoundFont(string path)
    {
        try
        {
            StopPlayback();
            _state = PlaybackState.Stopped;
            _pausedElapsed = TimeSpan.Zero;

            _soundFontPath = path;
            var settings = new MeltySynthLib.SynthesizerSettings(44100);
            _synthesizer = new MeltySynthLib.Synthesizer(_soundFontPath, settings);
            _sequencer = new MeltySynthLib.MidiFileSequencer(_synthesizer);

            if (_midiFilePath != null)
            {
                _loadedMeltySynthMidi = new MeltySynthLib.MidiFile(_midiFilePath);
            }
        }
        catch
        {
            _soundFontPath = null;
            _synthesizer = null;
            _sequencer = null;
        }
    }

    private static int ExtractTempo(MidiFile midiFile)
    {
        foreach (var evt in midiFile.Events[0])
        {
            if (evt is TempoEvent tempoEvent)
            {
                return (int)Math.Round(60_000_000.0 / tempoEvent.MicrosecondsPerQuarterNote);
            }
        }

        return 120;
    }

    private static TimeSpan EstimateDuration(MidiFile midiFile)
    {
        long maxTicks = 0;

        foreach (var track in midiFile.Events)
        {
            foreach (var evt in track)
            {
                if (evt.AbsoluteTime > maxTicks)
                    maxTicks = evt.AbsoluteTime;
            }
        }

        int tempo = 120;
        foreach (var evt in midiFile.Events[0])
        {
            if (evt is TempoEvent te)
            {
                tempo = (int)Math.Round(60_000_000.0 / te.MicrosecondsPerQuarterNote);
                break;
            }
        }

        double ticksPerBeat = midiFile.DeltaTicksPerQuarterNote;
        double beats = maxTicks / ticksPerBeat;
        double seconds = beats * 60.0 / tempo;

        return TimeSpan.FromSeconds(seconds);
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_state == PlaybackState.Playing)
        {
            var elapsed = DateTime.Now - _playbackStartTime;
            if (elapsed >= _totalDuration && _totalDuration > TimeSpan.Zero)
            {
                Stop();
                _state = PlaybackState.Stopped;
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_fileNameText != null)
        {
            _fileNameText.Text = _midiFilePath != null
                ? Path.GetFileName(_midiFilePath)
                : "No file loaded";
        }

        if (_playbackStateText != null)
        {
            _playbackStateText.Text = _state switch
            {
                PlaybackState.Playing => "Playing",
                PlaybackState.Paused => "Paused",
                _ => "Stopped",
            };
        }

        if (_progressText != null)
        {
            var elapsed = _state switch
            {
                PlaybackState.Playing => DateTime.Now - _playbackStartTime,
                PlaybackState.Paused => _pausedElapsed,
                _ => TimeSpan.Zero,
            };

            _progressText.Text = $"{FormatTime(elapsed)} / {FormatTime(_totalDuration)}";
        }

        if (_tempoText != null)
        {
            _tempoText.Text = $"Tempo: {_tempoBpm} BPM";
        }

        if (_soundFontText != null)
        {
            _soundFontText.Text = _soundFontPath != null
                ? $"SoundFont: {Path.GetFileName(_soundFontPath)}"
                : "SoundFont: Windows MIDI";
        }
    }

    private static string FormatTime(TimeSpan ts)
    {
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"m\:ss");
    }

    private enum PlaybackState
    {
        Stopped,
        Playing,
        Paused,
    }

    private sealed class SynthWaveProvider : IWaveProvider
    {
        private readonly MeltySynthLib.Synthesizer _synthesizer;
        private readonly MeltySynthLib.MidiFileSequencer _sequencer;

        public SynthWaveProvider(MeltySynthLib.Synthesizer synthesizer, MeltySynthLib.MidiFileSequencer sequencer)
        {
            _synthesizer = synthesizer;
            _sequencer = sequencer;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(
                _synthesizer.SampleRate, 2);
        }

        public WaveFormat WaveFormat { get; }

        public int Read(byte[] buffer, int offset, int count)
        {
            var sampleCount = count / (2 * sizeof(float));
            var left = new float[sampleCount];
            var right = new float[sampleCount];

            _sequencer.Render(left, right);

            // Interleave into the byte buffer as IEEE float
            int byteIndex = offset;
            for (int i = 0; i < sampleCount; i++)
            {
                var leftBytes = BitConverter.GetBytes(left[i]);
                var rightBytes = BitConverter.GetBytes(right[i]);
                Buffer.BlockCopy(leftBytes, 0, buffer, byteIndex, 4);
                byteIndex += 4;
                Buffer.BlockCopy(rightBytes, 0, buffer, byteIndex, 4);
                byteIndex += 4;
            }

            return count;
        }
    }
}
