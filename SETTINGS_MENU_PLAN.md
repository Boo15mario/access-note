# Access Note - Settings Menu Implementation Plan

## Goal
Replace the current Settings placeholder with a keyboard-first, NVDA-friendly settings experience that persists app preferences.

## Design Principles
- Keyboard-only completion for all settings tasks.
- Predictable focus and announcement behavior.
- Incremental rollout: ship useful settings first, then expand.
- Reuse existing dialog/announcement patterns from Notes and Exit flows.

## Out of Scope (for initial implementation)
- NVDA global configuration management.
- Cloud sync of preferences.
- Theme customization beyond simple light/dark toggle.

## Target IA (Information Architecture)
- `General`
- `Notes`
- `Accessibility`
- `Advanced`

Each category contains simple controls (toggle/select/input) with explicit labels and descriptions.

## v1 Settings (First Functional Release)
### General
- `Start screen`: `Main Menu` | `Notes`

### Notes Applet
- `Auto focus in Notes`: `List` | `Editor`
- `Confirm before deleting note`: `On` | `Off`
- `Notes sort order`: `Last modified (newest)` | `Last modified (oldest)` | `Title (A-Z)`

### Accessibility
- `Announce status messages`: `On` | `Off`
- `Announce hints on screen open`: `On` | `Off`

### Advanced
- `Reset settings to defaults` (button/action)

## Finalized v1 Rows (Control + Default + Storage Key)
| Category | Label | Control Type | Values | Default | Storage Key |
| --- | --- | --- | --- | --- | --- |
| General | Start screen | Select | `MainMenu`, `Notes` | `MainMenu` | `start_screen` |
| Notes Applet | Auto focus in Notes | Select | `List`, `Editor` | `List` | `notes_initial_focus` |
| Notes Applet | Confirm before deleting note | Toggle | `true`, `false` | `true` | `confirm_before_delete_note` |
| Notes Applet | Notes sort order | Select | `LastModifiedNewest`, `LastModifiedOldest`, `TitleAscending` | `LastModifiedNewest` | `notes_sort_order` |
| Accessibility | Announce status messages | Toggle | `true`, `false` | `true` | `announce_status_messages` |
| Accessibility | Announce hints on screen open | Toggle | `true`, `false` | `true` | `announce_hints_on_screen_open` |
| Advanced | Reset settings to defaults | Action | N/A | N/A | N/A |

## Keyboard Model (Settings Screen)
- `Up` / `Down`: move between settings rows.
- `Left` / `Right`: change value for toggle/select rows.
- `Enter`: activate button/action row.
- `Ctrl+S`: save settings immediately.
- `Esc`: return to Main Menu.
  - If there are unsaved settings edits: show confirmation dialog (`Save`, `Discard`, `Cancel`).
- `F1`: context help for Settings.

## Announcement Rules
- On Settings open: announce screen title once plus focused row label/value.
- On row focus: announce `label`, `value`, and short hint.
- On value change: announce updated value once.
- On save: announce `Settings saved.`
- On unsaved prompt result:
  - `Save`: `Settings saved.`
  - `Discard`: `Changes discarded.`
  - `Cancel`: `Navigation canceled.`

## Persistence Plan
- Introduce `AppSettings` model with explicit typed properties.
- Introduce `SettingsStorage` (SQLite-backed).
  - Table: `app_settings`
  - Schema: `key TEXT PRIMARY KEY, value TEXT NOT NULL`
- Load settings on startup before first screen render.
- Apply runtime settings where relevant:
  - Start screen selection.
  - Notes default focus target.
  - Delete confirmation behavior.
  - Notes list sort mode.
  - Optional status/hint announcement behavior.

## Implementation Phases
## Phase 1: Foundation
- Add `AppSettings` model + defaults.
- Add `SettingsStorage` load/save.
- Wire app startup to load settings.
- Keep current placeholder UI behavior unchanged.

## Phase 2: Settings UI Skeleton
- Replace placeholder text with:
  - category list (left),
  - setting rows/details (right),
  - save/reset actions.
- Add focus management and row navigation.
- Keep only one or two test settings active during this phase.

## Phase 3: Functional v1 Settings
- Implement all v1 settings listed above.
- Hook each setting into runtime behavior.
- Add unsaved-settings confirmation dialog.
- Add `Ctrl+S` settings save flow.

## Phase 4: NVDA Hardening and Regression
- Ensure no duplicate announcements.
- Verify key transitions (`Esc`, `Back`, `Ctrl+S`, `Left/Right`) across rows.
- Run full regression for Main Menu, Notes, Settings, and Exit flows.

## Test Plan Additions (IDs to add/use)
- `SET-101`: Open Settings and hear first row announcement.
- `SET-102`: Move between categories with keyboard.
- `SET-103`: Change toggle/select value with Left/Right and hear updated value.
- `SET-104`: Save with `Ctrl+S`, restart app, verify persistence.
- `SET-105`: Unsaved settings prompt appears on `Esc`; Save/Discard/Cancel paths work.
- `SET-106`: `Start screen` setting respected at next launch.
- `SET-107`: `Confirm before deleting note` affects delete workflow.
- `SET-108`: `Notes sort order` updates list ordering.

## Acceptance Criteria
- Settings screen is fully operable without mouse.
- All v1 settings persist across restart.
- No existing Notes/Main Menu shortcuts regress.
- NVDA announces focused controls and changed values clearly and once.
- Existing baseline script still passes after settings rollout (with updated expected prompts where applicable).
