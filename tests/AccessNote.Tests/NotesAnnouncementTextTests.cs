namespace AccessNote.Tests;

public sealed class NotesAnnouncementTextTests
{
    [Fact]
    public void FocusNotesList_ReturnsExpectedMessage()
    {
        var text = NotesAnnouncementText.FocusNotesList();

        Assert.Equal("Notes list.", text);
    }

    [Fact]
    public void FocusEditor_ReturnsExpectedMessage()
    {
        var text = NotesAnnouncementText.FocusEditor();

        Assert.Equal("Editor.", text);
    }
}
