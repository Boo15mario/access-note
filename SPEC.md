# Access Note - v1 Specification

## Scope
Build a fullscreen Windows notetaker shell with a menu system and applets.
Primary requirement: strong NVDA compatibility from day one.
Detailed Settings implementation roadmap: `SETTINGS_MENU_PLAN.md`.

## Platform and Stack
- OS: Windows 10/11
- Language: C#
- UI: WPF (.NET 8)
- Pattern: MVVM
- Storage: SQLite

## Product Shape
- One fullscreen host app (`Shell`)
- Menu-driven navigation
- Internal applets launched from menu
- External program launching supported later

## Screen Map (v1)
1. Main Menu
2. Notes Workspace
3. Settings
4. Exit Confirmation Dialog

## v1 Main Menu
Top-level entries:
1. Notes
2. Settings
3. Utilities (placeholder)
4. Exit

## Notes Workspace Layout (v1)
- Header: `Notes`
- Left pane: `Notes List`
- Right pane: `Editor`
- Bottom bar: `Status`

## Settings Behavior (Current Requirement)
- Settings exists as a visible menu item and navigable screen.
- Settings screen provides a keyboard-first experience with categories and options.
- Dedicated category for note-related controls: `Notes Applet`.
- On entry, app announces settings navigation guidance.
- User can press `Esc` (or activate `Back`) to return to Main Menu.
- After return, focus lands on `Settings` in Main Menu (not top of list).
- Detailed option roadmap is defined in `SETTINGS_MENU_PLAN.md`.

## Settings Screen Layout (Current)
- Header: `Settings`
- Left pane: category list (`General`, `Notes Applet`, `Accessibility`, `Advanced`)
- Right pane: settings rows and hint text for selected row
- Action row:
  - `Save` button
  - `Reset Defaults` button
  - `Back` button

## Command Routing
- Global commands are handled by `Shell` first.
- If a global command does not apply, active screen handles it.
- If active screen does not handle it, command is ignored with status announcement.

## Keybindings (v1)

### Global
| Key | Behavior |
| --- | --- |
| `F1` | Repeat current screen help text |
| `Esc` | Back or cancel current context |
| `Alt+F4` | Open Exit Confirmation Dialog |

### Main Menu
| Key | Behavior |
| --- | --- |
| `Up` / `Down` | Move selection |
| `Home` / `End` | Jump to first/last item |
| `Enter` | Activate selected item |
| `Esc` | Open Exit Confirmation Dialog |

### Settings
| Key | Behavior |
| --- | --- |
| `Up` / `Down` | Move selection in current settings region |
| `Left` / `Right` | Change selected option value (or move between actions) |
| `Tab` / `Shift+Tab` | Cycle focus region: categories -> options -> actions |
| `Enter` | Activate current selection |
| `Ctrl+S` | Save settings |
| `Esc` | Return to Main Menu |
| `F1` | Speak Settings help text |

### Notes Workspace
| Key | Behavior |
| --- | --- |
| `F6` | Cycle focus: Notes List -> Editor -> Status -> Notes List |
| `Ctrl+L` | Focus Notes List |
| `Ctrl+E` | Focus Editor |
| `Ctrl+N` | Create new note and focus Editor |
| `Ctrl+S` | Save current note |
| `F2` | Rename selected note |
| `Delete` | Delete selected note (with confirmation dialog) |
| `Ctrl+F` | Focus Notes search box |
| `Esc` | Cancel active dialog/search, otherwise return to Main Menu |

## Focus and Selection Rules
- Every screen sets initial focus intentionally.
- Returning from Settings to Main Menu restores focus to `Settings`.
- Opening Notes from Main Menu sets focus to `Notes List`.
- If no note exists, focus moves to `Editor` with a new blank note selected.
- No focus loss during dialog open/close.

## Notes Editor Flow (v1)
1. User activates `Notes` in Main Menu.
2. App opens Notes Workspace and announces: `Notes`.
3. Notes List loads in last-modified order (newest first).
4. If notes exist, first note is selected and loaded in Editor.
5. If no notes exist, app creates an unsaved blank note and focuses Editor.
6. User edits in Editor with standard text navigation keys.
7. User presses `Ctrl+S` to save; status announces `Saved`.
8. User can press `Ctrl+N` to create another note.
9. User can rename (`F2`) or delete (`Delete`) from Notes List.
10. User can press `Esc` to return to Main Menu.

## Unsaved Changes Policy (v1)
- Dirty state is tracked for current note.
- On navigation away from a dirty note, show confirmation dialog:
  - `Save`
  - `Discard`
  - `Cancel`
- Dialog focus starts on `Save`.
- Dialog action is announced once when opened.

## Accessibility Requirements (v1)
- Every focusable control has explicit Automation `Name`.
- Every major region has a clear landmark name (Menu, Notes List, Editor, Status).
- Screen transitions announce screen title once.
- Settings guidance and focused controls are announced once on open.
- Save/delete/rename outcomes are announced through Status region.
- No keyboard trap on any screen.
- Main tasks usable with NVDA speech and keyboard only.

## Initial Applets
1. Notes Editor (basic text editing)
2. Settings (keyboard-first v1 options with persistence)

## Notes Editor (v1 target)
- New note
- Open note
- Edit/save note
- Rename/delete note
- Search notes list

## State and Persistence
- Keep user notes in SQLite.
- Keep user preferences in SQLite.

## Definition of Done for Current Milestone
1. App launches fullscreen.
2. Main Menu navigation works by keyboard.
3. Settings opens and presents layout.
4. Settings guidance is announced on open.
5. `Back` or `Esc` returns to Main Menu and restores focus to `Settings`.
6. Notes Workspace keybindings match this specification.
7. Unsaved changes dialog works with keyboard and NVDA announcements.
