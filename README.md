# Access Note

Access Note is a keyboard-driven, accessibility-first Windows desktop application built with WPF and .NET 8. Designed for use with screen readers such as NVDA, it provides a suite of productivity applets accessible entirely through keyboard navigation.

## Features

- **Fully keyboard-driven** — no mouse required
- **Screen reader optimized** — tested with NVDA, uses UIA LiveRegion announcements
- **Fullscreen shell** with a navigable main menu
- **9 built-in applets** covering notes, media, utilities, and more
- **Theming support** with configurable settings
- **SQLite-backed storage** for notes, contacts, calendar events, and settings

## Applets

| Applet | Description |
|--------|-------------|
| **Notes** | Create, edit, search, and manage text notes with a multi-line editor |
| **Settings** | Configure application options across General, Notes, Accessibility, and Advanced categories |
| **Media Player** | Play audio files and streams with playlist management |
| **MIDI Player** | Play MIDI files with tempo control and optional SoundFont support |
| **App Launcher** | Launch favorite applications or browse the filesystem |
| **Calendar** | Navigate a monthly calendar and manage events |
| **Contacts** | Store and manage contacts with vCard import/export |
| **Utilities > Date & Time** | View the current date and time |
| **Utilities > Calculator** | Basic and scientific calculator with expression history |
| **Utilities > System Monitor** | View CPU, memory, and disk usage |

## Prerequisites

- Windows 10 or 11
- .NET 8 SDK (or newer)

## Build & Run

```powershell
# Build
dotnet build src/AccessNote/AccessNote.csproj

# Run
dotnet run --project src/AccessNote/AccessNote.csproj

# Run tests
dotnet test tests/AccessNote.Tests/AccessNote.Tests.csproj
```

## Data Storage

- SQLite database: `%LOCALAPPDATA%\AccessNote\access-note.db`
- Settings, notes, contacts, calendar events, and favorites are all persisted here.

## Keyboard Shortcuts

### Global

| Key | Action |
|-----|--------|
| Escape | Return to main menu (from any applet) |
| F1 | Announce help text for current screen |
| Alt+F4 | Exit application |

### Main Menu

| Key | Action |
|-----|--------|
| Up / Down | Navigate menu items |
| Home / End | Jump to first / last item |
| Enter | Open selected item |

### Notes

| Key | Action |
|-----|--------|
| Ctrl+N | Create new note |
| Ctrl+S | Save active note |
| Ctrl+L | Focus notes list |
| Ctrl+E | Focus editor |
| Ctrl+F | Focus search box |
| F2 | Rename note |
| F6 | Cycle focus (list, editor, search) |
| Delete | Delete selected note |

### Settings

| Key | Action |
|-----|--------|
| Up / Down | Navigate categories and options |
| Left / Right | Change option values |
| Tab | Switch between regions |
| Ctrl+S | Save settings |

### Media Player

| Key | Action |
|-----|--------|
| Space | Play / Pause |
| S | Stop |
| N | Next track |
| P | Previous track |
| T | Announce track info (title, position, remaining time) |
| + | Volume up |
| - | Volume down |
| M | Toggle mute |
| Left / Right | Seek backward / forward 5 seconds |
| O | Open audio file(s) |
| U | Add stream URL |

### MIDI Player

| Key | Action |
|-----|--------|
| Space | Play / Pause |
| S | Stop |
| O | Open MIDI file |
| + | Increase tempo (+10 BPM) |
| - | Decrease tempo (-10 BPM) |
| F3 | Load SoundFont |

### App Launcher

| Key | Action |
|-----|--------|
| Tab | Toggle mode (Favorites / Browse) |
| Enter | Launch application or open directory |
| Ctrl+N | Add favorite (Favorites mode) |
| F2 | Rename favorite (Favorites mode) |
| Delete | Remove favorite (Favorites mode) |
| Backspace | Navigate up directory (Browse mode) |

### Calendar

| Key | Action |
|-----|--------|
| Left / Right | Previous / next day |
| Up / Down | Previous / next week |
| Page Up / Page Down | Previous / next month |
| Enter | View events for selected date |
| Ctrl+N | Create new event |
| Delete | Delete selected event |
| F6 | Toggle focus between calendar grid and events list |

### Contacts

| Key | Action |
|-----|--------|
| Ctrl+N | New contact |
| Ctrl+S | Save contact |
| Ctrl+I | Import contacts (vCard) |
| Ctrl+E | Export contacts (vCard) |
| Ctrl+F | Focus search box |
| F6 | Cycle focus (list, form, buttons) |
| Delete | Delete selected contact |

### Calculator

| Key | Action |
|-----|--------|
| 0-9, +, -, *, /, . | Enter numbers and operators |
| Enter | Evaluate expression |
| Backspace | Delete last character |
| Escape | Clear expression |
| Ctrl+M | Toggle mode (Basic / Scientific) |
| Up / Down | Navigate expression history |

Scientific mode adds: S (sin), C (cos), T (tan), L (log), N (ln), P (pow).

### Date & Time

| Key | Action |
|-----|--------|
| Any key | Return to main menu |

### System Monitor

| Key | Action |
|-----|--------|
| Up / Down | Navigate sections (CPU, Memory, Disks) |
| F5 | Refresh display |

## License

All rights reserved.
