# Access Note - NVDA Test Data (Manual)

## Purpose
Keep repeatable keyboard test data in one place for manual NVDA verification runs.

## Run Commands (WSL -> Windows)
1. Build app:
   - `"/mnt/c/Program Files/dotnet/dotnet.exe" build "$(wslpath -w src/AccessNote/AccessNote.csproj)"`
2. Launch app:
   - `"/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe" -NoProfile -Command "Start-Process -FilePath 'F:\git\access-note\src\AccessNote\bin\Debug\net8.0-windows\AccessNote.exe'"`
3. Optional process check:
   - `"/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe" -NoProfile -Command "Get-Process -Name AccessNote -ErrorAction SilentlyContinue | Select-Object Id,ProcessName"`
4. Start clean NVDA (Windows side):
   - `nvda.exe --disable-addons --debug-logging -f "%TEMP%\nvda.log"`

## Manual Baseline Keyboard Flow
1. `Home`, `Down`, `Enter`, `Esc` -> open Settings, return to Main Menu.
2. `Home`, `Enter` -> open Notes.
3. `Ctrl+E`, type ` baseline`, `Ctrl+S`.
4. `Esc`, `Enter`, `Ctrl+E`, type ` discard`, `Esc`, `Tab`, `Enter` (discard).
5. `Enter`, `F6` x4, `Ctrl+F`, `Ctrl+L`.
6. `Esc`, `Alt+F4`, `Enter`.

## Manual Settings Persistence Flow
1. Open `Settings`.
2. In categories, verify `General`, `Notes Applet`, `Accessibility`, and `Advanced`.
3. Enter `General` and change `Start screen` from `Main Menu` to `Notes`.
4. Press `Esc`, then test the `Unsaved Settings` dialog paths:
   - `Cancel` returns you to settings with pending changes.
   - `Discard` returns to menu without applying.
   - `Save` persists the value.
5. Exit app and relaunch.
6. Verify app starts in `Notes`.
7. Return to `General`, set `Start screen` back to `Main Menu`, save, relaunch, and verify Main Menu start.

## Expected Speech Landmarks
- `Settings category. General.`
- `Settings options list`
- `Start screen. Main Menu. Left and Right to change startup destination.`
- `Unsaved Settings` dialog with `Save settings changes` initial focus
- `Main menu. Settings selected.`
- `Saved.`
- `Notes List` when startup is `Notes`
