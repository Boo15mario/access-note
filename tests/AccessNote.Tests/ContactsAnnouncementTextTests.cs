namespace AccessNote.Tests;

public sealed class ContactsAnnouncementTextTests
{
    [Fact]
    public void FocusContactsList_ReturnsExpectedMessage()
    {
        var text = ContactsAnnouncementText.FocusContactsList();

        Assert.Equal("Contacts list.", text);
    }

    [Fact]
    public void FocusContactForm_ReturnsExpectedMessage()
    {
        var text = ContactsAnnouncementText.FocusContactForm();

        Assert.Equal("Contact form.", text);
    }

    [Fact]
    public void FocusContactActions_ReturnsExpectedMessage()
    {
        var text = ContactsAnnouncementText.FocusContactActions();

        Assert.Equal("Contact actions.", text);
    }
}
