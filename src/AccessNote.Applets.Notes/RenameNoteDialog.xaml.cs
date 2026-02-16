using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

public partial class RenameNoteDialog : Window
{
    public RenameNoteDialog(string currentTitle)
    {
        InitializeComponent();
        NameTextBox.Text = currentTitle;
    }

    public string NoteTitle { get; private set; } = string.Empty;

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
        var title = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
            return;
        }

        NoteTitle = title;
        DialogResult = true;
        Close();
    }
}
