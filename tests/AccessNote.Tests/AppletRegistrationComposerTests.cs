using AccessNote;
using System.Windows.Input;

namespace AccessNote.Tests;

public sealed class AppletRegistrationComposerTests
{
    [Fact]
    public void CreateRegistry_ComposesRegisteredApplets()
    {
        var notesApplet = new TestApplet(AppletId.Notes);
        var settingsApplet = new TestApplet(AppletId.Settings);
        var registrations = new IAppletRegistration[]
        {
            new TestRegistration(notesApplet),
            new TestRegistration(settingsApplet),
        };

        var registry = AppletRegistrationComposer.CreateRegistry(registrations, CreateContext());

        Assert.Same(notesApplet, registry.GetRequired(AppletId.Notes));
        Assert.Same(settingsApplet, registry.GetRequired(AppletId.Settings));
    }

    [Fact]
    public void CreateRegistry_WhenDuplicateAppletIds_Throws()
    {
        var registrations = new IAppletRegistration[]
        {
            new TestRegistration(new TestApplet(AppletId.Notes)),
            new TestRegistration(new TestApplet(AppletId.Notes)),
        };

        Assert.Throws<InvalidOperationException>(
            () => AppletRegistrationComposer.CreateRegistry(registrations, CreateContext()));
    }

    private static AppletRegistrationContext CreateContext()
    {
        return new AppletRegistrationContext
        {
            ShellView = null!,
            AnnounceHint = _ => { },
            Dispatcher = null!,
        };
    }

    private sealed class TestRegistration : IAppletRegistration
    {
        private readonly IApplet _applet;

        public TestRegistration(IApplet applet)
        {
            _applet = applet;
        }

        public IApplet Create(AppletRegistrationContext context)
        {
            return _applet;
        }
    }

    private sealed class TestApplet : IApplet
    {
        public TestApplet(AppletId id)
        {
            Descriptor = new AppletDescriptor(id, id.ToString());
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
