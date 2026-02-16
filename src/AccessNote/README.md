# Access Note (WPF Prototype)

## Prerequisites
- Windows 10/11
- .NET 8 SDK (or newer SDK that can build `net8.0-windows`)

## Build
```powershell
dotnet build src/AccessNote/AccessNote.csproj
```

## Run
```powershell
dotnet run --project src/AccessNote/AccessNote.csproj
```

## Current Implemented Flow
- Fullscreen shell window
- Keyboard-driven Main Menu (`Notes`, `Settings`, `Utilities`, `Exit`)
- Notes workspace with:
  - Notes list + search box
  - Multi-line editor
  - SQLite-backed note create/save/rename/delete
  - Unsaved changes dialog (`Save`, `Discard`, `Cancel`)
  - Focus cycle (`F6`) and shortcuts (`Ctrl+N/S/F/L/E`)
  - Search across note title and note body
- `Settings` screen with categories (`General`, `Notes Applet`, `Accessibility`, `Advanced`) and editable options
- Settings keyboard navigation (`Tab`, arrows, `Enter`, `Ctrl+S`, `Esc`) with persisted values
- Unsaved settings prompt on leave (`Save`, `Discard`, `Cancel`)
- `Notes Applet` settings are grouped in their own category
- `Back`/`Esc` returns to Main Menu and restores focus to `Settings`
- Exit confirmation dialog for `Esc` on Main Menu and `Alt+F4`

## Notes Storage
- SQLite file path: `%LOCALAPPDATA%\AccessNote\access-note.db`

## Known Limit
- Settings screen is functional but still an incremental implementation (further refinement tracked in `SETTINGS_MENU_PLAN.md`).
