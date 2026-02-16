using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class ContactsModule
{
    private const string AllGroupsFilter = "(All)";

    private readonly ContactStorage _storage;
    private readonly ContactsScreenView _screenView;
    private readonly Action<string> _announce;
    private readonly Dispatcher _dispatcher;
    private readonly Action _showMainMenu;

    private Contact? _selectedContact;
    private bool _suppressSelectionChanged;
    private bool _suppressSearchChanged;
    private bool _suppressGroupFilterChanged;

    public ContactsModule(
        ContactStorage storage,
        ContactsScreenView screenView,
        Action<string> announce,
        Dispatcher dispatcher,
        Action showMainMenu)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _screenView = screenView ?? throw new ArgumentNullException(nameof(screenView));
        _announce = announce ?? throw new ArgumentNullException(nameof(announce));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _showMainMenu = showMainMenu ?? throw new ArgumentNullException(nameof(showMainMenu));

        _screenView.SearchTextChanged = OnSearchTextChanged;
        _screenView.ContactsSelectionChanged = OnContactsSelectionChanged;
        _screenView.GroupFilterChanged = OnGroupFilterChanged;
        _screenView.SaveClicked = SaveContact;
        _screenView.DeleteClicked = DeleteContact;
        _screenView.BackClicked = () => _showMainMenu();
    }

    public void Enter()
    {
        RefreshGroups();
        RefreshContacts();
        ClearForm();
        _dispatcher.BeginInvoke(() => _screenView.ContactsListControl.Focus(), DispatcherPriority.Input);
    }

    public void RestoreFocus()
    {
        if (_selectedContact != null)
        {
            _screenView.FirstNameBoxControl.Focus();
        }
        else
        {
            _screenView.ContactsListControl.Focus();
        }
    }

    public bool CanLeave()
    {
        return true;
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        if (key == Key.N && modifiers == ModifierKeys.Control)
        {
            NewContact();
            return true;
        }

        if (key == Key.S && modifiers == ModifierKeys.Control)
        {
            SaveContact();
            return true;
        }

        if (key == Key.Delete && modifiers == ModifierKeys.None)
        {
            if (_screenView.ContactsListControl.IsKeyboardFocusWithin)
            {
                DeleteContact();
                return true;
            }
        }

        if (key == Key.F && modifiers == ModifierKeys.Control)
        {
            _screenView.ContactSearchBoxControl.Focus();
            _screenView.ContactSearchBoxControl.SelectAll();
            return true;
        }

        if (key == Key.I && modifiers == ModifierKeys.Control)
        {
            ImportContacts();
            return true;
        }

        if (key == Key.E && modifiers == ModifierKeys.Control)
        {
            ExportContacts();
            return true;
        }

        if (key == Key.F6 && modifiers == ModifierKeys.None)
        {
            CycleFocus();
            return true;
        }

        if (key == Key.Escape)
        {
            _showMainMenu();
            return true;
        }

        return false;
    }

    private void NewContact()
    {
        _selectedContact = null;
        _suppressSelectionChanged = true;
        _screenView.ContactsListControl.SelectedIndex = -1;
        _suppressSelectionChanged = false;
        ClearForm();
        _screenView.FirstNameBoxControl.Focus();
        _announce("New contact.");
    }

    private void SaveContact()
    {
        var contact = _selectedContact ?? new Contact();
        LoadFormIntoContact(contact);

        try
        {
            if (contact.Id == 0)
            {
                _storage.AddContact(contact);
                _selectedContact = contact;
                _announce($"{contact.DisplayName} added.");
            }
            else
            {
                _storage.UpdateContact(contact);
                _announce($"{contact.DisplayName} saved.");
            }
        }
        catch (Exception ex)
        {
            _announce($"Error saving contact: {ex.Message}");
            return;
        }

        RefreshGroups();
        RefreshContacts();
        SelectContactInList(contact);
    }

    private void DeleteContact()
    {
        if (_selectedContact == null || _selectedContact.Id == 0)
        {
            _announce("No contact selected.");
            return;
        }

        var result = MessageBox.Show(
            $"Delete {_selectedContact.DisplayName}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            _storage.DeleteContact(_selectedContact.Id);
            _announce($"{_selectedContact.DisplayName} deleted.");
        }
        catch (Exception ex)
        {
            _announce($"Error deleting contact: {ex.Message}");
            return;
        }

        _selectedContact = null;
        ClearForm();
        RefreshContacts();
        _screenView.ContactsListControl.Focus();
    }

    private void ImportContacts()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "vCard Files (*.vcf)|*.vcf|All Files (*.*)|*.*",
            Title = "Import Contacts"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var imported = VCardImporter.Import(dialog.FileName);
            foreach (var contact in imported)
            {
                _storage.AddContact(contact);
            }
            _announce($"{imported.Count} contact(s) imported.");
        }
        catch (Exception ex)
        {
            _announce($"Import error: {ex.Message}");
            return;
        }

        RefreshGroups();
        RefreshContacts();
    }

    private void ExportContacts()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "vCard Files (*.vcf)|*.vcf",
            Title = "Export Contacts",
            FileName = "contacts.vcf"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var contacts = _storage.GetAllContacts();
            VCardExporter.Export(contacts, dialog.FileName);
            _announce($"{contacts.Count} contact(s) exported.");
        }
        catch (Exception ex)
        {
            _announce($"Export error: {ex.Message}");
        }
    }

    private void CycleFocus()
    {
        if (_screenView.ContactsListControl.IsKeyboardFocusWithin)
        {
            _screenView.FirstNameBoxControl.Focus();
        }
        else if (_screenView.SaveButtonControl.IsKeyboardFocusWithin
                 || _screenView.DeleteButtonControl.IsKeyboardFocusWithin
                 || _screenView.BackButtonControl.IsKeyboardFocusWithin)
        {
            _screenView.ContactsListControl.Focus();
        }
        else
        {
            _screenView.SaveButtonControl.Focus();
        }
    }

    private void OnSearchTextChanged()
    {
        if (_suppressSearchChanged) return;
        RefreshContacts();
    }

    private void OnContactsSelectionChanged()
    {
        if (_suppressSelectionChanged) return;

        var selected = _screenView.ContactsListControl.SelectedItem as Contact;
        if (selected == null) return;

        _selectedContact = selected;
        LoadContactIntoForm(selected);
        _announce($"{selected.DisplayName} selected.");
    }

    private void OnGroupFilterChanged()
    {
        if (_suppressGroupFilterChanged) return;
        RefreshContacts();
    }

    private void RefreshContacts()
    {
        var searchText = _screenView.ContactSearchBoxControl.Text;
        var groupFilter = _screenView.GroupFilterComboControl.SelectedItem as string;

        IReadOnlyList<Contact> contacts;
        try
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                contacts = _storage.SearchContacts(searchText);
            }
            else if (!string.IsNullOrEmpty(groupFilter) && groupFilter != AllGroupsFilter)
            {
                contacts = _storage.GetContactsByGroup(groupFilter);
            }
            else
            {
                contacts = _storage.GetAllContacts();
            }
        }
        catch (Exception ex)
        {
            _announce($"Error loading contacts: {ex.Message}");
            return;
        }

        _suppressSelectionChanged = true;
        _screenView.VisibleContacts.Clear();
        foreach (var c in contacts)
        {
            _screenView.VisibleContacts.Add(c);
        }
        _screenView.ContactsListControl.ItemsSource = _screenView.VisibleContacts;
        _suppressSelectionChanged = false;

        if (_selectedContact != null)
        {
            SelectContactInList(_selectedContact);
        }
    }

    private void RefreshGroups()
    {
        IReadOnlyList<ContactGroup> groups;
        try
        {
            groups = _storage.GetGroups();
        }
        catch
        {
            return;
        }

        var groupNames = new List<string> { AllGroupsFilter };
        groupNames.AddRange(groups.Select(g => g.Name));

        _suppressGroupFilterChanged = true;
        var previousFilter = _screenView.GroupFilterComboControl.SelectedItem as string;
        _screenView.GroupFilterComboControl.ItemsSource = groupNames;
        if (previousFilter != null && groupNames.Contains(previousFilter))
        {
            _screenView.GroupFilterComboControl.SelectedItem = previousFilter;
        }
        else
        {
            _screenView.GroupFilterComboControl.SelectedIndex = 0;
        }
        _suppressGroupFilterChanged = false;

        // Update group combo in form
        var formGroupNames = groups.Select(g => g.Name).ToList();
        _screenView.GroupComboControl.ItemsSource = formGroupNames;
    }

    private void LoadContactIntoForm(Contact contact)
    {
        _screenView.FirstNameBoxControl.Text = contact.FirstName;
        _screenView.LastNameBoxControl.Text = contact.LastName;
        _screenView.PhoneBoxControl.Text = contact.Phone;
        _screenView.EmailBoxControl.Text = contact.Email;
        _screenView.AddressBoxControl.Text = contact.Address;
        _screenView.NotesBoxControl.Text = contact.Notes;
        _screenView.GroupComboControl.Text = contact.GroupName;
    }

    private void LoadFormIntoContact(Contact contact)
    {
        contact.FirstName = _screenView.FirstNameBoxControl.Text;
        contact.LastName = _screenView.LastNameBoxControl.Text;
        contact.Phone = _screenView.PhoneBoxControl.Text;
        contact.Email = _screenView.EmailBoxControl.Text;
        contact.Address = _screenView.AddressBoxControl.Text;
        contact.Notes = _screenView.NotesBoxControl.Text;
        contact.GroupName = _screenView.GroupComboControl.Text;

        // Ensure group exists if specified
        if (!string.IsNullOrWhiteSpace(contact.GroupName))
        {
            var groups = _storage.GetGroups();
            if (!groups.Any(g => g.Name == contact.GroupName))
            {
                _storage.AddGroup(new ContactGroup { Name = contact.GroupName });
            }
        }
    }

    private void ClearForm()
    {
        _screenView.FirstNameBoxControl.Text = string.Empty;
        _screenView.LastNameBoxControl.Text = string.Empty;
        _screenView.PhoneBoxControl.Text = string.Empty;
        _screenView.EmailBoxControl.Text = string.Empty;
        _screenView.AddressBoxControl.Text = string.Empty;
        _screenView.NotesBoxControl.Text = string.Empty;
        _screenView.GroupComboControl.Text = string.Empty;
    }

    private void SelectContactInList(Contact contact)
    {
        _suppressSelectionChanged = true;
        foreach (Contact c in _screenView.VisibleContacts)
        {
            if (c.Id == contact.Id)
            {
                _screenView.ContactsListControl.SelectedItem = c;
                break;
            }
        }
        _suppressSelectionChanged = false;
    }
}
