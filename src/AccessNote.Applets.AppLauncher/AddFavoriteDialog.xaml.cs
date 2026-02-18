using System.Windows;
using System.Windows.Threading;
using System.IO;

namespace AccessNote;

internal partial class AddFavoriteDialog : Window
{
    private readonly bool _canUseCurrentSelection;
    private readonly string _currentSelectionDisplayName;
    private readonly string _currentSelectionPath;

    public AddFavoriteDialog(bool canUseCurrentSelection, string currentSelectionDisplayName, string currentSelectionPath)
    {
        _canUseCurrentSelection = canUseCurrentSelection;
        _currentSelectionDisplayName = currentSelectionDisplayName ?? string.Empty;
        _currentSelectionPath = currentSelectionPath ?? string.Empty;

        InitializeComponent();
    }

    internal AddFavoriteSource SelectedSource { get; private set; } = AddFavoriteSource.Browse;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UseSelectionButton.IsEnabled = _canUseCurrentSelection;
        SelectionSummaryText.Text = BuildSelectionSummaryText();

        Dispatcher.BeginInvoke(
            () =>
            {
                if (_canUseCurrentSelection)
                {
                    UseSelectionButton.Focus();
                    return;
                }

                BrowseButton.Focus();
            },
            DispatcherPriority.Input);
    }

    private void OnUseSelectionClick(object sender, RoutedEventArgs e)
    {
        SelectedSource = AddFavoriteSource.CurrentSelection;
        DialogResult = true;
        Close();
    }

    private void OnBrowseClick(object sender, RoutedEventArgs e)
    {
        SelectedSource = AddFavoriteSource.Browse;
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private string BuildSelectionSummaryText()
    {
        if (_canUseCurrentSelection)
        {
            var name = string.IsNullOrWhiteSpace(_currentSelectionDisplayName)
                ? ResolveSelectionLabelFromPath(_currentSelectionPath)
                : _currentSelectionDisplayName;
            return $"Current selection: {name}.";
        }

        return "Current selection is not a launchable file. Choose Browse to pick an app.";
    }

    private static string ResolveSelectionLabelFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "Current item";
        }

        return Path.GetFileNameWithoutExtension(path);
    }
}
