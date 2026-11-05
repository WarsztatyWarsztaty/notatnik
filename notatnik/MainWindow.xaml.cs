using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace notatnik;

public partial class MainWindow : Window
{
    private readonly INoteRepository _repository;
    private readonly ObservableCollection<Note> _notes = new();
    private Guid? _selectedNoteId = null;
    private bool _isPopulatingForm = false;

    public MainWindow() : this(new JsonNoteRepository())
    {
    }

    public MainWindow(INoteRepository repository)
    {
        InitializeComponent();

        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        NotesListBox.ItemsSource = _notes;

        StoragePathTextBlock.Text = $"Plik: {_repository.FilePath}";

        Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        LoadNotes();
    }

    private void LoadNotes()
    {
        try
        {
            var loaded = _repository.GetAllNotes();

            _notes.Clear();
            foreach (var note in loaded.OrderByDescending(n => n.UpdatedAt))
            {
                _notes.Add(note);
            }

            SetStatus($"Wczytano {_notes.Count} notatek z pliku.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Wystąpił błąd wejścia/wyjścia (I/O) podczas wczytywania notatek:\n{ex.Message}",
                "Błąd I/O",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            SetStatus("Błąd wczytywania pliku.");
        }
    }

    private void OnNoteSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isPopulatingForm) return;

        if (NotesListBox.SelectedItem is Note selectedNote)
        {
            _selectedNoteId = selectedNote.Id;

            _isPopulatingForm = true;
            TitleTextBox.Text = selectedNote.Title;
            ContentTextBox.Text = selectedNote.Content;
            _isPopulatingForm = false;

            HideValidation();
            CurrentModeTextBlock.Text = $"Tryb: Edycja ({selectedNote.DisplayTitle})";
            SetStatus($"Wczytano notatkę: {selectedNote.DisplayTitle}");
        }
    }

    private void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        string rawTitle = TitleTextBox.Text;
        string title = rawTitle?.Trim() ?? string.Empty;
        string content = ContentTextBox.Text ?? string.Empty;

        // Walidacja: tytuł jest wymagany
        if (string.IsNullOrWhiteSpace(title))
        {
            ShowValidation("Tytuł jest wymagany.");
            MessageBox.Show(
                "Tytuł jest wymagany.",
                "Walidacja formularza",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            TitleTextBox.Focus();
            return;
        }

        HideValidation();

        try
        {
            if (_selectedNoteId.HasValue)
            {
                // Aktualizacja istniejącej notatki bez tworzenia duplikatu
                var existing = _notes.FirstOrDefault(n => n.Id == _selectedNoteId.Value);
                if (existing != null)
                {
                    existing.Title = title;
                    existing.Content = content;
                    existing.UpdatedAt = DateTime.Now;
                    string path = _repository.FilePath;
string content = TitleTextBox.Text + Environment.NewLine + ContentTextBox.Text;

                   try
{
    File.WriteAllText(path, content);
    StatusText.Text = "Zapisano";
}
catch (Exception ex)
{
    StatusText.Text = $"Nie udało się zapisać: {ex.Message}";
}
                    SetStatus($"Zaktualizowano notatkę: '{existing.Title}'.");
                }
                else
                {
                    var newNote = new Note
                    {
                        Id = _selectedNoteId.Value,
                        Title = title,
                        Content = content,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _notes.Add(newNote);
                   try
{
    File.WriteAllText(path, content);
    StatusText.Text = "Zapisano";
}
catch (Exception ex)
{
    StatusText.Text = $"Nie udało się zapisać: {ex.Message}";
}
                    SetStatus($"Zapisano notatkę: '{newNote.Title}'.");
                }
            }
            else
            {
                // Nowa notatka
                var newNote = new Note
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _notes.Add(newNote);
                try
{
    File.WriteAllText(path, content);
    StatusText.Text = "Zapisano";
}
catch (Exception ex)
{
    StatusText.Text = $"Nie udało się zapisać: {ex.Message}";
}
                SetStatus($"Utworzono nową notatkę: '{newNote.Title}'.");
            }

            // Po poprawnym zapisie: pola formularza zostają wyczyszczone, a lista notatek odświeżona
            ClearForm();
            LoadNotes();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Wystąpił błąd wejścia/wyjścia (I/O) podczas zapisywania notatki:\n{ex.Message}",
                "Błąd zapisu (I/O)",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            SetStatus("Błąd zapisu notatki.");
        }
    }

    private void OnNewNoteClicked(object sender, RoutedEventArgs e)
    {
        ClearForm();
        TitleTextBox.Focus();
        SetStatus("Rozpoczęto tworzenie nowej notatki.");
    }

    private void OnClearClicked(object sender, RoutedEventArgs e)
    {
        ClearForm();
        SetStatus("Wyczyszczono formularz.");
    }

    private void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        if (NotesListBox.SelectedItem is not Note selectedNote)
        {
            MessageBox.Show(
                "Wybierz notatkę z listy, którą chcesz usunąć.",
                "Brak zaznaczenia",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Czy na pewno chcesz usunąć notatkę '{selectedNote.DisplayTitle}'?",
            "Potwierdzenie usunięcia",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _repository.DeleteNote(selectedNote.Id);
                if (_selectedNoteId == selectedNote.Id)
                {
                    ClearForm();
                }
                LoadNotes();
                SetStatus($"Usunięto notatkę '{selectedNote.DisplayTitle}'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Wystąpił błąd podczas usuwania notatki:\n{ex.Message}",
                    "Błąd I/O",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void OnTitleTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            HideValidation();
        }
    }

    private void ClearForm()
    {
        _isPopulatingForm = true;
        TitleTextBox.Text = string.Empty;
        ContentTextBox.Text = string.Empty;
        _selectedNoteId = null;
        NotesListBox.SelectedItem = null;
        _isPopulatingForm = false;

        HideValidation();
        CurrentModeTextBlock.Text = "Tryb: Nowa notatka";
    }

    private void ShowValidation(string message)
    {
        ValidationTextBlock.Text = message;
        ValidationTextBlock.Visibility = Visibility.Visible;
    }

    private void HideValidation()
    {
        ValidationTextBlock.Visibility = Visibility.Collapsed;
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
    }
}

