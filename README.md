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
