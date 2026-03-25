using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AccessNote;

internal enum ExitOption
{
    ExitApplication,
    ShutDownComputer,
    Cancel
}

internal partial class ExitOptionsDialog : Window
{
    public ExitOption SelectedOption { get; private set; } = ExitOption.Cancel;

    public ExitOptionsDialog()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            OptionsList.SelectedIndex = 0;
            if (OptionsList.ItemContainerGenerator.ContainerFromIndex(0) is System.Windows.Controls.ListBoxItem item)
            {
                item.Focus();
            }
        }, System.Windows.Threading.DispatcherPriority.Input);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                e.Handled = true;
                SelectedOption = ExitOption.Cancel;
                Close();
                return;

            case Key.Enter:
                e.Handled = true;
                SelectCurrentItem();
                return;

            case Key.Up:
                if (OptionsList.SelectedIndex > 0)
                {
                    OptionsList.SelectedIndex--;
                    e.Handled = true;
                }
                return;

            case Key.Down:
                if (OptionsList.SelectedIndex < OptionsList.Items.Count - 1)
                {
                    OptionsList.SelectedIndex++;
                    e.Handled = true;
                }
                return;

            case Key.Home:
                OptionsList.SelectedIndex = 0;
                e.Handled = true;
                return;

            case Key.End:
                OptionsList.SelectedIndex = OptionsList.Items.Count - 1;
                e.Handled = true;
                return;
        }
    }

    private void SelectCurrentItem()
    {
        switch (OptionsList.SelectedIndex)
        {
            case 0:
                SelectedOption = ExitOption.ExitApplication;
                break;
            case 1:
                SelectedOption = ExitOption.ShutDownComputer;
                break;
            default:
                SelectedOption = ExitOption.Cancel;
                break;
        }

        if (SelectedOption == ExitOption.ShutDownComputer)
        {
            var result = MessageBox.Show(
                "This will shut down your computer. Are you sure?",
                "Confirm Shut Down",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                SelectedOption = ExitOption.Cancel;
                Close();
                return;
            }
        }

        Close();
    }
}
