namespace AccessNote;

internal interface IStartupHost
{
    void LoadSettings();
    void ApplyTheme();
    void LoadPersistedNotes();
    void PrepareShellUi();
    void PlayStartupSound();
    StartScreenOption GetStartScreen();
    bool TryResolveStartApplet(StartScreenOption startScreen, out AppletId appletId);
    void ShowMainMenu();
    void OpenApplet(AppletId appletId);
    void AnnounceMainMenuHint();
}

internal sealed class StartupFlowCoordinator
{
    private readonly IStartupHost _host;

    public StartupFlowCoordinator(IStartupHost host)
    {
        _host = host;
    }

    public void HandleLoaded()
    {
        _host.LoadSettings();
        _host.ApplyTheme();
        _host.LoadPersistedNotes();
        _host.PrepareShellUi();
        _host.PlayStartupSound();

        _host.ShowMainMenu();
        if (_host.TryResolveStartApplet(_host.GetStartScreen(), out var appletId))
        {
            _host.OpenApplet(appletId);
            return;
        }

        _host.AnnounceMainMenuHint();
    }
}
