<!--MainWindow.axaml-->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:notatnik"
        mc:Ignorable="d" d:DesignWidth="950" d:DesignHeight="620"
        Width="950" Height="620"
        MinWidth="720" MinHeight="480"
        x:Class="notatnik.MainWindow"
        Title="Notatnik"
        WindowStartupLocation="CenterScreen">

    <Grid RowDefinitions="*,Auto">
        <!-- Główny obszar roboczy podzielony na listę notatek i edytor -->
        <Grid Grid.Row="0" ColumnDefinitions="320,*">
            
            <!-- Lewa kolumna: Lista notatek -->
            <Border Grid.Column="0" 
                    BorderBrush="#33808080" 
                    BorderThickness="0,0,1,0" 
                    Padding="14">
                <Grid RowDefinitions="Auto,Auto,*,Auto">
                    <!-- Nagłówek -->
                    <TextBlock Grid.Row="0" 
                               Text="📝 Zapisane notatki" 
                               FontSize="18" 
                               FontWeight="Bold" 
                               Margin="0,0,0,12"/>

                    <!-- Przycisk Nowa notatka -->
                    <Button Grid.Row="1" 
                            x:Name="NewNoteButton" 
                            Click="OnNewNoteClicked"
                            HorizontalAlignment="Stretch" 
                            HorizontalContentAlignment="Center"
                            Padding="10,8"
                            Margin="0,0,0,12"
                            Content="➕ Nowa notatka"
                            FontWeight="SemiBold"/>

                    <!-- Lista zapisanych notatek -->
                    <ListBox Grid.Row="2" 
                             x:Name="NotesListBox" 
                             SelectionChanged="OnNoteSelectionChanged"
                             CornerRadius="6">
                        <ListBox.ItemTemplate>
                            <DataTemplate x:DataType="local:Note">
                                <StackPanel Spacing="3" Margin="4">
                                    <TextBlock Text="{Binding DisplayTitle}" 
                                               FontWeight="SemiBold" 
                                               FontSize="14" 
                                               TextTrimming="CharacterEllipsis"/>
                                    <TextBlock Text="{Binding UpdatedAtFormatted}" 
                                               FontSize="11" 
                                               Foreground="#888888"/>
                                    <TextBlock Text="{Binding Preview}" 
                                               FontSize="12" 
                                               Foreground="#777777" 
                                               TextTrimming="CharacterEllipsis"/>
                                </StackPanel>
                            </DataTemplate>
                        </ListBox.ItemTemplate>
                    </ListBox>

                    <!-- Przycisk Usuń notatkę -->
                    <Button Grid.Row="3" 
                            x:Name="DeleteNoteButton" 
                            Click="OnDeleteNoteClicked"
                            HorizontalAlignment="Stretch" 
                            HorizontalContentAlignment="Center"
                            Margin="0,12,0,0"
                            Padding="8"
                            Content="🗑️ Usuń wybraną notatkę"/>
                </Grid>
            </Border>

            <!-- Prawa kolumna: Pole na tytuł, treść i przycisk zapisu -->
            <Grid Grid.Column="1" RowDefinitions="Auto,Auto,Auto,*,Auto" Margin="18">
                <!-- Pole na tytuł -->
                <TextBlock Grid.Row="0" 
                           Text="Tytuł notatki:" 
                           FontWeight="SemiBold" 
                           FontSize="14"
                           Margin="0,0,0,6"/>
                
                <TextBox Grid.Row="1" 
                         x:Name="TitleTextBox" 
                         PlaceholderText="Wpisz tytuł notatki..." 
                         FontSize="15" 
                         Margin="0,0,0,14"/>

                <!-- Pole na treść -->
                <TextBlock Grid.Row="2" 
                           Text="Treść notatki:" 
                           FontWeight="SemiBold" 
                           FontSize="14"
                           Margin="0,0,0,6"/>
                
                <TextBox Grid.Row="3" 
                         x:Name="ContentTextBox" 
                         PlaceholderText="Wpisz treść notatki..." 
                         AcceptsReturn="True" 
                         TextWrapping="Wrap" 
                         VerticalAlignment="Stretch" 
                         ScrollViewer.VerticalScrollBarVisibility="Auto"
                         FontSize="14"
                         Margin="0,0,0,14"/>

                <!-- Przyciski akcji (Zapis, Eksport, Status) -->
                <StackPanel Grid.Row="4" Orientation="Horizontal" Spacing="10" VerticalAlignment="Center">
                    <Button x:Name="SaveButton" 
                            Click="OnSaveNoteClicked"
                            Content="💾 Zapisz notatkę" 
                            FontWeight="Bold"
                            Padding="16,9"/>

                    <Button x:Name="ExportButton" 
                            Click="OnExportTxtClicked"
                            Content="📄 Eksportuj jako .txt" 
                            Padding="12,9"/>

                    <TextBlock x:Name="StatusMessageTextBlock" 
                               VerticalAlignment="Center" 
                               Foreground="#2E7D32" 
                               FontWeight="Medium"
                               Margin="10,0,0,0"/>
                </StackPanel>
            </Grid>
        </Grid>

        <!-- Dolny pasek informacyjny o pliku zapisu -->
        <Border Grid.Row="1" 
                BorderThickness="0,1,0,0" 
                BorderBrush="#33808080" 
                Padding="14,8">
            <Grid ColumnDefinitions="*,Auto">
                <TextBlock Grid.Column="0" 
                           x:Name="FilePathTextBlock" 
                           VerticalAlignment="Center" 
                           FontSize="11" 
                           Foreground="#888888" 
                           TextTrimming="CharacterEllipsis"/>
                
                <Button Grid.Column="1" 
                        Click="OnOpenFolderClicked"
                        Content="📂 Otwórz folder z plikiem" 
                        FontSize="11" 
                        Padding="8,4"/>
            </Grid>
        </Border>
    </Grid>
</Window>
<!--MainWindow.axaml.cs-->
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
            foreach (var note in loaded)
            {
                _notes.Add(note);
            }

            // Zaznacz pierwszą notatkę domyślnie
            NotesListBox.SelectedIndex = 0;
            SetStatus($"Wczytano {_notes.Count} notatek z pliku.");
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
            SetStatus("Utworzono plik z notatką powitalną.");
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
            SetStatus($"Zapisano pomyślnie do pliku o {DateTime.Now:HH:mm:ss}!");
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
<!--.gitignore-->
[Bb]in/
[Oo]bj/
*.user
*.userosscache
*.sln.docstates
.vscode/
.idea/
notatki.json
eksport_txt/

