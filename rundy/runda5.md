READMe.md
# Notatnik (WPF / .NET 8)

Aplikacja desktopowa **Notatnik** zrealizowana w technologii **C# (.NET 8, WPF)** z interfejsem graficznym i trwałym zapisem danych w formacie JSON (`notes.json`).

## ✨ Wymagania i funkcjonalności

1. **Interfejs użytkownika:**
   - **Pole tekstowe (TextBox) na tytuł notatki** – umożliwia podanie lub edycję tytułu wpisu.
   - **Wieloliniowe pole tekstowe (TextBox) na treść notatki** – pole z obsługą klawiszy Enter/Tab, zawijaniem wierszy (`TextWrapping="Wrap"`) i automatycznym paskiem przewijania.
   - **Przycisk "Zapisz"** – zapisuje nową notatkę lub aktualizuje istniejącą.
   - **Lista zapisanych notatek (ListBox)** – prezentuje tytuł oraz datę utworzenia i modyfikacji każdej notatki.
   - **Dodatkowe akcje:** przycisk „➕ Nowa notatka”, „Wyczyść” oraz „🗑️ Usuń zaznaczoną”.

2. **Zachowanie po akcjach:**
   - **Wczytywanie:** kliknięcie elementu na liście wczytuje tytuł i treść notatki do pól edycji oraz przełącza formularz w tryb edycji.
   - **Walidacja:** kliknięcie "Zapisz" z pustym tytułem wyświetla walidację (czerwony komunikat inline oraz ostrzeżenie) informujące, że tytuł jest wymagany.
   - **Czyszczenie i odświeżenie:** po poprawnym zapisie pola formularza zostają wyczyszczone, a lista notatek jest automatycznie odświeżona.
   - **Aktualizacja bez duplikatów:** jeśli notatka została wybrana z listy i zmodyfikowana, zapis aktualizuje istniejący wpis (podmieniając treść, tytuł oraz odświeżając pole `UpdatedAt`) zamiast tworzyć duplikat.

3. **Model danych i przechowywanie:**
   - Model `Note`:
     - `Id` (`Guid`)
     - `Title` (`string`)
     - `Content` (`string`)
     - `CreatedAt` (`DateTime`)
     - `UpdatedAt` (`DateTime`)
   - Notatki są serializowane do pojedynczego pliku `notes.json` w katalogu roboczym aplikacji (`System.Text.Json` z formatowaniem `WriteIndented` oraz zachowaniem polskich znaków).
   - Przy starcie aplikacja automatycznie wczytuje istniejące notatki z pliku (a jeśli plik nie istnieje, tworzy pusty plik `notes.json`).

4. **Architektura kodu i obsługa błędów:**
   - Logika operacji na plikach została oddzielona od widoku za pomocą interfejsu `INoteRepository` oraz klasy `JsonNoteRepository` (z aliasem/klasą serwisową `NoteStorage`).
   - Obsługa błędów wejścia/wyjścia (I/O) i błędów parsowania JSON za pomocą dedykowanego wyjątku `NoteStorageException` oraz komunikatów w widoku.

## 🚀 Uruchomienie aplikacji

Aby uruchomić aplikację:

```bash
dotnet run --project notatnik
```

## 🧪 Testy jednostkowe

Projekt zawiera testy jednostkowe xUnit w projekcie `notatnik.Tests`:

```bash
dotnet test
```
MainWindow.xaml

