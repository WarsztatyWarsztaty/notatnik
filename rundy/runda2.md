notatki.md
## 2026-11-05 08:26 — ąćęół
.

## 2026-11-05 08:25 — runda 2 test
w jakim jest teraz formacie?

## 2026-10-29 09:28 — runda 1 test
gdzie sie to zapisze

## 2026-10-29 09:27 — Witaj w Notatniku
To jest Twoja pierwsza notatka!

Możesz:
- edytować tytuł i treść powyżej
- kliknąć 'Zapisz notatkę'
- utworzyć nową notatkę klikając '+ Nowa notatka'
- usuwać wybrane notatki

README.md
# Notatnik (Avalonia UI / .NET)

Aplikacja desktopowa **Notatnik** zrealizowana w oparciu o framework **Avalonia UI** oraz platformę **.NET 10**.

## ✨ Funkcjonalności

- **Pole na tytuł**: umożliwia podanie lub edycję tytułu notatki.
- **Pole na treść**: wielolinijkowe pole tekstowe z zawijaniem wierszy i paskiem przewijania.
- **Przycisk zapisu**: zapisuje i aktualizuje notatki na bieżąco, zapisując dane do trwałego pliku na dysku.
- **Lista zapisanych notatek**: interaktywna lista po lewej stronie z tytułem, datą modyfikacji oraz podglądem treści; kliknięcie wczytuje notatkę do edytora.
- **Zapis do pliku Markdown (`notatki.md`)**:
  - Wszystkie notatki trafiają do jednego pliku `notatki.md` w folderze projektu.
  - Format wpisu:
    ```markdown
    ## 2026-09-08 14:32 — Tytuł notatki
    treść notatki, może mieć kilka linii
    ```
  - Pusta linia oddziela poszczególne wpisy.
  - **Najnowsza notatka znajduje się na samej górze pliku**.
  - Pełne zachowanie polskich znaków diakrytycznych (kodowanie UTF-8 bez sekwencji ucieczki).
- **Dodatkowe udogodnienia**:
  - `➕ Nowa notatka` – czyści pola i przygotowuje edytor pod nową notatkę.
  - `🗑️ Usuń wybraną notatkę` – usuwa wybraną notatkę i aktualizuje plik.
  - `📄 Eksportuj jako .txt` – pozwala wyeksportować bieżącą notatkę do osobnego pliku tekstowego w katalogu `eksport_txt`.
  - `📂 Otwórz folder z plikiem` – otwiera Eksplorator Windows w miejscu zapisanego pliku `notatki.md`.

## 🚀 Uruchomienie aplikacji

Aby uruchomić aplikację, wykonaj w terminalu polecenie:

```bash
dotnet run --project notatnik
```

## 🧪 Uruchomienie testów jednostkowych

Projekt zawiera zestaw testów xUnit sprawdzających m.in. zapis, parsowanie, kolejność wpisów oraz polskie znaki diakrytyczne:

```bash
dotnet test
```

MainWindow.axaml.cs
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


NoteStorage.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace notatnik;

public static class NoteStorage
{
    public const string DefaultFileName = "notatki.md";

    private static readonly Regex NoteHeaderRegex = new(
        @"^##\s+(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2})\s*[—–-]\s*(.*)$",
        RegexOptions.Compiled);

    private static readonly Regex SecondaryHeaderRegex = new(
        @"^##\s+(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2})\s*(.*)$",
        RegexOptions.Compiled);

    private static readonly Regex FallbackHeaderRegex = new(
        @"^##\s*(.*)$",
        RegexOptions.Compiled);

    private static string? _cachedFilePath;

    public static string GetDefaultFilePath()
    {
        if (_cachedFilePath != null)
            return _cachedFilePath;

        string projectDir = FindProjectDirectory();
        _cachedFilePath = Path.Combine(projectDir, DefaultFileName);
        return _cachedFilePath;
    }

