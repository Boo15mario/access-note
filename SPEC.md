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
- Audio: NAudio, MeltySynth (for MIDI SoundFont playback)

## Product Shape
- One fullscreen host app (`Shell`)
- Menu-driven navigation with submenu support
- Internal applets launched from menu
- External program launching via App Launcher applet

## Screen Map
1. Home Screen
2. Notes Workspace
3. Settings
4. Media Center submenu
5. Date & Time (Utility)
6. Calculator (Utility)
7. System Monitor (Utility)
8. Calendar (Utility)
9. Contacts (Utility)
10. App Launcher (Utility)
11. Exit confirmation flow

## Home Screen Structure
```
Home Screen
├── Notes
├── Media Center ▸
│   ├── Media Player
│   └── MIDI Player
├── Utilities ▸
│   ├── Calendar
│   ├── Contacts
│   ├── Calculator
│   ├── System Monitor
│   ├── Date & Time
│   └── App Launcher
├── Settings
└── Exit
```

Notes remains the primary top-level applet.
Media applets are grouped under a "Media Center" submenu.
Utility applets are grouped under a "Utilities" submenu.
Enter on a submenu opens it; Escape returns to the Home Screen.

## Sound System
- `login.wav`: plays on application startup
- `vol.wav`: plays on system volume change
- Sounds can be enabled/disabled in Settings > General > "Sounds enabled"
- Sound files are located in the `sounds/` folder alongside the executable

## Theme System
- Built-in themes: Dark (default), Light, High Contrast
- Custom themes: JSON files (`*.theme.json`) in `%APPDATA%/AccessNote/themes/`
- Theme selection in Settings > General > "Theme"
- All UI elements use WPF `DynamicResource` bindings for theme colors
- Theme colors: Background, Foreground, Accent, Border, MenuBackground, EditorBackground, StatusBackground, StatusBorder, HeaderForeground, SelectionBackground, SelectionForeground

## v1 Home Screen
Top-level entries:
1. Notes
2. Media Center (submenu)
3. Utilities (submenu)
4. Settings
5. Exit

## Notes Workspace Layout (v1)
- Header: `Notes`
- Left pane: `Notes Viewer`
- Right pane: `Editor`
- Bottom bar: `Status`

## Settings Behavior (Current Requirement)
- Settings exists as a visible menu item and navigable screen.
- Settings screen provides a keyboard-first experience with categories and options.
- Dedicated category for note-related controls: `Notes Applet`.
- On entry, app announces settings navigation guidance.
- User can press `Esc` (or activate `Back`) to return to Home Screen.
- After return, focus lands on `Settings` on Home Screen (not top of list).
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
| `Alt+F4` | Open Exit confirmation flow |

### Home Screen
| Key | Behavior |
| --- | --- |
| `Up` / `Down` | Move selection |
| `Home` / `End` | Jump to first/last item |
| `Enter` | Activate selected item (or open submenu) |
| `Esc` | Return to parent menu (or open Exit confirmation flow at root) |

### Settings
| Key | Behavior |
| --- | --- |
| `Up` / `Down` | Move selection in current settings region |
| `Left` / `Right` | Change selected option value (or move between actions) |
| `Tab` / `Shift+Tab` | Cycle focus region: categories -> options -> actions |
| `Enter` | Activate current selection |
| `Ctrl+S` | Save settings |
| `Esc` | Return to Home Screen |
| `F1` | Speak Settings help text |

### Notes Workspace
| Key | Behavior |
| --- | --- |
| `F6` | Cycle focus: Notes Viewer -> Editor -> Status -> Notes Viewer |
| `Ctrl+L` | Focus Notes Viewer |
| `Ctrl+E` | Focus Editor |
| `Ctrl+N` | Create new note and focus Editor |
| `Ctrl+S` | Save current note |
| `F2` | Rename selected note |
| `Delete` | Delete selected note (with confirmation dialog) |
| `Ctrl+F` | Focus Notes search box |
| `Esc` | Cancel active dialog/search, otherwise return to Home Screen |

### Date & Time
| Key | Behavior |
| --- | --- |
| `Enter` | Announce current date and time via NVDA |
| `Esc` | Return to Utilities submenu |

### Calculator
| Key | Behavior |
| --- | --- |
| `0-9`, `.` | Input digits |
| `+`, `-`, `*`, `/` | Input operators |
| `Enter` | Evaluate expression |
| `Backspace` | Delete last character |
| `Ctrl+M` | Toggle Basic/Scientific mode |
| `Up` / `Down` | Navigate calculation history |
| `Esc` | Clear expression, or return to menu if empty |

