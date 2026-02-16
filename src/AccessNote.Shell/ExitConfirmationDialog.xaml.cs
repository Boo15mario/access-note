using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal partial class ExitConfirmationDialog : Window
{
    public ExitConfirmationDialog()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() => YesButton.Focus(), DispatcherPriority.Input);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            DialogResult = false;
            Close();
            return;
        }
    }

    private void OnYesClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnNoClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
