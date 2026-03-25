using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AccessNote;

internal sealed class HomeScreenModule
{
    private readonly ShellViewAdapter _shellView;
    private readonly IReadOnlyList<HomeScreenEntry> _rootEntries;
    private readonly Action<AppletId> _openApplet;
    private readonly Action _showExitPrompt;
    private readonly Action<string> _announce;

    private IReadOnlyList<HomeScreenEntry> _activeEntries;
    private bool _inSubmenu;
    private int _activeRootIndex;

    public HomeScreenModule(
        ShellViewAdapter shellView,
        IReadOnlyList<HomeScreenEntry> entries,
        Action<AppletId> openApplet,
        Action showExitPrompt,
        Action<string> announce)
    {
        _shellView = shellView ?? throw new ArgumentNullException(nameof(shellView));
        _rootEntries = entries ?? throw new ArgumentNullException(nameof(entries));
        _openApplet = openApplet ?? throw new ArgumentNullException(nameof(openApplet));
        _showExitPrompt = showExitPrompt ?? throw new ArgumentNullException(nameof(showExitPrompt));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
        _activeEntries = _rootEntries;
    }

    public void ShowHomeScreen(int focusIndex, bool shouldAnnounce)
    {
        _activeEntries = _rootEntries;
        _inSubmenu = false;
        _activeRootIndex = focusIndex;
        _shellView.UpdateHomeScreenItems(_activeEntries);
        _shellView.ShowHomeScreenScreen();
        SetSelection(focusIndex, shouldAnnounce: false);

        if (shouldAnnounce && _shellView.HomeScreenSelectedIndex >= 0)
        {
            _announce($"Home screen. {_activeEntries[_shellView.HomeScreenSelectedIndex].Label} selected.");
        }
    }

    public void RestoreFocus()
    {
        var index = _shellView.HomeScreenSelectedIndex < 0 ? 0 : _shellView.HomeScreenSelectedIndex;
        SetSelection(index, shouldAnnounce: false);
    }

    public bool HandleInput(Key key)
    {
        if (_shellView.HomeScreenSelectedIndex < 0)
        {
            SetSelection(0, shouldAnnounce: false);
        }

        if (!InputCommandRouter.TryGetHomeScreenCommand(key, out var command))
        {
            return false;
        }

        switch (command)
        {
            case HomeScreenInputCommand.MoveUp:
                SetSelection(_shellView.HomeScreenSelectedIndex - 1);
                return true;
            case HomeScreenInputCommand.MoveDown:
                SetSelection(_shellView.HomeScreenSelectedIndex + 1);
                return true;
            case HomeScreenInputCommand.MoveHome:
                SetSelection(0);
                return true;
            case HomeScreenInputCommand.MoveEnd:
                SetSelection(_activeEntries.Count - 1);
                return true;
            case HomeScreenInputCommand.ActivateSelection:
                ActivateSelection();
                return true;
            case HomeScreenInputCommand.ShowExitPrompt:
                if (_inSubmenu)
                {
                    ReturnToRootMenu();
                    return true;
                }
                _showExitPrompt();
                return true;
            default:
                return false;
        }
    }

    private void SetSelection(int index, bool shouldAnnounce = true)
    {
        var selectedIndex = _shellView.SetHomeScreenSelection(index, _activeEntries);
        if (selectedIndex < 0)
        {
            return;
        }

        if (shouldAnnounce)
        {
            _announce(_activeEntries[selectedIndex].Label);
        }
    }

    private void ActivateSelection()
    {
        var selectedIndex = _shellView.HomeScreenSelectedIndex < 0 ? 0 : _shellView.HomeScreenSelectedIndex;
        var selectedEntry = _activeEntries[selectedIndex];

        if (selectedEntry.Id == HomeScreenEntryId.Applet && selectedEntry.AppletId.HasValue)
        {
            _openApplet(selectedEntry.AppletId.Value);
            return;
        }

        switch (selectedEntry.Id)
        {
            case HomeScreenEntryId.Submenu:
                EnterSubmenu(selectedEntry);
                return;
            case HomeScreenEntryId.Exit:
                _showExitPrompt();
                return;
            case HomeScreenEntryId.Utilities:
                _announce("Utilities is not implemented yet.");
                return;
            default:
                _announce("Unknown menu action.");
                return;
        }
    }

    private void EnterSubmenu(HomeScreenEntry submenuEntry)
    {
        var selectedIndex = _shellView.HomeScreenSelectedIndex;
        if (selectedIndex >= 0)
        {
            _activeRootIndex = selectedIndex;
        }

        _activeEntries = submenuEntry.Children;
        _inSubmenu = true;
        _shellView.UpdateHomeScreenItems(_activeEntries);
        _shellView.ShowHomeScreenScreen();
        SetSelection(0, shouldAnnounce: false);
        _announce($"{submenuEntry.Label}. {_activeEntries[0].Label} selected.");
    }

    private void ReturnToRootMenu()
    {
        _activeEntries = _rootEntries;
        _inSubmenu = false;
        _shellView.UpdateHomeScreenItems(_activeEntries);
        _shellView.ShowHomeScreenScreen();

        var rootIndex = _activeRootIndex;
        if (rootIndex < 0 || rootIndex >= _rootEntries.Count)
        {
            rootIndex = 0;
        }

        SetSelection(rootIndex, shouldAnnounce: false);
        _announce($"Home screen. {_rootEntries[rootIndex].Label} selected.");
    }
}