### System Monitor
| Key | Behavior |
| --- | --- |
| `F5` | Manual refresh |
| `Up` / `Down` | Move between sections |
| `Esc` | Return to Utilities submenu |

### Calendar
| Key | Behavior |
| --- | --- |
| Arrow keys | Navigate days in month grid |
| `PageUp` / `PageDown` | Previous/next month |
| `Enter` | View/add event on selected date |
| `Ctrl+N` | Create new event |
| `Delete` | Delete selected event |
| `F6` | Cycle focus: calendar grid / events list |
| `Esc` | Return to Home Screen |

### Contacts
| Key | Behavior |
| --- | --- |
| `Ctrl+N` | New contact |
| `Ctrl+S` | Save contact |
| `Ctrl+F` | Focus search box |
| `Ctrl+I` | Import contacts (vCard) |
| `Ctrl+E` | Export contacts (vCard) |
| `F2` | Rename selected contact |
| `Delete` | Delete selected contact |
| `F6` | Cycle focus: contacts list / editor |
| `Esc` | Return to Home Screen |

### Media Player
| Key | Behavior |
| --- | --- |
| `Space` | Play/Pause |
| `S` | Stop |
| `N` | Next track |
| `P` | Previous track |
| `+` / `-` | Volume up/down |
| `M` | Mute toggle |
| `Left` / `Right` | Seek backward/forward 5 seconds |
| `O` | Open file to add to playlist |
| `U` | Add stream URL |
| `Esc` | Return to Home Screen |

### MIDI Player
| Key | Behavior |
| --- | --- |
| `Space` | Play/Pause |
| `S` | Stop |
| `+` / `-` | Increase/decrease tempo |
| `O` | Open MIDI file |
| `F3` | Select SoundFont (.sf2) |
| `Esc` | Return to Home Screen |

### App Launcher
| Key | Behavior |
| --- | --- |
| `Enter` | Launch selected app / navigate into directory |
| `Tab` | Toggle between Favorites and Browse modes |
| `Ctrl+N` | Add new favorite |
| `F2` | Rename selected favorite |
| `Delete` | Remove selected favorite |
| `Backspace` | Go up one directory (Browse mode) |
| `Esc` | Return to Home Screen |

## Focus and Selection Rules
- Every screen sets initial focus intentionally.
- Returning from Settings to Home Screen restores focus to `Settings`.
- Opening Notes from Home Screen sets focus to `Notes Viewer`.
- If no note exists, focus moves to `Editor` with a new blank note selected.
- No focus loss during dialog open/close.

## Notes Editor Flow (v1)
1. User activates `Notes` on Home Screen.
2. App opens Notes Workspace and announces: `Notes`.
3. Notes Viewer loads in last-modified order (newest first).
4. If notes exist, first note is selected and loaded in Editor.
5. If no notes exist, app creates an unsaved blank note and focuses Editor.
6. User edits in Editor with standard text navigation keys.
7. User presses `Ctrl+S` to save; status announces `Saved`.
8. User can press `Ctrl+N` to create another note.
9. User can rename (`F2`) or delete (`Delete`) from Notes Viewer.
10. User can press `Esc` to return to Home Screen.

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
- Every major region has a clear landmark name (Home Screen, Notes Viewer, Editor, Status).
- Screen transitions announce screen title once.
- Settings guidance and focused controls are announced once on open.
- Save/delete/rename outcomes are announced through Status region.
- No keyboard trap on any screen.
- Main tasks usable with NVDA speech and keyboard only.

## Initial Applets
1. Notes Editor (basic text editing)
2. Settings (keyboard-first v1 options with persistence)
3. Date & Time (live clock, date, week number, timezone)
4. Calculator (basic + scientific mode toggle)
5. System Monitor (CPU, memory, disk usage)
6. Calendar (month grid navigation, event CRUD)
7. Contacts (contact list with groups, vCard import/export)
8. Media Player (audio files + streaming, playlist)
9. MIDI Player (Windows synth + SoundFont via MeltySynth)
10. App Launcher (favorites + file browser)

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
2. Home Screen navigation works by keyboard.
3. Settings opens and presents layout.
4. Settings guidance is announced on open.
5. `Back` or `Esc` returns to Home Screen and restores focus to `Settings`.
6. Notes Workspace keybindings match this specification.
7. Unsaved changes dialog works with keyboard and NVDA announcements.
