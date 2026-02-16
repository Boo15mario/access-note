# Access Note Settings Test Checklist

## Scope
Manual NVDA regression checklist for the Settings area, including persistence.

## Preflight
1. Build:
   - `"/mnt/c/Program Files/dotnet/dotnet.exe" build "$(wslpath -w src/AccessNote/AccessNote.csproj)"`
2. Launch:
   - `"/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe" -NoProfile -Command "Start-Process -FilePath 'F:\git\access-note\src\AccessNote\bin\Debug\net8.0-windows\AccessNote.exe'"`
3. Optional process check:
   - `"/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe" -NoProfile -Command "Get-Process -Name AccessNote -ErrorAction SilentlyContinue | Select-Object Id,ProcessName"`
4. For clean NVDA runs, start NVDA with:
   - `nvda.exe --disable-addons --debug-logging -f "%TEMP%\nvda.log"`

## Settings Navigation Checks
1. From Main Menu, open `Settings`.
2. Verify categories announce and navigate with `Up/Down`:
   - `General`
   - `Notes Applet`
   - `Accessibility`
   - `Advanced`
3. Press `Enter` on a category and verify options list is focused and announced.
4. In an option row, use `Left/Right` to change value and confirm updated speech.
5. Press `Esc` with no pending edits and verify return to `Main Menu` with `Settings` selected.

## Unsaved Dialog Checks
1. Change `General -> Start screen`.
2. Press `Esc` and verify `Unsaved Settings` dialog appears.
3. Verify dialog actions:
   - `Save settings changes` applies and exits settings.
   - `Discard settings changes` exits settings without applying.
   - `Cancel settings navigation` keeps you in settings with pending change.

## Persistence Checks
1. Set `Start screen` to `Notes`, save, exit app, relaunch, verify startup lands in Notes.
2. Set `Start screen` back to `Main Menu`, save, exit app, relaunch, verify startup lands in Main Menu.

## Pass Criteria
- Category and option navigation are fully keyboard-operable.
- NVDA announces category changes, option focus, and value changes clearly.
- Unsaved dialog behavior matches `Save`/`Discard`/`Cancel` semantics.
- `Start screen` persists correctly across restarts in both directions.
