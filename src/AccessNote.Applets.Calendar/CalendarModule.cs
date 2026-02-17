using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccessNote;

internal sealed class CalendarModule
{
    private readonly ICalendarEventStorage _storage;
    private readonly ObservableCollection<string> _dayCells = new();
    private readonly ObservableCollection<CalendarEvent> _events = new();

    private TextBlock? _monthYearHeader;
    private ListBox? _dayCellsList;
    private TextBlock? _selectedDateText;
    private ListBox? _eventsList;
    private TextBlock? _eventDetailText;

    private int _currentYear;
    private int _currentMonth;
    private DateTime _selectedDate;
    private int _focusRegion; // 0 = calendar grid, 1 = events list
    private Action<string>? _announce;

    public CalendarModule(ICalendarEventStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public void Enter(
        TextBlock monthYearHeader,
        ListBox dayCellsList,
        TextBlock selectedDateText,
        ListBox eventsList,
        TextBlock eventDetailText,
        Action<string> announce)
    {
        _monthYearHeader = monthYearHeader;
        _dayCellsList = dayCellsList;
        _selectedDateText = selectedDateText;
        _eventsList = eventsList;
        _eventDetailText = eventDetailText;
        _announce = announce;

        _dayCellsList.ItemsSource = _dayCells;
        _eventsList.ItemsSource = _events;

        var today = DateTime.Today;
        _currentYear = today.Year;
        _currentMonth = today.Month;
        _selectedDate = today;
        _focusRegion = 0;

        RebuildCalendarGrid();
        SelectDayInGrid(_selectedDate.Day);
        UpdateEventsForSelectedDate();

        var dateStr = _selectedDate.ToString("dddd, MMMM d, yyyy", CultureInfo.CurrentCulture);
        var evtCount = _events.Count;
        _announce($"Calendar. {dateStr}. {evtCount} event(s).");
    }

    public void RestoreFocus()
    {
        if (_focusRegion == 0)
        {
            _dayCellsList?.Focus();
        }
        else
        {
            _eventsList?.Focus();
        }
    }

    public bool CanLeave()
    {
        return true;
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        if (key == Key.F6)
        {
            _focusRegion = _focusRegion == 0 ? 1 : 0;
            RestoreFocus();
            _announce?.Invoke(_focusRegion == 0 ? "Calendar grid" : $"Events list. {_events.Count} event(s).");
            return true;
        }

        if (_focusRegion == 0)
        {
            return HandleCalendarGridInput(key, modifiers);
        }

        return HandleEventsInput(key, modifiers);
    }

    private bool HandleCalendarGridInput(Key key, ModifierKeys modifiers)
    {
        if (key == Key.Left)
        {
            MoveDays(-1);
            return true;
        }

        if (key == Key.Right)
        {
            MoveDays(1);
            return true;
        }

        if (key == Key.Up)
        {
            MoveDays(-7);
            return true;
        }

        if (key == Key.Down)
        {
            MoveDays(7);
            return true;
        }

        if (key == Key.PageUp)
        {
            ChangeMonth(-1);
            return true;
        }

        if (key == Key.PageDown)
        {
            ChangeMonth(1);
            return true;
        }

        if (key == Key.Enter)
        {
            _focusRegion = 1;
            UpdateEventsForSelectedDate();
            RestoreFocus();
            return true;
        }

        if (key == Key.N && modifiers.HasFlag(ModifierKeys.Control))
        {
            CreateNewEvent();
            return true;
        }

        return false;
    }

    private bool HandleEventsInput(Key key, ModifierKeys modifiers)
    {
        if (key == Key.Delete)
        {
            DeleteSelectedEvent();
            return true;
        }

        if (key == Key.N && modifiers.HasFlag(ModifierKeys.Control))
        {
            CreateNewEvent();
            return true;
        }

        if (key == Key.Up || key == Key.Down)
        {
            // Let ListBox handle navigation, then update detail
            UpdateEventDetail();
            return false;
        }

        if (key == Key.Enter)
        {
            UpdateEventDetail();
            return true;
        }

        return false;
    }

    private void MoveDays(int offset)
    {
        var newDate = _selectedDate.AddDays(offset);

        if (newDate.Year != _currentYear || newDate.Month != _currentMonth)
        {
            _currentYear = newDate.Year;
            _currentMonth = newDate.Month;
            RebuildCalendarGrid();
        }

        _selectedDate = newDate;
        SelectDayInGrid(_selectedDate.Day);
        UpdateEventsForSelectedDate();
        AnnounceSelectedDate();
    }

    private void ChangeMonth(int offset)
    {
        var newDate = new DateTime(_currentYear, _currentMonth, 1).AddMonths(offset);
        _currentYear = newDate.Year;
        _currentMonth = newDate.Month;

        var daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);
        var day = Math.Min(_selectedDate.Day, daysInMonth);
        _selectedDate = new DateTime(_currentYear, _currentMonth, day);

        RebuildCalendarGrid();
        SelectDayInGrid(_selectedDate.Day);
        UpdateEventsForSelectedDate();
        AnnounceSelectedDate();
    }

