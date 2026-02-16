using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccessNote;

public sealed class Contact : INotifyPropertyChanged
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _phone = string.Empty;
    private string _email = string.Empty;
    private string _address = string.Empty;
    private string _notes = string.Empty;
    private string _groupName = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public long Id { get; set; }

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (_firstName == value) return;
            _firstName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            if (_lastName == value) return;
            _lastName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string Phone
    {
        get => _phone;
        set { if (_phone != value) { _phone = value; OnPropertyChanged(); } }
    }

    public string Email
    {
        get => _email;
        set { if (_email != value) { _email = value; OnPropertyChanged(); } }
    }

    public string Address
    {
        get => _address;
        set { if (_address != value) { _address = value; OnPropertyChanged(); } }
    }

    public string Notes
    {
        get => _notes;
        set { if (_notes != value) { _notes = value; OnPropertyChanged(); } }
    }

    public string GroupName
    {
        get => _groupName;
        set { if (_groupName != value) { _groupName = value; OnPropertyChanged(); } }
    }

    public string DisplayName
    {
        get
        {
            var full = $"{_firstName} {_lastName}".Trim();
            return string.IsNullOrEmpty(full) ? "(No Name)" : full;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
