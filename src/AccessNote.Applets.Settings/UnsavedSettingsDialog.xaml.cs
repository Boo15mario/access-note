using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

public partial class UnsavedSettingsDialog : Window
{
    public UnsavedSettingsDialog()
    {
        InitializeComponent();
        Choice = UnsavedChangesChoice.Cancel;
    }

    public UnsavedChangesChoice Choice { get; private set; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() => SaveButton.Focus(), DispatcherPriority.Input);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Choice = UnsavedChangesChoice.Cancel;
            DialogResult = false;
            Close();
        }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        Choice = UnsavedChangesChoice.Save;
        DialogResult = true;
        Close();
    }

    private void OnDiscardClick(object sender, RoutedEventArgs e)
    {
        Choice = UnsavedChangesChoice.Discard;
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Choice = UnsavedChangesChoice.Cancel;
        DialogResult = false;
        Close();
    }
}
