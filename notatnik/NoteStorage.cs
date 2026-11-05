using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace notatnik;

public static class NoteStorage
{
    private static string? _cachedFilePath;

    public static string GetDefaultFilePath()
    {
        if (_cachedFilePath != null)
            return _cachedFilePath;

        // Try using the current directory (project directory when run via CLI or IDE)
        string cwdPath = Path.Combine(Directory.GetCurrentDirectory(), "notatki.json");
        try
        {
            // Verify if directory is writable
            string testDir = Directory.GetCurrentDirectory();
            if (Directory.Exists(testDir))
            {
                _cachedFilePath = cwdPath;
                return _cachedFilePath;
            }
        }
        catch
        {
            // fallback
        }

        _cachedFilePath = Path.Combine(AppContext.BaseDirectory, "notatki.json");
        return _cachedFilePath;
    }

    public static List<Note> LoadNotes(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new List<Note>();

            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<Note>();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<List<Note>>(json, options) ?? new List<Note>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas odczytu notatek z {filePath}: {ex.Message}");
            return new List<Note>();
        }
    }

    public static void SaveNotes(string filePath, IEnumerable<Note> notes)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Preserves Polish diacritics in JSON
        };

        string directory = Path.GetDirectoryName(filePath) ?? "";
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(notes, options);
        File.WriteAllText(filePath, json);
    }
}

