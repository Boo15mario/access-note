using AccessNote;
using System.Windows.Input;

namespace AccessNote.Tests;

public sealed class AppletRegistryTests
{
    [Fact]
    public void GetRequired_ReturnsRegisteredApplet()
    {
        var notesApplet = new TestApplet(AppletId.Notes);
        var settingsApplet = new TestApplet(AppletId.Settings);
        var registry = new AppletRegistry(new IApplet[] { notesApplet, settingsApplet });

        var resolved = registry.GetRequired(AppletId.Settings);

        Assert.Same(settingsApplet, resolved);
    }

    [Fact]
    public void Constructor_WhenDuplicateIds_ThrowsInvalidOperationException()
    {
        var firstNotesApplet = new TestApplet(AppletId.Notes);
        var secondNotesApplet = new TestApplet(AppletId.Notes);

        Assert.Throws<InvalidOperationException>(
            () => new AppletRegistry(new IApplet[] { firstNotesApplet, secondNotesApplet }));
    }

    [Fact]
    public void GetRequired_WhenMissingId_ThrowsKeyNotFoundException()
    {
        var notesApplet = new TestApplet(AppletId.Notes);
        var registry = new AppletRegistry(new IApplet[] { notesApplet });

        Assert.Throws<KeyNotFoundException>(() => registry.GetRequired(AppletId.Settings));
    }

    [Fact]
    public void ResolveStartAppletId_WhenMappedStartScreen_ReturnsAppletId()
    {
        var notesApplet = new TestApplet(
            new AppletDescriptor(
                id: AppletId.Notes,
                label: "Notes",
                screenHintText: "Notes.",
                helpText: "Notes help.",
                startScreenOption: StartScreenOption.Notes));
        var settingsApplet = new TestApplet(AppletId.Settings);
        var registry = new AppletRegistry(new IApplet[] { notesApplet, settingsApplet });

        var resolved = registry.ResolveStartAppletId(StartScreenOption.Notes);

        Assert.Equal(AppletId.Notes, resolved);
    }

    [Fact]
    public void GetDescriptorsInRegistrationOrder_ReturnsRegisteredDescriptorLabels()
    {
        var notesApplet = new TestApplet(
            new AppletDescriptor(
                id: AppletId.Notes,
                label: "Notes label",
                screenHintText: "Notes.",
                helpText: "Notes help."));
        var settingsApplet = new TestApplet(
            new AppletDescriptor(
                id: AppletId.Settings,
                label: "Settings label",
                screenHintText: "Settings.",
                helpText: "Settings help."));
        var registry = new AppletRegistry(new IApplet[] { notesApplet, settingsApplet });

        var labels = registry.GetDescriptorsInRegistrationOrder().Select(descriptor => descriptor.Label).ToArray();

        Assert.Equal(new[] { "Notes label", "Settings label" }, labels);
    }

    private sealed class TestApplet : IApplet
    {
        public TestApplet(AppletId id)
        {
            Descriptor = new AppletDescriptor(id, id.ToString());
        }

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
