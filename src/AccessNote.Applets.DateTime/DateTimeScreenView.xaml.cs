using System.Windows.Controls;

namespace AccessNote;

internal partial class DateTimeScreenView : UserControl
{
    public DateTimeScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock DateTextControl => DateText;
    internal TextBlock TimeTextControl => TimeText;
    internal TextBlock WeekTextControl => WeekText;
}
