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
- Status: Pass (log-verified)
- Tester: Codex scripted run
- Date: 2026-02-17 (CST) / 2026-02-18 (UTC)
- Result summary:
  - NVDA debug log evidence captured from `C:\Users\User\AppData\Local\Temp\nvda.log`.
  - Scripted key flow covered target applets and produced expected spoken lines for Phase 1 conformance:
    - Notes: `Notes.`, `Editor.`
    - Media Player: `Media Player. No tracks loaded. Press O to open a file.`, `No tracks loaded. Press O to open a file.`
    - MIDI Player: `MIDI Player. No file loaded. Press O to open a file.`, `No MIDI file loaded. Press O to open a file.`, `Stopped.`
    - App Launcher: `App Launcher. Favorites mode. 0 items.`, `Browse mode. 6 items.`
    - Calendar: `Calendar. Tuesday, February 17, 2026. 0 event(s).`, `Events list. 0 event(s).`
    - Contacts: `Contacts.`, `Contact form.`, `Contact actions.`
- Open issues:
  - During one scripted App Launcher run, `Enter` launched a local favorite (`As Dusk Falls`) as expected behavior.
  - Remaining validation gap is human auditory quality review (voice clarity/verbosity preference), not functional announcement presence.

### JAWS (Checklist Only)
- Status: Blocked (not installed on test machine)
- Tester: Codex environment preflight
- Date: 2026-02-17 (CST) / 2026-02-18 (UTC)
- Observations:
  - `jfw.exe` was not found via command lookup.
  - No JAWS process detected.
  - Checklist observations deferred until JAWS is installed on a validation machine.

### Narrator (Checklist Only)
- Status: Pass (runtime smoke), speech transcript not captured
- Tester: Codex scripted run
- Date: 2026-02-17 (CST) / 2026-02-18 (UTC)
- Observations:
  - `Narrator.exe` is present (`C:\Windows\System32\Narrator.exe`).
  - Narrator launched successfully.
  - AccessNote launched and accepted keyboard navigation smoke flow while Narrator was active.
  - Narrator process required manual OS-level closure in this environment (`Stop-Process`/`taskkill` were denied).

## Phase 2 Backlog Captured
- Install JAWS on a dedicated validation machine and run the same matrix.
- Add a repeatable Narrator transcript capture approach (or manual observation protocol) for deeper parity checks.
- Perform human auditory review pass for phrasing quality/timing (beyond log-presence validation).
