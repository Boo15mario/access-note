using System.Collections.Generic;

namespace AccessNote;

public interface INoteStorage
{
    IReadOnlyList<NoteDocument> LoadNotes();

    void SaveNotes(IEnumerable<NoteDocument> notes);
}

public interface ISettingsStorage
{
    AppSettings LoadSettings();

    void SaveSettings(AppSettings settings);
}

public interface ICalendarEventStorage
{
    IReadOnlyList<CalendarEvent> GetEventsForDate(DateTime date);

    IReadOnlyList<CalendarEvent> GetEventsForMonth(int year, int month);

    void AddEvent(CalendarEvent calendarEvent);

    void UpdateEvent(CalendarEvent calendarEvent);

    void DeleteEvent(int id);
}
