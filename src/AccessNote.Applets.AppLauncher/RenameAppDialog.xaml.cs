using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

public partial class RenameAppDialog : Window
{
    public RenameAppDialog(string currentName)
    {
        InitializeComponent();
        NameTextBox.Text = currentName;
    }

    public string AppName { get; private set; } = string.Empty;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        }, DispatcherPriority.Input);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.FocusedElement == NameTextBox)
        {
            e.Handled = true;
            Confirm();
        }
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Confirm();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Confirm()
    {
        var name = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
            return;
        }

        AppName = name;
        DialogResult = true;
        Close();
    }
}
