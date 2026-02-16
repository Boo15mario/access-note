using AccessNote;

namespace AccessNote.Tests;

public sealed class NotesSortPolicyTests
{
    [Fact]
    public void Apply_LastModifiedOldest_SortsAscendingByTimestamp()
    {
        var policy = new NotesSortPolicy();
        var notes = new[]
        {
            CreateNote("n1", "Zeta", new DateTime(2026, 2, 11, 12, 0, 0, DateTimeKind.Utc)),
            CreateNote("n2", "Alpha", new DateTime(2026, 2, 9, 12, 0, 0, DateTimeKind.Utc)),
            CreateNote("n3", "Beta", new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc)),
        };

        var sorted = policy.Apply(notes, NotesSortOrderOption.LastModifiedOldest).ToList();

        Assert.Equal(new[] { "n2", "n3", "n1" }, sorted.Select(n => n.Id));
    }

    [Fact]
    public void Apply_TitleAscending_SortsByCaseInsensitiveTitle()
    {
        var policy = new NotesSortPolicy();
        var notes = new[]
        {
            CreateNote("n1", "zeta", new DateTime(2026, 2, 11, 12, 0, 0, DateTimeKind.Utc)),
            CreateNote("n2", "Alpha", new DateTime(2026, 2, 9, 12, 0, 0, DateTimeKind.Utc)),
            CreateNote("n3", "beta", new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc)),
        };

        var sorted = policy.Apply(notes, NotesSortOrderOption.TitleAscending).ToList();

        Assert.Equal(new[] { "n2", "n3", "n1" }, sorted.Select(n => n.Id));
    }

    [Fact]
    public void Apply_LastModifiedNewest_DefaultSortsDescendingByTimestamp()
    {
        var policy = new NotesSortPolicy();
        var notes = new[]
        {
            CreateNote("n1", "A", new DateTime(2026, 2, 9, 12, 0, 0, DateTimeKind.Utc)),
            CreateNote("n2", "B", new DateTime(2026, 2, 11, 12, 0, 0, DateTimeKind.Utc)),
            CreateNote("n3", "C", new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc)),
        };

        var sorted = policy.Apply(notes, NotesSortOrderOption.LastModifiedNewest).ToList();

        Assert.Equal(new[] { "n2", "n3", "n1" }, sorted.Select(n => n.Id));
    }

    [Fact]
    public void GetPreferredSelection_ReturnsFirstNoteAfterSort()
    {
        var policy = new NotesSortPolicy();
        var notes = new[]
        {
            CreateNote("n1", "gamma", new DateTime(2026, 2, 9, 12, 0, 0, DateTimeKind.Utc)),
            CreateNote("n2", "Alpha", new DateTime(2026, 2, 11, 12, 0, 0, DateTimeKind.Utc)),
            CreateNote("n3", "beta", new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc)),
        };

        var preferred = policy.GetPreferredSelection(notes, NotesSortOrderOption.TitleAscending);

        Assert.NotNull(preferred);
        Assert.Equal("n2", preferred!.Id);
    }

    private static NoteDocument CreateNote(string id, string title, DateTime lastModifiedUtc)
    {
        return NoteDocument.FromPersisted(
            id: id,
            title: title,
            content: string.Empty,
            lastModifiedUtc: lastModifiedUtc);
    }
}
