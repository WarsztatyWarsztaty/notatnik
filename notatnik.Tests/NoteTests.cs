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
