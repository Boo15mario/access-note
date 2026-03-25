using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace AccessNote;

internal partial class ContextMenuDialog : Window
{
    private readonly string _appletName;
    private readonly Action<string> _onCommand;

    public string SelectedCommand { get; private set; } = string.Empty;

    public ContextMenuDialog(string appletName, IEnumerable<string> menuItems, Action<string> onCommand)
    {
        _appletName = appletName ?? throw new ArgumentNullException(nameof(appletName));
        _onCommand = onCommand ?? throw new ArgumentNullException(nameof(onCommand));

        InitializeComponent();

        var items = new List<string>(menuItems);
        MenuItemsList.ItemsSource = items;

        AppletHeaderText.Text = $"{_appletName} Options";
        AutomationProperties.SetName(AppletHeaderText, $"Context menu for {_appletName}");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (MenuItemsList.Items.Count > 0)
            {
                MenuItemsList.SelectedIndex = 0;
                if (MenuItemsList.ItemContainerGenerator.ContainerFromIndex(0) is System.Windows.Controls.ListBoxItem item)
                {
                    item.Focus();
                }
            }
        }, System.Windows.Threading.DispatcherPriority.Input);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                e.Handled = true;
                Close();
                return;

            case Key.Enter:
                e.Handled = true;
                SelectCurrentItem();
                return;

            case Key.Up:
                if (MenuItemsList.SelectedIndex > 0)
                {
                    MenuItemsList.SelectedIndex--;
                    e.Handled = true;
                }
                return;

            case Key.Down:
                if (MenuItemsList.SelectedIndex < MenuItemsList.Items.Count - 1)
                {
                    MenuItemsList.SelectedIndex++;
                    e.Handled = true;
                }
                return;

            case Key.Home:
                MenuItemsList.SelectedIndex = 0;
                e.Handled = true;
                return;

            case Key.End:
                MenuItemsList.SelectedIndex = MenuItemsList.Items.Count - 1;
                e.Handled = true;
                return;
        }
    }

    private void OnMenuItemDoubleClick(object sender, MouseButtonEventArgs e)
    {
        SelectCurrentItem();
    }

    private void SelectCurrentItem()
    {
        if (MenuItemsList.SelectedItem is string command)
        {
            SelectedCommand = command;
            _onCommand(command);
            Close();
        }
    }
}
