using System.Windows.Controls;

namespace AccessNote;

internal partial class SystemMonitorScreenView : UserControl
{
    public SystemMonitorScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock CpuTextControl => CpuText;
    internal TextBlock MemoryTextControl => MemoryText;
    internal ItemsControl DiskListControl => DiskList;
}
