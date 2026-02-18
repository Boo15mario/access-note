namespace AccessNote;

internal static class CalendarAnnouncementText
{
    public static string EventsList(int count)
    {
        return $"Events list. {count} event(s).";
    }

    public static string EventSelected(string title)
    {
        return $"Event selected. {title}.";
    }

    public static string EventCreated(string title)
    {
        return $"Event created. {title}.";
    }

    public static string EventDeleted(string title)
    {
        return $"Event deleted. {title}.";
    }
}
