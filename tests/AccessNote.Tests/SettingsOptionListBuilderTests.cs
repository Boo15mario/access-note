using System.Collections.Generic;
using System.IO;
using AccessNote;

namespace AccessNote.Tests;

public sealed class SettingsOptionListBuilderTests
{
    [Fact]
    public void Build_AdvancedCategory_ProvidesResetAction()
    {
        var session = CreateSession();
        session.BeginEditing();
        var resetCalled = false;
        var builder = new SettingsOptionListBuilder(session, () => resetCalled = true);

        var options = builder.Build(categoryIndex: 3);

        Assert.Single(options);
        Assert.Equal("Reset settings to defaults", options[0].Label);
        options[0].ChangeBy?.Invoke(1);
        Assert.True(resetCalled);
    }

    [Fact]
    public void PopulateVisibleRows_ReplacesRowsWithRenderedValues()
    {
        var options = new List<SettingsOptionEntry>
        {
            new()
            {
                Label = "Announce status messages",
                Hint = "Toggle announcements.",
                GetValue = () => "On",
                ChangeBy = _ => { },
            },
            new()
            {
                Label = "Announce hints on screen open",
                Hint = "Toggle hints.",
                GetValue = () => "Off",
                ChangeBy = _ => { },
            },
        };
        var visibleRows = new List<string> { "stale row" };

        SettingsOptionListBuilder.PopulateVisibleRows(options, visibleRows);

        Assert.Equal(2, visibleRows.Count);
        Assert.Equal("Announce status messages: On", visibleRows[0]);
        Assert.Equal("Announce hints on screen open: Off", visibleRows[1]);
    }

    private static SettingsSession CreateSession()
    {
        var tempDatabasePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.db");
        return new SettingsSession(new SettingsStorage(tempDatabasePath));
    }
}
