using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class DateTimeModule
{
    private TextBlock? _dateText;
    private TextBlock? _timeText;
    private TextBlock? _weekText;
    private DispatcherTimer? _timer;
    private Action<string>? _announce;
    private Action? _returnToMainMenu;

    public void Enter(TextBlock dateText, TextBlock timeText, TextBlock weekText, Action<string> announce, Action returnToMainMenu)
    {
        _dateText = dateText;
        _timeText = timeText;
        _weekText = weekText;
        _announce = announce;
        _returnToMainMenu = returnToMainMenu;

        UpdateDisplay();
        AnnounceDateTime();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    public void RestoreFocus()
    {
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        _returnToMainMenu?.Invoke();
        return true;
    }

    public bool CanLeave()
    {
        return true;
    }

    public void Stop()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var now = DateTime.Now;

        if (_dateText != null)
        {
            _dateText.Text = now.ToString("dddd, MMMM d, yyyy", CultureInfo.CurrentCulture);
        }

        if (_timeText != null)
        {
            _timeText.Text = now.ToString("h:mm:ss tt", CultureInfo.CurrentCulture);
        }

        if (_weekText != null)
        {
            var weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var timezone = TimeZoneInfo.Local.DisplayName;
            _weekText.Text = $"Week {weekNumber} — {timezone}";
        }
    }

    private void AnnounceDateTime()
    {
        if (_dateText != null && _timeText != null)
        {
            _announce?.Invoke($"Date and Time. {_dateText.Text}. {_timeText.Text}.");
        }
    }
}
