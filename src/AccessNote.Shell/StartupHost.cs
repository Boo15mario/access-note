using System;

namespace AccessNote;

internal sealed class StartupHost : IStartupHost
{
    private readonly Action _loadSettings;
    private readonly Action _applyTheme;
    private readonly Action _loadPersistedNotes;
    private readonly Action _prepareShellUi;
    private readonly Action _playStartupSound;
    private readonly Func<StartScreenOption> _getStartScreen;
    private readonly Func<StartScreenOption, AppletId?> _resolveStartApplet;
    private readonly Action _showMainMenu;
    private readonly Action<AppletId> _openApplet;
    private readonly Action _announceMainMenuHint;

    public StartupHost(
        Action loadSettings,
        Action applyTheme,
        Action loadPersistedNotes,
        Action prepareShellUi,
        Action playStartupSound,
        Func<StartScreenOption> getStartScreen,
        Func<StartScreenOption, AppletId?> resolveStartApplet,
        Action showMainMenu,
        Action<AppletId> openApplet,
        Action announceMainMenuHint)
    {
        _loadSettings = loadSettings;
        _applyTheme = applyTheme;
        _loadPersistedNotes = loadPersistedNotes;
        _prepareShellUi = prepareShellUi;
        _playStartupSound = playStartupSound;
        _getStartScreen = getStartScreen;
        _resolveStartApplet = resolveStartApplet;
        _showMainMenu = showMainMenu;
        _openApplet = openApplet;
        _announceMainMenuHint = announceMainMenuHint;
    }

    public void LoadSettings() => _loadSettings();

    public void ApplyTheme() => _applyTheme();

    public void LoadPersistedNotes() => _loadPersistedNotes();

    public void PrepareShellUi() => _prepareShellUi();

    public void PlayStartupSound() => _playStartupSound();

    public StartScreenOption GetStartScreen() => _getStartScreen();

    public bool TryResolveStartApplet(StartScreenOption startScreen, out AppletId appletId)
    {
        var resolved = _resolveStartApplet(startScreen);
        if (resolved.HasValue)
        {
            appletId = resolved.Value;
            return true;
        }

        appletId = default;
        return false;
    }

    public void ShowMainMenu() => _showMainMenu();

    public void OpenApplet(AppletId appletId) => _openApplet(appletId);

    public void AnnounceMainMenuHint() => _announceMainMenuHint();
}
