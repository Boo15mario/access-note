using AccessNote;

namespace AccessNote.Tests;

public sealed class StartupFlowCoordinatorTests
{
    [Fact]
    public void HandleLoaded_WhenStartScreenIsNotes_OpensNotesAndSkipsMainMenuHint()
    {
        var host = new FakeStartupHost(StartScreenOption.Notes);
        var coordinator = new StartupFlowCoordinator(host);

        coordinator.HandleLoaded();

        Assert.Equal(
            new[]
            {
                "load_settings",
                "load_notes",
                "prepare_ui",
                "show_main_menu",
                "open_applet_notes"
            },
            host.Calls);
    }

    [Fact]
    public void HandleLoaded_WhenStartScreenIsMainMenu_AnnouncesMainMenuHint()
    {
        var host = new FakeStartupHost(StartScreenOption.MainMenu);
        var coordinator = new StartupFlowCoordinator(host);

        coordinator.HandleLoaded();

        Assert.Equal(
            new[]
            {
                "load_settings",
                "load_notes",
                "prepare_ui",
                "show_main_menu",
                "announce_main_menu_hint"
            },
            host.Calls);
    }

    private sealed class FakeStartupHost : IStartupHost
    {
        private readonly StartScreenOption _startScreen;

        public FakeStartupHost(StartScreenOption startScreen)
        {
            _startScreen = startScreen;
        }

        public List<string> Calls { get; } = new();

        public void LoadSettings()
        {
            Calls.Add("load_settings");
        }

        public void LoadPersistedNotes()
        {
            Calls.Add("load_notes");
        }

        public void PrepareShellUi()
        {
            Calls.Add("prepare_ui");
        }

        public StartScreenOption GetStartScreen()
        {
            return _startScreen;
        }

        public bool TryResolveStartApplet(StartScreenOption startScreen, out AppletId appletId)
        {
            if (startScreen == StartScreenOption.Notes)
            {
                appletId = AppletId.Notes;
                return true;
            }

            appletId = default;
            return false;
        }

        public void ShowMainMenu()
        {
            Calls.Add("show_main_menu");
        }

        public void OpenApplet(AppletId appletId)
        {
            Calls.Add($"open_applet_{appletId.ToString().ToLowerInvariant()}");
        }

        public void AnnounceMainMenuHint()
        {
            Calls.Add("announce_main_menu_hint");
        }
    }
}
