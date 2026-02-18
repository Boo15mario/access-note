namespace AccessNote.Tests;

public sealed class CalendarAnnouncementTextTests
{
    [Fact]
    public void EventsList_IncludesCount()
    {
        var text = CalendarAnnouncementText.EventsList(2);

        Assert.Equal("Events list. 2 event(s).", text);
    }

    [Fact]
    public void EventSelected_UsesConsistentWording()
    {
        var text = CalendarAnnouncementText.EventSelected("Dentist");

        Assert.Equal("Event selected. Dentist.", text);
    }
}
