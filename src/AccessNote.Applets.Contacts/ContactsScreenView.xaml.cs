using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AccessNote;

internal partial class ContactsScreenView : UserControl
{
    public ContactsScreenView()
    {
        InitializeComponent();
    }

    internal ObservableCollection<Contact> VisibleContacts { get; } = new();

    internal ListBox ContactsListControl => ContactsList;
    internal TextBox ContactSearchBoxControl => ContactSearchBox;
    internal ComboBox GroupFilterComboControl => GroupFilterCombo;
    internal TextBox FirstNameBoxControl => FirstNameBox;
    internal TextBox LastNameBoxControl => LastNameBox;
    internal TextBox PhoneBoxControl => PhoneBox;
    internal TextBox EmailBoxControl => EmailBox;
    internal TextBox AddressBoxControl => AddressBox;
    internal TextBox NotesBoxControl => NotesBox;
    internal ComboBox GroupComboControl => GroupCombo;
    internal Button SaveButtonControl => SaveButton;
    internal Button DeleteButtonControl => DeleteButton;
    internal Button BackButtonControl => BackButton;

    internal Action? SearchTextChanged { get; set; }
    internal Action? ContactsSelectionChanged { get; set; }
    internal Action? GroupFilterChanged { get; set; }
    internal Action? SaveClicked { get; set; }
    internal Action? DeleteClicked { get; set; }
    internal Action? BackClicked { get; set; }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        SearchTextChanged?.Invoke();
    }

    private void OnContactsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ContactsSelectionChanged?.Invoke();
    }

    private void OnGroupFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        GroupFilterChanged?.Invoke();
    }

    private void OnSaveClicked(object sender, System.Windows.RoutedEventArgs e)
    {
        SaveClicked?.Invoke();
    }

    private void OnDeleteClicked(object sender, System.Windows.RoutedEventArgs e)
    {
        DeleteClicked?.Invoke();
    }

    private void OnBackClicked(object sender, System.Windows.RoutedEventArgs e)
    {
        BackClicked?.Invoke();
    }
}