    public static string FindProjectDirectory()
    {
        // 1. Sprawdź, czy notatki.md już istnieje w bieżącym katalogu lub katalogach nadrzędnych
        string? existingFileDir = SearchUpwardsForFile(Directory.GetCurrentDirectory(), DefaultFileName);
        if (existingFileDir != null)
            return existingFileDir;

        existingFileDir = SearchUpwardsForFile(AppContext.BaseDirectory, DefaultFileName);
        if (existingFileDir != null)
            return existingFileDir;

        // Sprawdź też podkatalog "notatnik"
        string subfolderCandidate = Path.Combine(Directory.GetCurrentDirectory(), "notatnik", DefaultFileName);
        if (File.Exists(subfolderCandidate))
            return Path.Combine(Directory.GetCurrentDirectory(), "notatnik");

        // 2. Szukaj katalogu głównego projektu / repozytorium (z notatnik.slnx, .sln lub .git)
        string? projectRoot = SearchUpwardsForProjectRoot(Directory.GetCurrentDirectory());
        if (projectRoot != null)
            return projectRoot;

        projectRoot = SearchUpwardsForProjectRoot(AppContext.BaseDirectory);
        if (projectRoot != null)
            return projectRoot;

        // 3. Fallback do bieżącego katalogu
        return Directory.GetCurrentDirectory();
    }

    private static string? SearchUpwardsForFile(string startDir, string fileName)
    {
        if (string.IsNullOrWhiteSpace(startDir) || !Directory.Exists(startDir))
            return null;

        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, fileName)))
                return dir.FullName;

            dir = dir.Parent;
        }

        return null;
    }

    private static string? SearchUpwardsForProjectRoot(string startDir)
    {
        if (string.IsNullOrWhiteSpace(startDir) || !Directory.Exists(startDir))
            return null;

        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "notatnik.slnx")) ||
                File.Exists(Path.Combine(dir.FullName, "notatnik.sln")) ||
                Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    public static string SerializeMarkdown(IEnumerable<Note> notes)
    {
        // Najnowsza notatka na górze pliku
        var sortedNotes = notes.OrderByDescending(n => n.UpdatedAt).ToList();
        var sb = new StringBuilder();

        for (int i = 0; i < sortedNotes.Count; i++)
        {
            var note = sortedNotes[i];
            if (i > 0)
            {
                sb.AppendLine(); // Pusta linia między wpisami
            }

            string dateStr = note.UpdatedAt.ToString("yyyy-MM-dd HH:mm");
            string title = string.IsNullOrWhiteSpace(note.Title) ? "Bez tytułu" : note.Title.Trim();
            sb.AppendLine($"## {dateStr} — {title}");

            if (!string.IsNullOrEmpty(note.Content))
            {
                sb.AppendLine(note.Content.TrimEnd('\r', '\n'));
            }
        }

        return sb.ToString();
    }

    public static List<Note> ParseMarkdown(string markdown)
    {
        var notes = new List<Note>();
        if (string.IsNullOrWhiteSpace(markdown))
            return notes;

        using var reader = new StringReader(markdown);
        Note? currentNote = null;
        var contentBuilder = new StringBuilder();

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var match = NoteHeaderRegex.Match(line);
            if (match.Success)
            {
                FinalizeCurrentNote(currentNote, contentBuilder, notes);
                currentNote = CreateNoteFromMatch(match.Groups[1].Value, match.Groups[2].Value);
                continue;
            }

            var secondaryMatch = SecondaryHeaderRegex.Match(line);
            if (secondaryMatch.Success)
            {
                FinalizeCurrentNote(currentNote, contentBuilder, notes);
                currentNote = CreateNoteFromMatch(secondaryMatch.Groups[1].Value, secondaryMatch.Groups[2].Value);
                continue;
            }

            var fallbackMatch = FallbackHeaderRegex.Match(line);
            if (fallbackMatch.Success && line.TrimStart().StartsWith("## "))
            {
                FinalizeCurrentNote(currentNote, contentBuilder, notes);
                currentNote = new Note
                {
                    Title = string.IsNullOrWhiteSpace(fallbackMatch.Groups[1].Value) ? "Bez tytułu" : fallbackMatch.Groups[1].Value.Trim(),
                    UpdatedAt = DateTime.Now
                };
                continue;
            }

            if (currentNote != null)
            {
                if (contentBuilder.Length > 0)
                {
                    contentBuilder.AppendLine();
                }
                contentBuilder.Append(line);
            }
        }

        FinalizeCurrentNote(currentNote, contentBuilder, notes);

        return notes;
    }

    private static void FinalizeCurrentNote(Note? note, StringBuilder contentBuilder, List<Note> targetList)
    {
        if (note == null) return;

        note.Content = contentBuilder.ToString().TrimEnd('\r', '\n');
        targetList.Add(note);
        contentBuilder.Clear();
    }

    private static Note CreateNoteFromMatch(string dateStr, string title)
    {
        DateTime updatedAt = DateTime.Now;
        if (DateTime.TryParseExact(dateStr.Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            updatedAt = parsedDate;
        }
        else if (DateTime.TryParse(dateStr.Trim(), CultureInfo.InvariantCulture, out var fallbackDate))
        {
            updatedAt = fallbackDate;
        }

        return new Note
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Bez tytułu" : title.Trim(),
            UpdatedAt = updatedAt
        };
    }

    public static List<Note> LoadNotes(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                var legacyNotes = TryLoadLegacyJson(filePath);
                if (legacyNotes != null && legacyNotes.Count > 0)
                {
                    SaveNotes(filePath, legacyNotes);
                    return legacyNotes;
                }

                return new List<Note>();
            }

            string markdown = File.ReadAllText(filePath, Encoding.UTF8);
            return ParseMarkdown(markdown);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas odczytu notatek z {filePath}: {ex.Message}");
            return new List<Note>();
        }
    }

    public static void SaveNotes(string filePath, IEnumerable<Note> notes)
    {
        string directory = Path.GetDirectoryName(filePath) ?? "";
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string markdown = SerializeMarkdown(notes);
        // Zapis w UTF-8 bez BOM z zachowaniem polskich znaków
        File.WriteAllText(filePath, markdown, new UTF8Encoding(false));
    }

    private static List<Note>? TryLoadLegacyJson(string filePath)
    {
        try
        {
            string? dir = Path.GetDirectoryName(filePath);
            var candidates = new List<string>();
            if (!string.IsNullOrEmpty(dir))
            {
                candidates.Add(Path.Combine(dir, "notatki.json"));
                candidates.Add(Path.Combine(dir, "notatnik", "notatki.json"));
            }
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "notatki.json"));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "notatnik", "notatki.json"));

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    string json = File.ReadAllText(candidate, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var list = JsonSerializer.Deserialize<List<Note>>(json, options);
                        if (list != null && list.Count > 0)
                        {
                            return list;
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignoruj błędy przy próbie odczytu legacy json
        }

        return null;
    }
}