    private void AnnounceSelectedDate()
    {
        var dateStr = _selectedDate.ToString("dddd, MMMM d, yyyy", CultureInfo.CurrentCulture);
        var evtCount = _events.Count;
        var evtText = evtCount > 0 ? $" {evtCount} event(s)." : "";
        _announce?.Invoke($"{dateStr}.{evtText}");
    }

    private void RebuildCalendarGrid()
    {
        _dayCells.Clear();

        if (_monthYearHeader != null)
        {
            _monthYearHeader.Text = new DateTime(_currentYear, _currentMonth, 1)
                .ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        }

        var firstDay = new DateTime(_currentYear, _currentMonth, 1);
        var startOffset = (int)firstDay.DayOfWeek;
        var daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);

        // Load events for the month to mark days with events
        IReadOnlyList<CalendarEvent> monthEvents;
        try
        {
            monthEvents = _storage.GetEventsForMonth(_currentYear, _currentMonth);
        }
        catch
        {
            monthEvents = Array.Empty<CalendarEvent>();
        }

        var daysWithEvents = new HashSet<int>();
        foreach (var evt in monthEvents)
        {
            daysWithEvents.Add(evt.Date.Day);
        }

        // Empty cells before the 1st
        for (var i = 0; i < startOffset; i++)
        {
            _dayCells.Add(string.Empty);
        }

        // Day cells
        for (var day = 1; day <= daysInMonth; day++)
        {
            var marker = daysWithEvents.Contains(day) ? " •" : "";
            _dayCells.Add($"{day}{marker}");
        }

        // Fill remaining cells to reach 42 (6 rows × 7 columns)
        while (_dayCells.Count < 42)
        {
            _dayCells.Add(string.Empty);
        }
    }

    private void SelectDayInGrid(int day)
    {
        if (_dayCellsList == null) return;

        var firstDay = new DateTime(_currentYear, _currentMonth, 1);
        var startOffset = (int)firstDay.DayOfWeek;
        var index = startOffset + day - 1;

        if (index >= 0 && index < _dayCells.Count)
        {
            _dayCellsList.SelectedIndex = index;
            _dayCellsList.ScrollIntoView(_dayCellsList.SelectedItem);
        }

        if (_selectedDateText != null)
        {
            _selectedDateText.Text = _selectedDate.ToString("dddd, MMMM d, yyyy", CultureInfo.CurrentCulture);
        }
    }

    private void UpdateEventsForSelectedDate()
    {
        _events.Clear();

        try
        {
            var dayEvents = _storage.GetEventsForDate(_selectedDate);
            foreach (var evt in dayEvents)
            {
                _events.Add(evt);
            }
        }
        catch
        {
            // Silently handle storage errors
        }

        if (_selectedDateText != null)
        {
            _selectedDateText.Text = _selectedDate.ToString("dddd, MMMM d, yyyy", CultureInfo.CurrentCulture);
        }

        if (_eventDetailText != null)
        {
            _eventDetailText.Text = _events.Count == 0 ? "No events" : $"{_events.Count} event(s)";
        }

        if (_eventsList != null && _events.Count > 0)
        {
            _eventsList.SelectedIndex = 0;
        }
    }

    private void UpdateEventDetail()
    {
        if (_eventDetailText == null || _eventsList == null) return;

        if (_eventsList.SelectedItem is CalendarEvent evt)
        {
            var detail = evt.Title;
            if (evt.Time.HasValue)
            {
                var hours = evt.Time.Value.Hours;
                var minutes = evt.Time.Value.Minutes;
                var ampm = hours >= 12 ? "PM" : "AM";
                var h = hours % 12;
                if (h == 0) h = 12;
                detail = $"{h}:{minutes:D2} {ampm} - {detail}";
            }

            if (!string.IsNullOrWhiteSpace(evt.Description))
            {
                detail += $"\n{evt.Description}";
            }

            _eventDetailText.Text = detail;
        }
        else
        {
            _eventDetailText.Text = "No event selected";
        }
    }

    private void CreateNewEvent()
    {
        var newEvent = new CalendarEvent
        {
            Title = "New Event",
            Date = _selectedDate,
        };

        try
        {
            _storage.AddEvent(newEvent);
            _events.Add(newEvent);
            RebuildCalendarGrid();
            SelectDayInGrid(_selectedDate.Day);

            if (_eventsList != null)
            {
                _eventsList.SelectedItem = newEvent;
            }

            _focusRegion = 1;
            RestoreFocus();
        }
        catch
        {
            // Silently handle storage errors
        }
    }

    private void DeleteSelectedEvent()
    {
        if (_eventsList?.SelectedItem is not CalendarEvent evt) return;

        try
        {
            _storage.DeleteEvent(evt.Id);
            _events.Remove(evt);
            RebuildCalendarGrid();
            SelectDayInGrid(_selectedDate.Day);
            UpdateEventDetail();
        }
        catch
        {
            // Silently handle storage errors
        }
    }
}
