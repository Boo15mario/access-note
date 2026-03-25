using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AccessNote;

public sealed class ShellViewAdapter
{
    private readonly FrameworkElement _homeScreen;
    private readonly ListBox _homeScreenList;
    private readonly Dispatcher _dispatcher;
    private readonly Dictionary<AppletId, FrameworkElement> _appletScreens = new();

    internal ShellViewAdapter(
        FrameworkElement homeScreen,
        ListBox homeScreenList,
        Dispatcher dispatcher)
    {
        _homeScreen = homeScreen;
        _homeScreenList = homeScreenList;
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

    internal int HomeScreenSelectedIndex => _homeScreenList.SelectedIndex;

    internal void ShowHomeScreenScreen()
    {
        HideAllAppletScreens();
        _homeScreen.Visibility = Visibility.Visible;
    }

    internal void UpdateHomeScreenItems(IReadOnlyList<HomeScreenEntry> entries)
    {
        _homeScreenList.ItemsSource = entries;
    }

    public void ShowAppletScreen(AppletId appletId)
    {
        _homeScreen.Visibility = Visibility.Collapsed;
        HideAllAppletScreens();
        if (_appletScreens.TryGetValue(appletId, out var screen))
        {
            screen.Visibility = Visibility.Visible;
        }
    }

    // Keep legacy methods for backward compat with existing applet code
    public void ShowNotesScreen() => ShowAppletScreen(AppletId.Notes);

    public void ShowSettingsScreen() => ShowAppletScreen(AppletId.Settings);

    internal int SetHomeScreenSelection(int requestedIndex, IReadOnlyList<HomeScreenEntry> entries)
    {
        if (entries.Count == 0)
        {
            return -1;
        }

        var selectedIndex = WrapIndex(requestedIndex, entries.Count);
        _homeScreenList.SelectedIndex = selectedIndex;
        _homeScreenList.ScrollIntoView(entries[selectedIndex]);

        _dispatcher.BeginInvoke(() =>
        {
            if (_homeScreenList.ItemContainerGenerator.ContainerFromIndex(selectedIndex) is ListBoxItem item)
            {
                item.Focus();
            }
            else
            {
                _homeScreenList.Focus();
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
