using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AccessNote;

public sealed class ShellViewAdapter
{
    private readonly FrameworkElement _mainMenuScreen;
    private readonly FrameworkElement _notesScreen;
    private readonly FrameworkElement _settingsScreen;
    private readonly ListBox _mainMenuList;
    private readonly Dispatcher _dispatcher;

    internal ShellViewAdapter(
        FrameworkElement mainMenuScreen,
        FrameworkElement notesScreen,
        FrameworkElement settingsScreen,
        ListBox mainMenuList,
        Dispatcher dispatcher)
    {
        _mainMenuScreen = mainMenuScreen;
        _notesScreen = notesScreen;
        _settingsScreen = settingsScreen;
        _mainMenuList = mainMenuList;
        _dispatcher = dispatcher;
    }

    internal int MainMenuSelectedIndex => _mainMenuList.SelectedIndex;

    internal void ShowMainMenuScreen()
    {
        _settingsScreen.Visibility = Visibility.Collapsed;
        _notesScreen.Visibility = Visibility.Collapsed;
        _mainMenuScreen.Visibility = Visibility.Visible;
    }

    public void ShowNotesScreen()
    {
        _mainMenuScreen.Visibility = Visibility.Collapsed;
        _settingsScreen.Visibility = Visibility.Collapsed;
        _notesScreen.Visibility = Visibility.Visible;
    }

    public void ShowSettingsScreen()
    {
        _mainMenuScreen.Visibility = Visibility.Collapsed;
        _notesScreen.Visibility = Visibility.Collapsed;
        _settingsScreen.Visibility = Visibility.Visible;
    }

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

    private static int WrapIndex(int index, int count)
    {
        return ((index % count) + count) % count;
    }
}
