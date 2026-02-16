# Access Note - NVDA Test Plan (v1)

## Purpose
Validate that Access Note is fully usable with NVDA and keyboard-only input for all v1 workflows.

## Latest Results
- Latest execution record: `NVDA_TEST_RESULTS_2026-02-10.md`
- Test-data reference: `NVDA_TEST_DATA.md`
- Settings feature roadmap: `SETTINGS_MENU_PLAN.md`
- Keep this document as the source of planned coverage and expected behavior.
- Keep dated run evidence in the results file above.

## Test Environment Matrix
- Windows 10 (latest updates)
- Windows 11 (latest updates)
- NVDA stable release currently in use by testers
- App build: current branch build under test

## Test Configuration
- NVDA speech mode: Talk
- NVDA punctuation: default
- Keyboard layout: desktop layout
- App starts with clean user profile and empty notes database unless stated otherwise

## Entry Criteria
1. App launches without crash.
2. Main Menu renders and receives keyboard focus.
3. Tester can hear NVDA speech output.

## Exit Criteria
1. All `Critical` cases pass.
2. No keyboard trap found.
3. No missing name/role/state announcement in core workflows.

## Severity Levels
- `Critical`: blocks core usage with NVDA/keyboard.
- `Major`: degraded accessibility but workaround exists.
- `Minor`: quality issue that does not block task completion.

## Core Test Cases

| ID | Severity | Scenario | Steps | Expected Result |
| --- | --- | --- | --- | --- |
| `NAV-001` | Critical | Startup focus | Launch app | NVDA announces main screen and focused menu item |
| `NAV-002` | Critical | Main menu navigation | Press `Up/Down`, `Home/End` | Selection changes and NVDA announces current item each time |
| `NAV-003` | Critical | Menu activation | Press `Enter` on `Notes` | Notes Workspace opens; NVDA announces `Notes` |
| `SET-001` | Critical | Settings screen opens | Open `Settings` from Main Menu | Settings categories/options are visible; NVDA announces settings guidance |
| `SET-002` | Critical | Settings return by Esc | Press `Esc` in Settings | Returns to Main Menu; focus restored to `Settings` |
| `SET-003` | Critical | Settings save shortcut | Change setting with arrows, press `Ctrl+S` | NVDA announces settings saved and value persists after restart |
| `SET-004` | Critical | Unsaved settings prompt | Change a setting, press `Esc` or activate `Back` | `Unsaved Settings` dialog opens with `Save`, `Discard`, `Cancel` choices |
| `SET-005` | Critical | Settings persistence (manual) | Change `Start screen`, save, restart app, then restore and repeat | Startup destination persists correctly across restart |
| `NOT-001` | Critical | Notes initial focus | Open `Notes` from Main Menu | Focus lands on Notes List (or Editor when no notes exist) |
| `NOT-002` | Critical | Create note | Press `Ctrl+N`, type text | New note context created; typing works in Editor |
| `NOT-003` | Critical | Save note | Press `Ctrl+S` | Note persists; NVDA announces save status |
| `NOT-004` | Critical | Rename note | Select note, press `F2`, confirm rename | New name appears in Notes List and is announced |
| `NOT-005` | Critical | Delete note | Select note, press `Delete`, confirm | Note removed; status announced; focus remains valid |
| `NOT-006` | Critical | Focus cycling | Press `F6` repeatedly | Focus cycles List -> Editor -> Status -> List with announcements |
| `NOT-007` | Critical | Focus shortcuts | Press `Ctrl+L` and `Ctrl+E` | Focus moves to requested region with NVDA announcement |
| `NOT-008` | Critical | Search focus | Press `Ctrl+F` | Search field gets focus and is announced correctly |
| `NOT-009` | Critical | Unsaved changes dialog | Edit note, attempt to leave note/workspace | Dialog opens with Save/Discard/Cancel; initial focus on Save |
| `NOT-010` | Critical | Escape behavior | Press `Esc` in search/dialog/workspace | Cancels context first; returns to Main Menu when no inner context |
| `A11Y-001` | Critical | No keyboard trap | Navigate all screens/dialogs by keyboard only | User can always move focus and exit current context |
| `A11Y-002` | Major | Announcement duplication | Repeat open/close of Settings and dialogs | Required announcement is spoken once per transition (no spam) |
| `A11Y-003` | Major | Control semantics | Inspect key controls with NVDA element info | Correct role/name/state exposed for focused control |

## Regression Checklist
1. Run all `Critical` cases on Windows 10 and Windows 11.
2. Run at least `NAV-001` through `NOT-010` on every feature branch touching UI.
3. Re-run `SET-001` through `SET-005` whenever Settings screen changes.
4. When functional settings ship, add and run `SET-101` through `SET-108` from `SETTINGS_MENU_PLAN.md`.

## Defect Logging Template
- Case ID:
- Build/Commit:
- OS + NVDA version:
- Actual behavior:
- Expected behavior:
- Repro steps:
- Severity:
- Notes or recording link:

## Optional Companion Tooling
- Accessibility Insights for Windows scan to catch automation name/control-pattern issues.
- Manual NVDA pass remains required even if tooling passes.