NoteTests.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using notatnik;
using Xunit;

namespace notatnik.Tests;

public class NoteTests
{
    [Fact]
    public void DisplayTitle_WhenTitleIsEmpty_ReturnsFallback()
    {
        var note = new Note { Title = "" };
        Assert.Equal("(Bez tytułu)", note.DisplayTitle);

        note.Title = "   ";
        Assert.Equal("(Bez tytułu)", note.DisplayTitle);
    }

    [Fact]
    public void DisplayTitle_WhenTitleProvided_ReturnsTitle()
    {
        var note = new Note { Title = "Lista zakupów" };
        Assert.Equal("Lista zakupów", note.DisplayTitle);
    }

    [Fact]
    public void Preview_TruncatesLongContent()
    {
        var note = new Note
        {
            Content = "To jest bardzo długa pierwsza linia notatki, która powinna zostać skrócona wielokropkiem po określonej liczbie znaków"
        };

        Assert.EndsWith("...", note.Preview);
    }

    [Fact]
    public void SerializeMarkdown_FormatsAccordingToSpec_WithNewestOnTop()
    {
        var olderNote = new Note
        {
            Title = "Pierwsza notatka",
            Content = "Treść pierwszej notatki\nw dwóch wierszach",
            UpdatedAt = new DateTime(2026, 9, 8, 14, 32, 0)
        };

        var newerNote = new Note
        {
            Title = "Druga notatka",
            Content = "Treść drugiej nowszej notatki",
            UpdatedAt = new DateTime(2026, 9, 8, 16, 45, 0)
        };

        // Przekazujemy w kolejności chronologicznej
        var markdown = NoteStorage.SerializeMarkdown(new[] { olderNote, newerNote });

        // Sprawdź format:
        // ## 2026-09-08 14:32 — Tytuł notatki
        // Najnowsza notatka na górze pliku
        var lines = markdown.Replace("\r\n", "\n").Split('\n');

        Assert.Equal("## 2026-09-08 16:45 — Druga notatka", lines[0]);
        Assert.Equal("Treść drugiej nowszej notatki", lines[1]);
        Assert.Equal("", lines[2]); // Pusta linia między wpisami
        Assert.Equal("## 2026-09-08 14:32 — Pierwsza notatka", lines[3]);
        Assert.Equal("Treść pierwszej notatki", lines[4]);
        Assert.Equal("w dwóch wierszach", lines[5]);
    }