<Window x:Class="notatnik.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:notatnik"
        mc:Ignorable="d"
        Title="Notatnik" Height="650" Width="950"
        MinWidth="750" MinHeight="480"
        WindowStartupLocation="CenterScreen"
        FontFamily="Segoe UI" FontSize="13">

    <Grid Background="#F9FAFB">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Nagłówek aplikacji -->
        <Border Grid.Row="0" Background="#FFFFFF" BorderBrush="#E5E7EB" BorderThickness="0,0,0,1" Padding="20,12">
            <DockPanel LastChildFill="True">
                <TextBlock Text="📝 Prosty Notatnik" FontSize="18" FontWeight="Bold" Foreground="#1F2937" VerticalAlignment="Center"/>
                <TextBlock x:Name="CurrentModeTextBlock" 
                           Text="Tryb: Nowa notatka" 
                           FontSize="12" 
                           Foreground="#6B7280" 
                           HorizontalAlignment="Right" 
                           VerticalAlignment="Center"/>
            </DockPanel>
        </Border>

        <!-- Główny podział: Lewa kolumna (Lista), Prawa kolumna (Edytor) -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="340" MinWidth="260"/>
                <ColumnDefinition Width="1"/>
                <ColumnDefinition Width="*" MinWidth="350"/>
            </Grid.ColumnDefinitions>

            <!-- LEWA KOLUMNA: Lista zapisanych notatek -->
            <Border Grid.Column="0" Background="#F3F4F6" Padding="14">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="*"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>

                    <!-- Pasek nad listą: Tytuł i przycisk Nowa notatka -->
                    <DockPanel Grid.Row="0" Margin="0,0,0,10">
                        <TextBlock Text="Zapisane notatki" 
                                   FontWeight="SemiBold" 
                                   FontSize="15" 
                                   Foreground="#374151" 
                                   VerticalAlignment="Center"/>
                        <Button x:Name="NewNoteButton" 
                                Content="➕ Nowa notatka" 
                                HorizontalAlignment="Right" 
                                Click="OnNewNoteClicked"
                                Background="#FFFFFF" 
                                BorderBrush="#D1D5DB"
                                Padding="8,4"
                                FontWeight="Medium"/>
                    </DockPanel>

                    <!-- Lista zapisanych notatek (ListBox) -->
                    <ListBox Grid.Row="1" 
                             x:Name="NotesListBox" 
                             SelectionChanged="OnNoteSelectionChanged"
                             Background="#FFFFFF"
                             BorderBrush="#E5E7EB"
                             BorderThickness="1"
                             ScrollViewer.HorizontalScrollBarVisibility="Disabled">
                        <ListBox.ItemTemplate>
                            <DataTemplate DataType="{x:Type local:Note}">
                                <Border Padding="8,8" 
                                        Margin="2,2" 
                                        BorderThickness="0,0,0,1" 
                                        BorderBrush="#F3F4F6">
                                    <StackPanel>
                                        <!-- Tytuł notatki -->
                                        <TextBlock Text="{Binding DisplayTitle}" 
                                                   FontWeight="SemiBold" 
                                                   FontSize="14" 
                                                   Foreground="#111827"
                                                   TextTrimming="CharacterEllipsis"/>
                                        
                                        <!-- Daty: Modyfikacji i Utworzenia -->
                                        <TextBlock FontSize="11" Foreground="#4B5563" Margin="0,4,0,0">
                                            <Run Text="Zmodyfikowano: " FontWeight="Normal"/>
                                            <Run Text="{Binding UpdatedAt, StringFormat='{}{0:yyyy-MM-dd HH:mm}'}" FontWeight="Medium"/>
                                        </TextBlock>
                                        <TextBlock FontSize="10" Foreground="#9CA3AF" Margin="0,2,0,0">
                                            <Run Text="Utworzono: "/>
                                            <Run Text="{Binding CreatedAt, StringFormat='{}{0:yyyy-MM-dd HH:mm}'}"/>
                                        </TextBlock>
                                    </StackPanel>
                                </Border>
                            </DataTemplate>
                        </ListBox.ItemTemplate>
                    </ListBox>

                    <!-- Przycisk usuwania wybranej notatki -->
                    <Button Grid.Row="2" 
                            x:Name="DeleteButton"
                            Content="🗑️ Usuń zaznaczoną" 
                            Margin="0,10,0,0" 
                            HorizontalAlignment="Stretch"
                            Background="#FEE2E2" 
                            Foreground="#991B1B" 
                            BorderBrush="#FCA5A5"
                            Click="OnDeleteClicked"/>
                </Grid>
            </Border>

            <!-- Separator pionowy -->
            <GridSplitter Grid.Column="1" HorizontalAlignment="Stretch" Background="#E5E7EB"/>

            <!-- PRAWA KOLUMNA: Formularz edycji notatki -->
            <Border Grid.Column="2" Background="#FFFFFF" Padding="24">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/> <!-- Etykieta tytułu -->
                        <RowDefinition Height="Auto"/> <!-- Pole tekstowe na tytuł -->
                        <RowDefinition Height="Auto"/> <!-- Komunikat walidacji -->
                        <RowDefinition Height="Auto"/> <!-- Etykieta treści -->
                        <RowDefinition Height="*"/>    <!-- Wieloliniowe pole na treść -->
                        <RowDefinition Height="Auto"/> <!-- Pasek z przyciskiem Zapisz -->
                    </Grid.RowDefinitions>

                    <!-- Etykieta tytułu -->
                    <TextBlock Grid.Row="0" 
                               Text="Tytuł notatki:" 
                               FontWeight="SemiBold" 
                               Foreground="#374151" 
                               Margin="0,0,0,6"/>

                    <!-- Pole tekstowe na tytuł notatki -->
                    <TextBox Grid.Row="1" 
                             x:Name="TitleTextBox" 
                             FontSize="14" 
                             Padding="8,6" 
                             BorderBrush="#D1D5DB"
                             TextChanged="OnTitleTextChanged"/>

                    <!-- Komunikat walidacji tytułu -->
                    <TextBlock Grid.Row="2" 
                               x:Name="ValidationTextBlock" 
                               Text="Tytuł jest wymagany." 
                               Foreground="#DC2626" 
                               FontWeight="Medium" 
                               FontSize="12" 
                               Margin="0,4,0,10" 
                               Visibility="Collapsed"/>

                    <!-- Etykieta treści -->
                    <TextBlock Grid.Row="3" 
                               Text="Treść notatki:" 
                               FontWeight="SemiBold" 
                               Foreground="#374151" 
                               Margin="0,10,0,6"/>

                    <!-- Wieloliniowe pole tekstowe na treść notatki -->
                    <TextBox Grid.Row="4" 
                             x:Name="ContentTextBox" 
                             AcceptsReturn="True" 
                             AcceptsTab="True"
                             TextWrapping="Wrap" 
                             VerticalScrollBarVisibility="Auto" 
                             FontSize="13" 
                             Padding="8" 
                             BorderBrush="#D1D5DB" 
                             FontFamily="Consolas, Segoe UI, sans-serif"/>

                    <!-- Pasek akcji: Przycisk Zapisz -->
                    <StackPanel Grid.Row="5" 
                                Orientation="Horizontal" 
                                HorizontalAlignment="Right" 
                                Margin="0,16,0,0">
                        <Button x:Name="ClearButton"
                                Content="Wyczyść" 
                                Margin="0,0,10,0" 
                                Padding="14,8"
                                Background="#F3F4F6" 
                                BorderBrush="#D1D5DB"
                                Click="OnClearClicked"/>
                        
                        <Button x:Name="SaveButton" 
                                Content="💾 Zapisz" 
                                Padding="20,8" 
                                Background="#2563EB" 
                                Foreground="#FFFFFF" 
                                FontWeight="Bold" 
                                BorderThickness="0"
                                Click="OnSaveClicked"/>
                    </StackPanel>
                </Grid>
            </Border>
        </Grid>

        <!-- Pasek statusu na dole -->
        <Border Grid.Row="2" Background="#F3F4F6" BorderBrush="#E5E7EB" BorderThickness="0,1,0,0" Padding="12,6">
            <DockPanel LastChildFill="True">
                <TextBlock x:Name="StoragePathTextBlock" 
                           FontSize="11" 
                           Foreground="#6B7280" 
                           DockPanel.Dock="Right" 
                           Margin="10,0,0,0"/>
                <TextBlock x:Name="StatusTextBlock" 
                           Text="Gotowy" 
                           FontSize="11" 
                           Foreground="#374151"/>
            </DockPanel>
        </Border>
    </Grid>
