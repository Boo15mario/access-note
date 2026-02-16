using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AccessNote;

public partial class NotesScreenView : UserControl
{
    public NotesScreenView()
    {
        InitializeComponent();
    }

    internal ObservableCollection<NoteDocument> VisibleNotes { get; } = new();

    internal ListBox NotesListControl => NotesList;
    internal TextBox NoteSearchBoxControl => NoteSearchBox;
    internal TextBox EditorTextBoxControl => EditorTextBox;
    internal TextBlock EditorTitleTextControl => EditorTitleText;

    internal Action? SearchTextChanged { get; set; }
    internal Action? NotesSelectionChanged { get; set; }
    internal Action? EditorTextChanged { get; set; }

    private void OnNoteSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        SearchTextChanged?.Invoke();
    }

    private void OnNotesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NotesSelectionChanged?.Invoke();
    }

    private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        EditorTextChanged?.Invoke();
    }
}
