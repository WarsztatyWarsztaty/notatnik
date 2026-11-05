using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace notatnik;

public class JsonNoteRepository : INoteRepository
{
    public const string DefaultFileName = "notes.json";

    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string FilePath => _filePath;

    public JsonNoteRepository(string? filePath = null)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath)
            ? Path.GetFullPath(DefaultFileName)
            : Path.GetFullPath(filePath);
    }

    public void EnsureStorageExists()
    {
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]", Encoding.UTF8);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new NoteStorageException($"Nie udało się utworzyć pliku bazy notatek '{_filePath}': {ex.Message}", ex);
        }
    }

    public List<Note> GetAllNotes()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                EnsureStorageExists();
                return new List<Note>();
            }

            string json = File.ReadAllText(_filePath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Note>();
            }

            var notes = JsonSerializer.Deserialize<List<Note>>(json, JsonOptions);
            return notes ?? new List<Note>();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            throw new NoteStorageException($"Wystąpił błąd podczas wczytywania notatek z pliku '{_filePath}': {ex.Message}", ex);
        }
    }

    public void SaveAll(IEnumerable<Note> notes)
    {
        if (notes == null)
            throw new ArgumentNullException(nameof(notes));

        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(notes, JsonOptions);
            File.WriteAllText(_filePath, json, Encoding.UTF8);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            throw new NoteStorageException($"Wystąpił błąd podczas zapisywania notatek do pliku '{_filePath}': {ex.Message}", ex);
        }
    }

    public Note SaveOrUpdate(Note note)
    {
        if (note == null)
            throw new ArgumentNullException(nameof(note));

        var notes = GetAllNotes();
        var existing = notes.Find(n => n.Id == note.Id);

        if (existing != null)
        {
            existing.Title = note.Title;
            existing.Content = note.Content;
            existing.UpdatedAt = DateTime.Now;
            note.UpdatedAt = existing.UpdatedAt;
        }
        else
        {
            if (note.Id == Guid.Empty)
            {
                note.Id = Guid.NewGuid();
            }

            var now = DateTime.Now;
            if (note.CreatedAt == default)
            {
                note.CreatedAt = now;
            }
            note.UpdatedAt = now;

            notes.Add(note);
        }

        SaveAll(notes);
        return note;
    }

    public bool DeleteNote(Guid id)
    {
        var notes = GetAllNotes();
        int removedCount = notes.RemoveAll(n => n.Id == id);
        if (removedCount > 0)
        {
            SaveAll(notes);
            return true;
        }

        return false;
    }
}

