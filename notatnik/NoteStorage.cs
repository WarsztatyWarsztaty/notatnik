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
