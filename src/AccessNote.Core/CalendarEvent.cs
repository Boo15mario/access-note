using System;

namespace AccessNote;

public sealed class CalendarEvent
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public TimeSpan? Time { get; set; }

    public string? Description { get; set; }

    public string DisplayText
    {
        get
        {
            if (Time.HasValue)
            {
                var hours = Time.Value.Hours;
                var minutes = Time.Value.Minutes;
                var ampm = hours >= 12 ? "PM" : "AM";
                var h = hours % 12;
                if (h == 0) h = 12;
                return $"{h}:{minutes:D2} {ampm} - {Title}";
            }

            return Title;
        }
    }
}
