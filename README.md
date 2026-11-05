# Notatnik (Avalonia UI / .NET)

Aplikacja desktopowa **Notatnik** zrealizowana w oparciu o framework **Avalonia UI** oraz platformę **.NET 10**.

## ✨ Funkcjonalności

- **Pole na tytuł**: umożliwia podanie lub edycję tytułu notatki.
- **Pole na treść**: wielolinijkowe pole tekstowe z zawijaniem wierszy i paskiem przewijania.
- **Przycisk zapisu**: zapisuje i aktualizuje notatki na bieżąco, zapisując dane do trwałego pliku na dysku.
- **Lista zapisanych notatek**: interaktywna lista po lewej stronie z tytułem, datą modyfikacji oraz podglądem treści.
- **Trwały zapis do pliku**: notatki są serializowane do pliku `notatki.json` (obsługa formatowania i polskich znaków diakrytycznych).
- **Dodatkowe udogodnienia**:
  - `➕ Nowa notatka` – czyści pola i przygotowuje edytor pod nową notatkę.
  - `🗑️ Usuń wybraną notatkę` – kasuje notatkę i aktualizuje plik.
  - `📄 Eksportuj jako .txt` – pozwala wyeksportować bieżącą notatkę do osobnego pliku tekstowego w katalogu `eksport_txt`.
  - `📂 Otwórz folder z plikiem` – otwiera Eksplorator Windows w miejscu zapisanego pliku.

## 🚀 Uruchomienie aplikacji

Aby uruchomić aplikację, wykonaj w terminalu polecenie:

```bash
dotnet run --project notatnik
```

## 🧪 Uruchomienie testów jednostkowych

Projekt zawiera zestaw testów xUnit sprawdzających m.in. zapis, odczyt oraz polskie znaki diakrytyczne:

```bash
dotnet test
```