</Window>

MainWindow.xaml.cs

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

                    _repository.SaveAll(_notes);
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
                    _repository.SaveAll(_notes);
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
                _repository.SaveAll(_notes);
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

notatnik.csproj

<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

</Project>

Note.cs

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace notatnik;

public class Note : INotifyPropertyChanged
{
    private Guid _id;
    private string _title = string.Empty;
    private string _content = string.Empty;
    private DateTime _createdAt;
    private DateTime _updatedAt;

    public Note()
    {
        _id = Guid.NewGuid();
        _createdAt = DateTime.Now;
        _updatedAt = DateTime.Now;
    }

    public Guid Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged();
            }
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set
        {
            if (_createdAt != value)
            {
                _createdAt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedDate));
            }
        }
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set
        {
            if (_updatedAt != value)
            {
                _updatedAt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedDate));
            }
        }
    }

    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "(Bez tytułu)" : Title;

    public string FormattedDate => $"Zmodyfikowano: {UpdatedAt:yyyy-MM-dd HH:mm} | Utworzono: {CreatedAt:yyyy-MM-dd HH:mm}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

notes.json

[]

NoteStorage.cs

namespace notatnik;

public class NoteStorage : JsonNoteRepository
{
    public NoteStorage(string? filePath = null) : base(filePath)
    {
    }
}

NoteStorageException.cs

using System;

namespace notatnik;

public class NoteStorageException : Exception
{
    public NoteStorageException(string message) : base(message)
    {
    }

