using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccessNote;

public sealed class NoteDocument : INotifyPropertyChanged
{
    private string _title;
    private string _content;
    private string _savedTitle;
    private string _savedContent;
    private DateTime _lastModifiedUtc;
    private bool _isDirty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public NoteDocument(string title, string content)
        : this(Guid.NewGuid().ToString("N"), title, content, DateTime.UtcNow)
    {
    }

    private NoteDocument(string id, string title, string content, DateTime lastModifiedUtc)
    {
        Id = id;
        _title = title;
        _content = content;
        _savedTitle = title;
        _savedContent = content;
        _lastModifiedUtc = lastModifiedUtc;
        _isDirty = false;
    }

    public string Id { get; }

    public string Title
    {
        get => _title;
        set
        {
            if (_title == value)
            {
                return;
            }

            _title = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayTitle));
            RecomputeDirty();
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            if (_content == value)
            {
                return;
            }

            _content = value;
            OnPropertyChanged();
            RecomputeDirty();
        }
    }

    public string DisplayTitle => _isDirty ? $"{_title} *" : _title;

    public DateTime LastModifiedUtc
    {
        get => _lastModifiedUtc;
        private set
        {
            if (_lastModifiedUtc == value)
            {
                return;
            }

            _lastModifiedUtc = value;
            OnPropertyChanged();
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (_isDirty == value)
            {
                return;
            }

            _isDirty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    public string PersistedTitle => _savedTitle;

    public string PersistedContent => _savedContent;

    public DateTime PersistedLastModifiedUtc => _lastModifiedUtc;

    public static NoteDocument FromPersisted(string id, string title, string content, DateTime lastModifiedUtc)
    {
        return new NoteDocument(id, title, content, lastModifiedUtc);
    }

    public void Save()
    {
        _savedTitle = _title;
        _savedContent = _content;
        LastModifiedUtc = DateTime.UtcNow;
        IsDirty = false;
    }

    public void RestorePersistedState(string title, string content, DateTime lastModifiedUtc)
    {
        _savedTitle = title;
        _savedContent = content;
        LastModifiedUtc = lastModifiedUtc;
        RecomputeDirty();
    }

    public void DiscardChanges()
    {
        _title = _savedTitle;
        _content = _savedContent;
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(DisplayTitle));
        IsDirty = false;
    }

    private void RecomputeDirty()
    {
        IsDirty = _title != _savedTitle || _content != _savedContent;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
