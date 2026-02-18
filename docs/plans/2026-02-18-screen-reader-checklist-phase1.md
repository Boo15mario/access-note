# Screen Reader Checklist (Phase 1)

## Purpose
Track manual verification across NVDA, JAWS, and Narrator for Phase 1 accessibility conformance updates.

## Scope
- Phase 1 applets:
  - Media Player
  - MIDI Player
  - App Launcher
  - Calendar
  - Notes
  - Contacts

## Test Environment
- OS: Windows 10/11
- Build: latest local debug build
- Screen readers:
  - NVDA (required pass in Phase 1)
  - JAWS (checklist only in Phase 1)
  - Narrator (checklist only in Phase 1)

## Expected Speech Matrix

### Media Player
- `Space`: playback state announced once (`Playing` / `Paused` / `Stopped`).
- `N` / `P`: track change announced once with current state context.
- `O` / `U`: added track(s) announcement is concise and single-call.
- no rapid double-announcement for one key action.

### MIDI Player
- `O`: file load success/failure announced once.
- `Space`: playback state announced once.
- `S`: stop state announced once.
- playback completion announces once.

### App Launcher
- `Tab`: mode switch announced (`Favorites mode` / `Browse mode`).
- Enter folder / `Backspace`: directory navigation feedback announced with item count.
- launch success/failure announcement is concise and single-call.

### Calendar
- date navigation announces selected date with event count context.
- grid-to-events transition announces events list context.
- event create/delete announces outcome once.

### Notes
- `F6` cycle announces region transitions (`Notes list`, `Editor`, status region text).
- note selection announcement remains clear and single-call.
- search clear/escape feedback remains understandable.

### Contacts
- list selection announces selected contact.
- `F6` cycle announces focus region (`Contacts list`, `Contact form`, `Contact actions`).
- add/save/delete/import/export outcomes announce once.

## Execution Log

### NVDA (Phase 1 Required)
- Status: Pending
- Tester:
- Date:
- Result summary:
- Open issues:

### JAWS (Checklist Only)
- Status: Pending
- Tester:
- Date:
- Observations:

### Narrator (Checklist Only)
- Status: Pending
- Tester:
- Date:
- Observations:
