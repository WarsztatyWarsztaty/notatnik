using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace notatnik;

public class Note : INotifyPropertyChanged
{
    private string _id = Guid.NewGuid().ToString();
    private string _title = string.Empty;
    private string _content = string.Empty;
    private DateTime _updatedAt = DateTime.Now;
    private bool _hasTime = true;
    private string _separator = "—";

    public string Id
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
                OnPropertyChanged(nameof(Preview));
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
                OnPropertyChanged(nameof(UpdatedAtFormatted));
            }
        }
    }

    public bool HasTime
    {
        get => _hasTime;
        set
        {
            if (_hasTime != value)
            {
                _hasTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UpdatedAtFormatted));
            }
        }
    }

    public string Separator
    {
        get => _separator;
        set
        {
            if (_separator != value)
            {
                _separator = value;
                OnPropertyChanged();
            }
        }
    }

    [JsonIgnore]
    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "(Bez tytułu)" : Title;

    [JsonIgnore]
    public string UpdatedAtFormatted => HasTime
        ? UpdatedAt.ToString("yyyy-MM-dd HH:mm")
        : UpdatedAt.ToString("yyyy-MM-dd");

    [JsonIgnore]
    public string Preview
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Content)) return "(brak treści)";
            string[] lines = Content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string firstLine = lines.Length > 0 ? lines[0].Trim() : "";
            return firstLine.Length > 40 ? firstLine.Substring(0, 40) + "..." : firstLine;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
