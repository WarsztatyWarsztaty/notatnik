using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using notatnik;
using Xunit;

namespace notatnik.Tests;

public class NoteTests
{
    [Fact]
    public void Note_Initialization_HasValidDefaults()
    {
        var note = new Note();

        Assert.NotEqual(Guid.Empty, note.Id);
        Assert.Equal(string.Empty, note.Title);
        Assert.Equal(string.Empty, note.Content);
        Assert.True(note.CreatedAt <= DateTime.Now);
        Assert.True(note.UpdatedAt <= DateTime.Now);
        Assert.Equal("(Bez tytułu)", note.DisplayTitle);
    }

    [Fact]
    public void Note_DisplayTitle_WhenTitleProvided_ReturnsTitle()
    {
        var note = new Note { Title = "Moja notatka" };
        Assert.Equal("Moja notatka", note.DisplayTitle);
    }

    [Fact]
    public void Note_FormattedDate_ContainsDates()
    {
        var note = new Note
        {
            CreatedAt = new DateTime(2026, 11, 5, 10, 0, 0),
            UpdatedAt = new DateTime(2026, 11, 5, 12, 30, 0)
        };

        Assert.Contains("2026-11-05 10:00", note.FormattedDate);
        Assert.Contains("2026-11-05 12:30", note.FormattedDate);
    }

    [Fact]
    public void Repository_EnsureStorageExists_CreatesEmptyJsonFileIfNotExist()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"notes_test_{Guid.NewGuid()}.json");

        try
        {
            var repo = new JsonNoteRepository(tempFile);
            Assert.False(File.Exists(tempFile));

            var notes = repo.GetAllNotes();

            Assert.True(File.Exists(tempFile));
            Assert.Empty(notes);

            string content = File.ReadAllText(tempFile).Trim();
            Assert.Equal("[]", content);
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
    public void Repository_SaveAllAndGetAll_PreservesDataAndPolishCharacters()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"notes_test_{Guid.NewGuid()}.json");

        try
        {
            var repo = new JsonNoteRepository(tempFile);

            var note1 = new Note
            {
                Id = Guid.NewGuid(),
                Title = "Zażółć gęślą jaźń",
                Content = "Wieloliniowa treść:\nLinia 1: ąćęłńóśźż\nLinia 2: ĄĆĘŁŃÓŚŹŻ",
                CreatedAt = new DateTime(2026, 11, 5, 8, 0, 0),
                UpdatedAt = new DateTime(2026, 11, 5, 9, 30, 0)
            };

            var note2 = new Note
            {
                Id = Guid.NewGuid(),
                Title = "Druga notatka",
                Content = "Krótki opis",
                CreatedAt = new DateTime(2026, 11, 5, 9, 0, 0),
                UpdatedAt = new DateTime(2026, 11, 5, 9, 15, 0)
            };

            repo.SaveAll(new[] { note1, note2 });

            // Sprawdź czy plik fizycznie istnieje i zawiera nieescapowane polskie znaki
            string rawJson = File.ReadAllText(tempFile, Encoding.UTF8);
            Assert.Contains("Zażółć gęślą jaźń", rawJson);
            Assert.Contains("ąćęłńóśźż", rawJson);
            Assert.DoesNotContain("\\u0", rawJson);

            // Odczyt z repozytorium
            var loaded = repo.GetAllNotes();
            Assert.Equal(2, loaded.Count);

            var loaded1 = loaded.First(n => n.Id == note1.Id);
            Assert.Equal("Zażółć gęślą jaźń", loaded1.Title);
            Assert.Equal(note1.Content, loaded1.Content);
            Assert.Equal(note1.CreatedAt, loaded1.CreatedAt);
            Assert.Equal(note1.UpdatedAt, loaded1.UpdatedAt);

            var loaded2 = loaded.First(n => n.Id == note2.Id);
            Assert.Equal("Druga notatka", loaded2.Title);
            Assert.Equal("Krótki opis", loaded2.Content);
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
    public void Repository_SaveOrUpdate_UpdatesExistingNoteWithoutDuplicate()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"notes_test_{Guid.NewGuid()}.json");

        try
        {
            var repo = new JsonNoteRepository(tempFile);

            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = "Tytuł początkowy",
                Content = "Treść początkowa",
                CreatedAt = new DateTime(2026, 11, 1, 10, 0, 0),
                UpdatedAt = new DateTime(2026, 11, 1, 10, 0, 0)
            };

            repo.SaveOrUpdate(note);

            var listAfterAdd = repo.GetAllNotes();
            Assert.Single(listAfterAdd);
            Assert.Equal("Tytuł początkowy", listAfterAdd[0].Title);

            // Modyfikacja tej samej notatki
            note.Title = "Zaktualizowany tytuł";
            note.Content = "Zaktualizowana treść";

            repo.SaveOrUpdate(note);

            var listAfterUpdate = repo.GetAllNotes();
            Assert.Single(listAfterUpdate); // Nie może być duplikatu!
            Assert.Equal("Zaktualizowany tytuł", listAfterUpdate[0].Title);
            Assert.Equal("Zaktualizowana treść", listAfterUpdate[0].Content);
            Assert.Equal(note.Id, listAfterUpdate[0].Id);
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
    public void Repository_DeleteNote_RemovesNoteSuccessfully()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"notes_test_{Guid.NewGuid()}.json");

        try
        {
            var repo = new JsonNoteRepository(tempFile);
            var note = new Note { Title = "Do usunięcia", Content = "Treść" };
            repo.SaveOrUpdate(note);

            Assert.Single(repo.GetAllNotes());

            bool deleted = repo.DeleteNote(note.Id);
            Assert.True(deleted);
            Assert.Empty(repo.GetAllNotes());
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
    public void Repository_CorruptedJson_ThrowsNoteStorageException()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"notes_test_{Guid.NewGuid()}.json");

        try
        {
            File.WriteAllText(tempFile, "NIEPOPRAWNY_JSON {{{", Encoding.UTF8);
            var repo = new JsonNoteRepository(tempFile);

            Assert.Throws<NoteStorageException>(() => repo.GetAllNotes());
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
    public void NoteStorage_InheritsFromJsonNoteRepository()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"notes_test_{Guid.NewGuid()}.json");
        try
        {
            var storage = new NoteStorage(tempFile);
            Assert.IsAssignableFrom<INoteRepository>(storage);
            Assert.IsAssignableFrom<JsonNoteRepository>(storage);

            var note = new Note { Title = "Test NoteStorage", Content = "Działa" };
            storage.SaveOrUpdate(note);

            var loaded = storage.GetAllNotes();
            Assert.Single(loaded);
            Assert.Equal("Test NoteStorage", loaded[0].Title);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
