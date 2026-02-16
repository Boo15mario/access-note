using System.Windows.Controls;

namespace AccessNote;

public partial class DateTimeScreenView : UserControl
{
    public DateTimeScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock DateTextControl => DateText;
    internal TextBlock TimeTextControl => TimeText;
    internal TextBlock WeekTextControl => WeekText;
}