    public NoteStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

notatnik.csproj.nuget.dgspec.json

{
  "format": 1,
  "restore": {
    "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\notatnik.csproj": {}
  },
  "projects": {
    "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\notatnik.csproj": {
      "version": "1.0.0",
      "restore": {
        "projectUniqueName": "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\notatnik.csproj",
        "projectName": "notatnik",
        "projectPath": "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\notatnik.csproj",
        "packagesPath": "C:\\Users\\Martyna\\.nuget\\packages\\",
        "outputPath": "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\obj\\",
        "projectStyle": "PackageReference",
        "fallbackFolders": [
          "C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\NuGetPackages"
        ],
        "configFilePaths": [
          "C:\\Users\\Martyna\\AppData\\Roaming\\NuGet\\NuGet.Config",
          "C:\\Program Files (x86)\\NuGet\\Config\\Microsoft.VisualStudio.FallbackLocation.config",
          "C:\\Program Files (x86)\\NuGet\\Config\\Microsoft.VisualStudio.Offline.config"
        ],
        "originalTargetFrameworks": [
          "net8.0-windows"
        ],
        "sources": {
          "C:\\Program Files (x86)\\Microsoft SDKs\\NuGetPackages\\": {},
          "C:\\Program Files\\dotnet\\library-packs": {},
          "https://api.nuget.org/v3/index.json": {}
        },
        "frameworks": {
          "net8.0-windows": {
            "framework": "net8.0-windows7.0",
            "targetAlias": "net8.0-windows",
            "projectReferences": {}
          }
        },
        "warningProperties": {
          "warnAsError": [
            "NU1605"
          ]
        },
        "restoreAuditProperties": {
          "enableAudit": "true",
          "auditLevel": "low",
          "auditMode": "direct"
        },
        "SdkAnalysisLevel": "10.0.400"
      },
      "frameworks": {
        "net8.0-windows": {
          "framework": "net8.0-windows7.0",
          "targetAlias": "net8.0-windows",
          "imports": [
            "net461",
            "net462",
            "net47",
            "net471",
            "net472",
            "net48",
            "net481"
          ],
          "assetTargetFallback": true,
          "warn": true,
          "downloadDependencies": [
            {
              "name": "Microsoft.AspNetCore.App.Ref",
              "version": "[8.0.31, 8.0.31]"
            },
            {
              "name": "Microsoft.NETCore.App.Host.win-x64",
              "version": "[8.0.31, 8.0.31]"
            },
            {
              "name": "Microsoft.NETCore.App.Ref",
              "version": "[8.0.31, 8.0.31]"
            },
            {
              "name": "Microsoft.WindowsDesktop.App.Ref",
              "version": "[8.0.31, 8.0.31]"
            }
          ],
          "frameworkReferences": {
            "Microsoft.NETCore.App": {
              "privateAssets": "all"
            },
            "Microsoft.WindowsDesktop.App.WPF": {
              "privateAssets": "none"
            }
          },
          "runtimeIdentifierGraphPath": "C:\\Program Files\\dotnet\\sdk\\10.0.401/PortableRuntimeIdentifierGraph.json"
        }
      }
    }
  }
}

notatnik.csproj.nuget.g.props

<?xml version="1.0" encoding="utf-8" standalone="no"?>
<Project ToolsVersion="14.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup Condition=" '$(ExcludeRestorePackageImports)' != 'true' ">
    <RestoreSuccess Condition=" '$(RestoreSuccess)' == '' ">True</RestoreSuccess>
    <RestoreTool Condition=" '$(RestoreTool)' == '' ">NuGet</RestoreTool>
    <ProjectAssetsFile Condition=" '$(ProjectAssetsFile)' == '' ">$(MSBuildThisFileDirectory)project.assets.json</ProjectAssetsFile>
    <NuGetPackageRoot Condition=" '$(NuGetPackageRoot)' == '' ">$(UserProfile)\.nuget\packages\</NuGetPackageRoot>
    <NuGetPackageFolders Condition=" '$(NuGetPackageFolders)' == '' ">C:\Users\Martyna\.nuget\packages\;C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages</NuGetPackageFolders>
    <NuGetProjectStyle Condition=" '$(NuGetProjectStyle)' == '' ">PackageReference</NuGetProjectStyle>
    <NuGetToolVersion Condition=" '$(NuGetToolVersion)' == '' ">7.0.0</NuGetToolVersion>
  </PropertyGroup>
  <ItemGroup Condition=" '$(ExcludeRestorePackageImports)' != 'true' ">
    <SourceRoot Include="C:\Users\Martyna\.nuget\packages\" />
    <SourceRoot Include="C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages\" />
  </ItemGroup>
</Project>

notatnik.csproj.nuget.g.targets

<?xml version="1.0" encoding="utf-8" standalone="no"?>
<Project ToolsVersion="14.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" />

project.assets.json

{
  "version": 4,
  "targets": {
    "net8.0-windows": {}
  },
  "libraries": {},
  "projectFileDependencyGroups": {
    "net8.0-windows": []
  },
  "packageFolders": {
    "C:\\Users\\Martyna\\.nuget\\packages\\": {},
    "C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\NuGetPackages": {}
  },
  "project": {
    "version": "1.0.0",
    "restore": {
      "projectUniqueName": "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\notatnik.csproj",
      "projectName": "notatnik",
      "projectPath": "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\notatnik.csproj",
      "packagesPath": "C:\\Users\\Martyna\\.nuget\\packages\\",
      "outputPath": "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\obj\\",
      "projectStyle": "PackageReference",
      "fallbackFolders": [
        "C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\NuGetPackages"
      ],
      "configFilePaths": [
        "C:\\Users\\Martyna\\AppData\\Roaming\\NuGet\\NuGet.Config",
        "C:\\Program Files (x86)\\NuGet\\Config\\Microsoft.VisualStudio.FallbackLocation.config",
        "C:\\Program Files (x86)\\NuGet\\Config\\Microsoft.VisualStudio.Offline.config"
      ],
      "originalTargetFrameworks": [
        "net8.0-windows"
      ],
      "sources": {
        "C:\\Program Files (x86)\\Microsoft SDKs\\NuGetPackages\\": {},
        "C:\\Program Files\\dotnet\\library-packs": {},
        "https://api.nuget.org/v3/index.json": {}
      },
      "frameworks": {
        "net8.0-windows": {
          "framework": "net8.0-windows7.0",
          "targetAlias": "net8.0-windows",
          "projectReferences": {}
        }
      },
      "warningProperties": {
        "warnAsError": [
          "NU1605"
        ]
      },
      "restoreAuditProperties": {
        "enableAudit": "true",
        "auditLevel": "low",
        "auditMode": "direct"
      },
      "SdkAnalysisLevel": "10.0.400"
    },
    "frameworks": {
      "net8.0-windows": {
        "framework": "net8.0-windows7.0",
        "targetAlias": "net8.0-windows",
        "imports": [
          "net461",
          "net462",
          "net47",
          "net471",
          "net472",
          "net48",
          "net481"
        ],
        "assetTargetFallback": true,
        "warn": true,
        "downloadDependencies": [
          {
            "name": "Microsoft.AspNetCore.App.Ref",
            "version": "[8.0.31, 8.0.31]"
          },
          {
            "name": "Microsoft.NETCore.App.Host.win-x64",
            "version": "[8.0.31, 8.0.31]"
          },
          {
            "name": "Microsoft.NETCore.App.Ref",
            "version": "[8.0.31, 8.0.31]"
          },
          {
            "name": "Microsoft.WindowsDesktop.App.Ref",
            "version": "[8.0.31, 8.0.31]"
          }
        ],
        "frameworkReferences": {
          "Microsoft.NETCore.App": {
            "privateAssets": "all"
          },
          "Microsoft.WindowsDesktop.App.WPF": {
            "privateAssets": "none"
          }
        },
        "runtimeIdentifierGraphPath": "C:\\Program Files\\dotnet\\sdk\\10.0.401/PortableRuntimeIdentifierGraph.json"
      }
    }
  }
}

notatnik.nuget.cache

{
  "version": 2,
  "dgSpecHash": "TckYjGlJOMk=",
  "success": true,
  "projectFilePath": "C:\\Users\\Martyna\\aigoogle\\notatnik\\notatnik\\notatnik.csproj",
  "expectedPackageFiles": [
    "C:\\Users\\Martyna\\.nuget\\packages\\microsoft.netcore.app.ref\\8.0.31\\microsoft.netcore.app.ref.8.0.31.nupkg.sha512",
    "C:\\Users\\Martyna\\.nuget\\packages\\microsoft.windowsdesktop.app.ref\\8.0.31\\microsoft.windowsdesktop.app.ref.8.0.31.nupkg.sha512",
    "C:\\Users\\Martyna\\.nuget\\packages\\microsoft.aspnetcore.app.ref\\8.0.31\\microsoft.aspnetcore.app.ref.8.0.31.nupkg.sha512",
    "C:\\Users\\Martyna\\.nuget\\packages\\microsoft.netcore.app.host.win-x64\\8.0.31\\microsoft.netcore.app.host.win-x64.8.0.31.nupkg.sha512"
  ],
  "logs": []
}

notatnik.Tests.csproj

<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\notatnik\notatnik.csproj" />
  </ItemGroup>

</Project>

NoteTests.cs

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
