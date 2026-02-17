using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AccessNote;

internal sealed class MainMenuModule
{
    private readonly ShellViewAdapter _shellView;
    private readonly IReadOnlyList<MainMenuEntry> _rootEntries;
    private readonly Action<AppletId> _openApplet;
    private readonly Action _showExitPrompt;
    private readonly Action<string> _announce;

    private IReadOnlyList<MainMenuEntry> _activeEntries;
    private bool _inSubmenu;

    public MainMenuModule(
        ShellViewAdapter shellView,
        IReadOnlyList<MainMenuEntry> entries,
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

    public void ShowMainMenu(int focusIndex, bool shouldAnnounce)
    {
        _activeEntries = _rootEntries;
        _inSubmenu = false;
        _shellView.UpdateMainMenuItems(_activeEntries);
        _shellView.ShowMainMenuScreen();
        SetSelection(focusIndex, shouldAnnounce: false);

        if (shouldAnnounce && _shellView.MainMenuSelectedIndex >= 0)
        {
            _announce($"Main menu. {_activeEntries[_shellView.MainMenuSelectedIndex].Label} selected.");
        }
    }

    public void RestoreFocus()
    {
        var index = _shellView.MainMenuSelectedIndex < 0 ? 0 : _shellView.MainMenuSelectedIndex;
        SetSelection(index, shouldAnnounce: false);
    }

    public bool HandleInput(Key key)
    {
        if (_shellView.MainMenuSelectedIndex < 0)
        {
            SetSelection(0, shouldAnnounce: false);
        }

        if (!InputCommandRouter.TryGetMainMenuCommand(key, out var command))
        {
            return false;
        }

        switch (command)
        {
            case MainMenuInputCommand.MoveUp:
                SetSelection(_shellView.MainMenuSelectedIndex - 1);
                return true;
            case MainMenuInputCommand.MoveDown:
                SetSelection(_shellView.MainMenuSelectedIndex + 1);
                return true;
            case MainMenuInputCommand.MoveHome:
                SetSelection(0);
                return true;
            case MainMenuInputCommand.MoveEnd:
                SetSelection(_activeEntries.Count - 1);
                return true;
            case MainMenuInputCommand.ActivateSelection:
                ActivateSelection();
                return true;
            case MainMenuInputCommand.ShowExitPrompt:
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
        var selectedIndex = _shellView.SetMainMenuSelection(index, _activeEntries);
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
        var selectedIndex = _shellView.MainMenuSelectedIndex < 0 ? 0 : _shellView.MainMenuSelectedIndex;
        var selectedEntry = _activeEntries[selectedIndex];

        if (selectedEntry.Id == MainMenuEntryId.Applet && selectedEntry.AppletId.HasValue)
        {
            _openApplet(selectedEntry.AppletId.Value);
            return;
        }

        switch (selectedEntry.Id)
        {
            case MainMenuEntryId.Submenu:
                EnterSubmenu(selectedEntry);
                return;
            case MainMenuEntryId.Exit:
                _showExitPrompt();
                return;
            case MainMenuEntryId.Utilities:
                _announce("Utilities is not implemented yet.");
                return;
            default:
                _announce("Unknown menu action.");
                return;
        }
    }

    private void EnterSubmenu(MainMenuEntry submenuEntry)
    {
        _activeEntries = submenuEntry.Children;
        _inSubmenu = true;
        _shellView.UpdateMainMenuItems(_activeEntries);
        _shellView.ShowMainMenuScreen();
        SetSelection(0, shouldAnnounce: false);
        _announce($"{submenuEntry.Label}. {_activeEntries[0].Label} selected.");
    }

    private void ReturnToRootMenu()
    {
        _activeEntries = _rootEntries;
        _inSubmenu = false;
        _shellView.UpdateMainMenuItems(_activeEntries);
        _shellView.ShowMainMenuScreen();

        // Try to focus the Utilities submenu entry
        int utilitiesIndex = 0;
        for (int i = 0; i < _rootEntries.Count; i++)
        {
            if (_rootEntries[i].Id == MainMenuEntryId.Submenu)
            {
                utilitiesIndex = i;
                break;
            }
        }

        SetSelection(utilitiesIndex, shouldAnnounce: false);
        _announce($"Main menu. {_rootEntries[utilitiesIndex].Label} selected.");
    }
}
