using System;
using System.Collections.Generic;
using System.IO;
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
    public void SaveAndLoadNotes_PreservesDataAndPolishCharacters()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"notatki_test_{Guid.NewGuid()}.json");

        try
        {
            var notes = new List<Note>
            {
                new Note
                {
                    Title = "Notatka z polskimi znakami: ąćęłńóśźż",
                    Content = "Treść: Zażółć gęślą jaźń!\nW drugiej linii coś jeszcze.",
                    UpdatedAt = new DateTime(2026, 10, 29, 10, 30, 0)
                },
                new Note
                {
                    Title = "Druga notatka",
                    Content = "Krótki opis zadania",
                    UpdatedAt = new DateTime(2026, 10, 29, 11, 0, 0)
                }
            };

            // Zapis do pliku
            NoteStorage.SaveNotes(tempFile, notes);
            Assert.True(File.Exists(tempFile));

            // Odczyt z pliku
            var loaded = NoteStorage.LoadNotes(tempFile);

            Assert.Equal(2, loaded.Count);
            Assert.Equal("Notatka z polskimi znakami: ąćęłńóśźż", loaded[0].Title);
            Assert.Contains("Zażółć gęślą jaźń!", loaded[0].Content);
            Assert.Equal("Druga notatka", loaded[1].Title);
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
        string nonExistentFile = Path.Combine(Path.GetTempPath(), $"non_existent_{Guid.NewGuid()}.json");
        var loaded = NoteStorage.LoadNotes(nonExistentFile);
        Assert.NotNull(loaded);
        Assert.Empty(loaded);
    }
}

