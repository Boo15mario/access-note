using System.Windows.Controls;

namespace AccessNote;

public partial class AppLauncherScreenView : UserControl
{
    public AppLauncherScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock ModeTextControl => ModeText;
    internal ListBox AppListControl => AppList;
    internal TextBlock DetailNameControl => DetailName;
    internal TextBlock DetailPathControl => DetailPath;
    internal TextBlock DetailArgumentsControl => DetailArguments;
    internal Button AddButtonControl => AddButton;
    internal Button RemoveButtonControl => RemoveButton;
    internal Button LaunchButtonControl => LaunchButton;
}
