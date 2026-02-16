using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AccessNote;

public partial class CalendarScreenView : UserControl
{
    public CalendarScreenView()
    {
        InitializeComponent();
    }

    internal TextBlock MonthYearHeaderControl => MonthYearHeader;
    internal ListBox DayCellsListControl => DayCellsList;
    internal TextBlock SelectedDateTextControl => SelectedDateText;
    internal ListBox EventsListControl => EventsList;
    internal TextBlock EventDetailTextControl => EventDetailText;
}