    [Fact]
    public void ParseMarkdown_ParsesSpecFormatAndMultilineContent()
    {
        string inputMarkdown =
@"## 2026-09-08 16:45 — Druga notatka
Treść drugiej nowszej notatki

## 2026-09-08 14:32 — Tytuł notatki
treść notatki, może mieć kilka linii
druga linia treści
trzecia linia treści
";

        var notes = NoteStorage.ParseMarkdown(inputMarkdown);

        Assert.Equal(2, notes.Count);

        Assert.Equal("Druga notatka", notes[0].Title);
        Assert.Equal(new DateTime(2026, 9, 8, 16, 45, 0), notes[0].UpdatedAt);
        Assert.Equal("Treść drugiej nowszej notatki", notes[0].Content);

        Assert.Equal("Tytuł notatki", notes[1].Title);
        Assert.Equal(new DateTime(2026, 9, 8, 14, 32, 0), notes[1].UpdatedAt);
        Assert.Equal("treść notatki, może mieć kilka linii\ndruga linia treści\ntrzecia linia treści", notes[1].Content.Replace("\r\n", "\n"));
    }

    [Fact]
    public void SaveAndLoadNotes_PreservesPolishCharactersAndFormatting()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"notatki_test_{Guid.NewGuid()}.md");

        try
        {
            var notes = new List<Note>
            {
                new Note
                {
                    Title = "Notatka z polskimi znakami: ąćęłńóśźż ĄĆĘŁŃÓŚŹŻ",
                    Content = "Zażółć gęślą jaźń!\nZażółconą gęślą jaźń w drugiej linii.",
                    UpdatedAt = new DateTime(2026, 11, 5, 8, 30, 0)
                },
                new Note
                {
                    Title = "Zwykły tytuł",
                    Content = "Prosta treść bez udziwnień",
                    UpdatedAt = new DateTime(2026, 11, 5, 8, 0, 0)
                }
            };

            // Zapis do pliku
            NoteStorage.SaveNotes(tempFile, notes);
            Assert.True(File.Exists(tempFile));

            // Upewnij się, że w pliku są surowe polskie znaki (bez encji typu \u0105 czy &oacute;)
            string rawFileText = File.ReadAllText(tempFile, Encoding.UTF8);
            Assert.Contains("ąćęłńóśźż ĄĆĘŁŃÓŚŹŻ", rawFileText);
            Assert.Contains("Zażółć gęślą jaźń!", rawFileText);
            Assert.DoesNotContain("\\u0", rawFileText);

            // Odczyt z pliku
            var loaded = NoteStorage.LoadNotes(tempFile);

            Assert.Equal(2, loaded.Count);
            Assert.Equal("Notatka z polskimi znakami: ąćęłńóśźż ĄĆĘŁŃÓŚŹŻ", loaded[0].Title);
            Assert.Contains("Zażółć gęślą jaźń!", loaded[0].Content);
            Assert.Equal("Zwykły tytuł", loaded[1].Title);
            Assert.Equal("Prosta treść bez udziwnień", loaded[1].Content);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void LoadNotes_WhenFileDoesNotExist_ReturnsEmptyList()
    {
        string nonExistentFile = Path.Combine(Path.GetTempPath(), $"non_existent_{Guid.NewGuid()}.md");
        var loaded = NoteStorage.LoadNotes(nonExistentFile);
        Assert.NotNull(loaded);
        Assert.Empty(loaded);
    }
}

