# Notatnik (Avalonia UI / .NET)

Aplikacja desktopowa **Notatnik** zrealizowana w oparciu o framework **Avalonia UI** oraz platformę **.NET 10**.

## ✨ Funkcjonalności

- **Pole na tytuł**: umożliwia podanie lub edycję tytułu notatki.
- **Pole na treść**: wielolinijkowe pole tekstowe z zawijaniem wierszy i paskiem przewijania.
- **Przycisk zapisu**: zapisuje i aktualizuje notatki na bieżąco, zapisując dane do trwałego pliku na dysku.
- **Lista zapisanych notatek**: interaktywna lista po lewej stronie z tytułem, datą modyfikacji oraz podglądem treści; kliknięcie wczytuje notatkę do edytora.
- **Zapis do pliku Markdown (`notatki.md`)**:
  - Wszystkie notatki trafiają do jednego pliku `notatki.md` w folderze projektu.
  - Format wpisu:
    ```markdown
    ## 2026-09-08 14:32 — Tytuł notatki
    treść notatki, może mieć kilka linii
    ```
  - Pusta linia oddziela poszczególne wpisy.
  - **Najnowsza notatka znajduje się na samej górze pliku**.
  - Pełne zachowanie polskich znaków diakrytycznych (kodowanie UTF-8 bez sekwencji ucieczki).
- **Dodatkowe udogodnienia**:
  - `➕ Nowa notatka` – czyści pola i przygotowuje edytor pod nową notatkę.
  - `🗑️ Usuń wybraną notatkę` – usuwa wybraną notatkę i aktualizuje plik.
  - `📄 Eksportuj jako .txt` – pozwala wyeksportować bieżącą notatkę do osobnego pliku tekstowego w katalogu `eksport_txt`.
  - `📂 Otwórz folder z plikiem` – otwiera Eksplorator Windows w miejscu zapisanego pliku `notatki.md`.

## 🚀 Uruchomienie aplikacji

Aby uruchomić aplikację, wykonaj w terminalu polecenie:

```bash
dotnet run --project notatnik
```

## 🧪 Uruchomienie testów jednostkowych

Projekt zawiera zestaw testów xUnit sprawdzających m.in. zapis, parsowanie, kolejność wpisów oraz polskie znaki diakrytyczne:

```bash
dotnet test
```
