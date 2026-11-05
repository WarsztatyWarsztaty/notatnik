using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace notatnik;

public class Note : INotifyPropertyChanged
{
    private Guid _id;
    private string _title = string.Empty;
    private string _content = string.Empty;
    private DateTime _createdAt;
    private DateTime _updatedAt;

    public Note()
    {
        _id = Guid.NewGuid();
        _createdAt = DateTime.Now;
        _updatedAt = DateTime.Now;
    }

    public Guid Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged();
            }
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set
        {
            if (_createdAt != value)
            {
                _createdAt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedDate));
            }
        }
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set
        {
            if (_updatedAt != value)
            {
                _updatedAt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedDate));
            }
        }
    }

    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "(Bez tytułu)" : Title;

    public string FormattedDate => $"Zmodyfikowano: {UpdatedAt:yyyy-MM-dd HH:mm} | Utworzono: {CreatedAt:yyyy-MM-dd HH:mm}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
