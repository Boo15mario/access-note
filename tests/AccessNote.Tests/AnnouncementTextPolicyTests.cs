namespace AccessNote.Tests;

public sealed class AnnouncementTextPolicyTests
{
    [Fact]
    public void Normalize_WhenMessageIsNullOrWhitespace_ReturnsReady()
    {
        Assert.Equal("Ready.", AnnouncementTextPolicy.Normalize(null));
        Assert.Equal("Ready.", AnnouncementTextPolicy.Normalize(string.Empty));
        Assert.Equal("Ready.", AnnouncementTextPolicy.Normalize("   "));
    }

    [Fact]
    public void Normalize_CollapsesWhitespaceAndTrims()
    {
        var normalized = AnnouncementTextPolicy.Normalize("  Media   player\tstarted \r\n successfully.  ");

        Assert.Equal("Media player started successfully.", normalized);
    }
}
