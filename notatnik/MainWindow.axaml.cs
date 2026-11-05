using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace notatnik;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Note> _notes = new();
    private readonly string _filePath;
    private bool _isUpdatingFields;

    public MainWindow()
    {
        InitializeComponent();

        _filePath = NoteStorage.GetDefaultFilePath();
        FilePathTextBlock.Text = $"Plik zapisu: {_filePath}";

        NotesListBox.ItemsSource = _notes;

        LoadSavedNotes();
    }

    private void LoadSavedNotes()
    {
        _notes.Clear();
        var loaded = NoteStorage.LoadNotes(_filePath);

        if (loaded.Count > 0)
        {
            // Posortuj od najnowszej (najnowsza na górze)
            foreach (var note in loaded.OrderByDescending(n => n.UpdatedAt))
            {
                _notes.Add(note);
            }

            // Zaznacz pierwszą notatkę domyślnie
            NotesListBox.SelectedIndex = 0;
            SetStatus($"Wczytano {_notes.Count} notatek z pliku notatki.md.");
        }
        else
        {
            // Domyślna pierwsza notatka powitalna
            var sampleNote = new Note
            {
                Title = "Witaj w Notatniku",
                Content = "To jest Twoja pierwsza notatka!\n\nMożesz:\n- edytować tytuł i treść powyżej\n- kliknąć 'Zapisz notatkę'\n- utworzyć nową notatkę klikając '+ Nowa notatka'\n- usuwać wybrane notatki",
                UpdatedAt = DateTime.Now
            };
            _notes.Add(sampleNote);
            NoteStorage.SaveNotes(_filePath, _notes);
            NotesListBox.SelectedIndex = 0;
            SetStatus("Utworzono plik notatki.md z notatką powitalną.");
        }
    }

    private void OnNoteSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingFields) return;

        if (NotesListBox.SelectedItem is Note selectedNote)
        {
            _isUpdatingFields = true;
            TitleTextBox.Text = selectedNote.Title;
            ContentTextBox.Text = selectedNote.Content;
            _isUpdatingFields = false;

            SetStatus($"Wybrano notatkę: {selectedNote.DisplayTitle}");
        }
    }

    private void OnNewNoteClicked(object? sender, RoutedEventArgs e)
    {
        _isUpdatingFields = true;
        NotesListBox.SelectedItem = null;
        TitleTextBox.Text = string.Empty;
        ContentTextBox.Text = string.Empty;
        _isUpdatingFields = false;

        TitleTextBox.Focus();
        SetStatus("Wprowadź tytuł i treść nowej notatki, a następnie kliknij 'Zapisz notatkę'.");
    }

    private void OnSaveNoteClicked(object? sender, RoutedEventArgs e)
    {
        string title = TitleTextBox.Text?.Trim() ?? string.Empty;
        string content = ContentTextBox.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(content))
        {
            SetStatus("Wpisz tytuł lub treść przed zapisaniem!", isError: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Bez tytułu";
            TitleTextBox.Text = title;
        }

        if (NotesListBox.SelectedItem is Note currentNote)
        {
            // Edycja istniejącej notatki
            currentNote.Title = title;
            currentNote.Content = content;
            currentNote.UpdatedAt = DateTime.Now;

            // Przesuń zaktualizowaną notatkę na górę listy (najnowsza na górze)
            int oldIndex = _notes.IndexOf(currentNote);
            if (oldIndex > 0)
            {
                _isUpdatingFields = true;
                _notes.Move(oldIndex, 0);
                NotesListBox.SelectedItem = currentNote;
                _isUpdatingFields = false;
            }
        }
        else
        {
            // Tworzenie nowej notatki
            var newNote = new Note
            {
                Title = title,
                Content = content,
                UpdatedAt = DateTime.Now
            };

            _notes.Insert(0, newNote);
            NotesListBox.SelectedItem = newNote;
        }

        // Zapis do pliku
        try
        {
            NoteStorage.SaveNotes(_filePath, _notes);
            SetStatus($"Zapisano do pliku notatki.md o {DateTime.Now:HH:mm:ss}!");
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd zapisu: {ex.Message}", isError: true);
        }
    }

    private void OnDeleteNoteClicked(object? sender, RoutedEventArgs e)
    {
        if (NotesListBox.SelectedItem is Note selectedNote)
        {
            _notes.Remove(selectedNote);

            try
            {
                NoteStorage.SaveNotes(_filePath, _notes);
            }
            catch (Exception ex)
            {
                SetStatus($"Błąd zapisu po usunięciu: {ex.Message}", isError: true);
                return;
            }

            _isUpdatingFields = true;
            if (_notes.Count > 0)
            {
                NotesListBox.SelectedIndex = 0;
            }
            else
            {
                TitleTextBox.Text = string.Empty;
                ContentTextBox.Text = string.Empty;
            }
            _isUpdatingFields = false;

            SetStatus("Usunięto notatkę i zaktualizowano plik.");
        }
        else
        {
            SetStatus("Zaznacz notatkę na liście, aby ją usunąć.", isError: true);
        }
    }

    private void OnExportTxtClicked(object? sender, RoutedEventArgs e)
    {
        string title = TitleTextBox.Text?.Trim() ?? string.Empty;
        string content = ContentTextBox.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(content))
        {
            SetStatus("Brak treści do wyeksportowania.", isError: true);
            return;
        }

        try
        {
            string baseDir = Path.GetDirectoryName(_filePath) ?? Directory.GetCurrentDirectory();
            string exportDir = Path.Combine(baseDir, "eksport_txt");
            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            string safeTitle = string.Join("_", (string.IsNullOrWhiteSpace(title) ? "notatka" : title).Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safeTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string fullPath = Path.Combine(exportDir, fileName);

            File.WriteAllText(fullPath, $"{title}\n\n{content}");
            SetStatus($"Wyeksportowano do: {Path.Combine("eksport_txt", fileName)}");
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd eksportu: {ex.Message}", isError: true);
        }
    }

    private void OnOpenFolderClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            string? dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Nie udało się otworzyć folderu: {ex.Message}", isError: true);
        }
    }

    private void SetStatus(string message, bool isError = false)
    {
        StatusMessageTextBlock.Text = message;
        StatusMessageTextBlock.Foreground = isError ? Brushes.Crimson : Brushes.ForestGreen;
    }
}
