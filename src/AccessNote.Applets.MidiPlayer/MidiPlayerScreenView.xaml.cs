using System.Windows.Controls;

namespace AccessNote;

internal partial class MidiPlayerScreenView : UserControl
{
    public MidiPlayerScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock FileNameTextControl => FileNameText;
    internal TextBlock PlaybackStateTextControl => PlaybackStateText;
    internal TextBlock ProgressTextControl => ProgressText;
    internal TextBlock TempoTextControl => TempoText;
    internal TextBlock SoundFontTextControl => SoundFontText;
}
