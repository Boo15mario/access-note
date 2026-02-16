using System.Windows.Controls;

namespace AccessNote;

public partial class SystemMonitorScreenView : UserControl
{
    public SystemMonitorScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock CpuTextControl => CpuText;
    internal TextBlock MemoryTextControl => MemoryText;
    internal ItemsControl DiskListControl => DiskList;
}
