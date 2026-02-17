using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AccessNote;

public sealed class ShellViewAdapter
{
    private readonly FrameworkElement _mainMenuScreen;
    private readonly ListBox _mainMenuList;
    private readonly Dispatcher _dispatcher;
    private readonly Dictionary<AppletId, FrameworkElement> _appletScreens = new();

    internal ShellViewAdapter(
        FrameworkElement mainMenuScreen,
        ListBox mainMenuList,
        Dispatcher dispatcher)
    {
        _mainMenuScreen = mainMenuScreen;
        _mainMenuList = mainMenuList;
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Registers an applet screen element so it can be shown/hidden by applet id.
    /// </summary>
    public void RegisterScreen(AppletId appletId, FrameworkElement screen)
    {
        ArgumentNullException.ThrowIfNull(screen);
        _appletScreens[appletId] = screen;
    }

    internal int MainMenuSelectedIndex => _mainMenuList.SelectedIndex;

    internal void ShowMainMenuScreen()
    {
        HideAllAppletScreens();
        _mainMenuScreen.Visibility = Visibility.Visible;
    }

    internal void UpdateMainMenuItems(IReadOnlyList<MainMenuEntry> entries)
    {
        _mainMenuList.ItemsSource = entries;
    }

    public void ShowAppletScreen(AppletId appletId)
    {
        _mainMenuScreen.Visibility = Visibility.Collapsed;
        HideAllAppletScreens();
        if (_appletScreens.TryGetValue(appletId, out var screen))
        {
            screen.Visibility = Visibility.Visible;
        }
    }

    // Keep legacy methods for backward compat with existing applet code
    public void ShowNotesScreen() => ShowAppletScreen(AppletId.Notes);

    public void ShowSettingsScreen() => ShowAppletScreen(AppletId.Settings);

    internal int SetMainMenuSelection(int requestedIndex, IReadOnlyList<MainMenuEntry> entries)
    {
        if (entries.Count == 0)
        {
            return -1;
        }

        var selectedIndex = WrapIndex(requestedIndex, entries.Count);
        _mainMenuList.SelectedIndex = selectedIndex;
        _mainMenuList.ScrollIntoView(entries[selectedIndex]);

        _dispatcher.BeginInvoke(() =>
        {
            if (_mainMenuList.ItemContainerGenerator.ContainerFromIndex(selectedIndex) is ListBoxItem item)
            {
                item.Focus();
            }
            else
            {
                _mainMenuList.Focus();
            }
        }, DispatcherPriority.Input);

        return selectedIndex;
    }

    private void HideAllAppletScreens()
    {
        foreach (var screen in _appletScreens.Values)
        {
            screen.Visibility = Visibility.Collapsed;
        }
    }

    private static int WrapIndex(int index, int count)
    {
        return ((index % count) + count) % count;
    }
}
