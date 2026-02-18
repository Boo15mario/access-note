# Accessibility Contract v1 (NVDA-First)

## Purpose
Define a consistent, modular accessibility behavior contract across applets so announcements and help text remain predictable as the app grows.

## Scope
- Applies to all applets and shell-routed help (`F1`).
- Phase 1 implementation target: NVDA behavior.
- JAWS/Narrator: checklist and expected speech matrix in this phase; parity fixes deferred.

## Core Rules

### 1) Message Style
- Use short, explicit outcome-first messages.
- Prefer one sentence for one user action.
- Avoid filler phrases and avoid repeating the applet name unless context changed.
- Use consistent wording for common actions (selected, saved, deleted, failed, opened, mode changed).

### 2) Announce vs Native Selection Speech
- Do not duplicate what list navigation already announces through native selection patterns.
- Announce when:
  - mode changes
  - state changes (play/pause/stop, loaded/unloaded, saved/failed)
  - operation outcomes (created/deleted/imported/exported)
  - focus-region changes that are not obvious from native speech
- Prefer native speech for simple row-to-row movement where "X of Y" is already read.

### 3) Single-Call Announcement Rule
- For one user action, issue at most one `StatusAnnouncer.Announce(...)` call.
- If multiple facts are needed, combine into one message.
- Rationale: rapid consecutive live-region updates can cancel each other and reduce reliability.

### 4) Error Announcement Rule
- On failure, announce one concise failure message.
- Keep technical details in logs/diagnostics, not spoken status text.
- Format:
  - `"Could not <action>. <short reason or next step>."`

## F1 Help Contract

### Minimum Requirements
Each applet `HelpText` must:
- Start with the applet name.
- Include the highest-value controls for that applet.
- Include exit guidance (`Escape` to return).
- Be non-empty and not placeholder text.

Each applet `ScreenHintText` must:
- Be non-empty.
- Be context-setting and short.

### Help Text Template
`<Applet>. <Primary actions>. <Secondary actions>. Escape to return.`

Example:
`Media Player. Space play or pause, S stop, N next, P previous. O open file, U add stream URL. Escape to return.`

## Event-to-Message Contract (Phase 1 Priority)

### Media Player
- Track change: announce selected track/title context.
- Playback state: announce play, pause, stop transitions.

### MIDI Player
- File load: announce loaded file name or failure.
- Playback state: announce play, pause, stop transitions.

### App Launcher
- Mode switch: announce Favorites/Browse mode entry.
- Navigation feedback: announce actionable context change (selected app/path).

### Calendar
- Announce meaningful selection context changes during calendar navigation.

### Notes
- Announce meaningful editing/focus context changes.

### Contacts
- Announce contact list navigation context and key operation outcomes.
- Feature expansion is out of scope for this phase.

## Non-Goals (v1)
- No large centralized accessibility framework.
- No JAWS/Narrator parity bug-fixing in this phase.
- No feature expansion for Contacts.

## Validation Requirements
- Unit tests:
  - contract doc exists
  - applet descriptors avoid placeholder help text
  - help/hint metadata is present where required
- Manual:
  - NVDA verification pass per updated applet
  - JAWS/Narrator checklist completed and stored
