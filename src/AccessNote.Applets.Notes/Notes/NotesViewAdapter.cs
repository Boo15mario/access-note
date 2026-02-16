using System.Windows;
using System.Windows.Controls;

namespace AccessNote;

internal sealed class NotesViewAdapter
{
    private readonly ListBox _notesList;
    private readonly TextBox _searchBox;
    private readonly TextBox _editorTextBox;
    private readonly TextBlock _editorTitleText;
    private readonly FrameworkElement _statusRegion;
    private readonly FrameworkElement _statusText;

    public NotesViewAdapter(
        ListBox notesList,
        TextBox searchBox,
        TextBox editorTextBox,
        TextBlock editorTitleText,
        FrameworkElement statusRegion,
        FrameworkElement statusText)
    {
        _notesList = notesList;
        _searchBox = searchBox;
        _editorTextBox = editorTextBox;
        _editorTitleText = editorTitleText;
        _statusRegion = statusRegion;
        _statusText = statusText;
    }

    public bool IsSearchFocusedWithin => _searchBox.IsKeyboardFocusWithin;

    public bool IsEditorFocusedWithin => _editorTextBox.IsKeyboardFocusWithin;

    public bool IsStatusFocusedWithin => _statusRegion.IsKeyboardFocusWithin || _statusText.IsKeyboardFocusWithin;

    public bool IsFocusInListRegion => _notesList.IsKeyboardFocusWithin || _searchBox.IsKeyboardFocusWithin;

    public string SearchText => _searchBox.Text;

    public string EditorText => _editorTextBox.Text;

    public NoteDocument? GetSelectedNote()
    {
        return _notesList.SelectedItem as NoteDocument;
    }

    public void SetSearchText(string text)
    {
        _searchBox.Text = text;
    }

    public void SetEditorTitle(string title)
    {
        _editorTitleText.Text = title;
    }

    public void SetEditorText(string text)
    {
        _editorTextBox.Text = text;
        _editorTextBox.CaretIndex = _editorTextBox.Text.Length;
    }

    public void FocusNotesListSelectionOrList()
    {
        if (_notesList.SelectedIndex >= 0 &&
            _notesList.ItemContainerGenerator.ContainerFromIndex(_notesList.SelectedIndex) is ListBoxItem item)
        {
            item.Focus();
            return;
        }

        _notesList.Focus();
    }

    public void FocusEditorToEnd()
    {
        _editorTextBox.Focus();
        _editorTextBox.CaretIndex = _editorTextBox.Text.Length;
    }

    public void FocusSearchSelectAll()
    {
        _searchBox.Focus();
        _searchBox.SelectAll();
    }

    public void FocusStatusRegion()
    {
        _statusRegion.Focus();
    }

    public void SelectNote(NoteDocument note)
    {
        _notesList.SelectedItem = note;
        _notesList.ScrollIntoView(note);
    }

    public void ClearNoteSelection()
    {
        _notesList.SelectedItem = null;
    }
}
