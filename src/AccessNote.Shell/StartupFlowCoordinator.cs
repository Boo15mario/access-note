namespace AccessNote;

internal interface IStartupHost
{
    void LoadSettings();
    void LoadPersistedNotes();
    void PrepareShellUi();
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
        _host.LoadPersistedNotes();
        _host.PrepareShellUi();

        _host.ShowMainMenu();
        if (_host.TryResolveStartApplet(_host.GetStartScreen(), out var appletId))
        {
            _host.OpenApplet(appletId);
            return;
        }

        _host.AnnounceMainMenuHint();
    }
}
