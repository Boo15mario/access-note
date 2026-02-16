using AccessNote;
using System.Windows.Input;

namespace AccessNote.Tests;

public sealed class MainMenuEntryBuilderTests
{
    [Fact]
    public void Build_IncludesTopLevelAppletsThenExit_WhenNoUtilityApplets()
    {
        var notesApplet = new TestApplet(new AppletDescriptor(AppletId.Notes, "Notes"));
        var settingsApplet = new TestApplet(new AppletDescriptor(AppletId.Settings, "Settings"));
        var registry = new AppletRegistry(new IApplet[] { notesApplet, settingsApplet });

        var entries = MainMenuEntryBuilder.Build(registry);

        Assert.Equal(3, entries.Count);
        Assert.Equal("Notes", entries[0].Label);
        Assert.Equal(AppletId.Notes, entries[0].AppletId);
        Assert.Equal("Settings", entries[1].Label);
        Assert.Equal(AppletId.Settings, entries[1].AppletId);
        Assert.Equal(MainMenuEntryId.Exit, entries[2].Id);
    }

    private sealed class TestApplet : IApplet
    {
        public TestApplet(AppletDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public AppletDescriptor Descriptor { get; }

        public void Enter()
        {
        }

        public void RestoreFocus()
        {
        }

        public bool CanLeave()
        {
            return true;
        }

        public bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)
        {
            return false;
        }
    }
}
