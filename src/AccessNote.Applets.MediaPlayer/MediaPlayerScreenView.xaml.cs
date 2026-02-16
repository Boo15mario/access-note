using System.Windows.Controls;

namespace AccessNote;

public partial class MediaPlayerScreenView : UserControl
{
    public MediaPlayerScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock TrackTitleTextControl => TrackTitleText;
    internal TextBlock ArtistTextControl => ArtistText;
    internal TextBlock ProgressTextControl => ProgressText;
    internal TextBlock VolumeTextControl => VolumeText;
    internal TextBlock PlaybackStateTextControl => PlaybackStateText;
    internal ListBox PlaylistListControl => PlaylistList;
}
