using System;
using System.Windows.Input;

namespace AccessNote;

internal sealed class ScreenRouter
{
    private readonly AppletRegistry _appletRegistry;
    private readonly Action<int, bool> _showHomeScreen;
    private readonly Action _restoreHomeScreenFocus;
    private IApplet? _activeApplet;

    public ScreenRouter(
        AppletRegistry appletRegistry,
        Action<int, bool> showHomeScreen,
        Action restoreHomeScreenFocus)
    {
        _appletRegistry = appletRegistry ?? throw new ArgumentNullException(nameof(appletRegistry));
        _showHomeScreen = showHomeScreen ?? throw new ArgumentNullException(nameof(showHomeScreen));
        _restoreHomeScreenFocus = restoreHomeScreenFocus ?? throw new ArgumentNullException(nameof(restoreHomeScreenFocus));
    }

    public AppletId? ActiveAppletId { get; private set; }

    public void OpenApplet(AppletId appletId)
    {
        ActivateApplet(appletId);
    }

    public void ShowHomeScreen(int focusIndex, bool shouldAnnounce)
    {
        ActiveAppletId = null;
        _activeApplet = null;
        _showHomeScreen(focusIndex, shouldAnnounce);
    }

    public bool CanLeaveActiveScreen()
    {
        return _activeApplet?.CanLeave() ?? true;
    }

    public void RestoreFocusForActiveScreen()
    {
        if (!ActiveAppletId.HasValue)
        {
            _restoreHomeScreenFocus();
            return;
        }

        _activeApplet?.RestoreFocus();
    }

    public bool HandleInputForActiveApplet(KeyEventArgs e, Key key, ModifierKeys modifiers)
    {
        if (_activeApplet == null)
        {
            return false;
        }

        return _activeApplet.HandleInput(e, key, modifiers);
    }

    private void ActivateApplet(AppletId appletId)
    {
        var applet = _appletRegistry.GetRequired(appletId);
        _activeApplet = applet;
        ActiveAppletId = applet.Descriptor.Id;
        applet.Enter();
    }
}
