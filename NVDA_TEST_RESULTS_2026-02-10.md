# Access Note - NVDA Test Results (2026-02-10)

## Session Metadata
- Date: 2026-02-10
- OS: Windows 11 25H2 (10.0.26200.7705)
- NVDA: 2025.3.2 x86
- App: Access Note (`src/AccessNote`)
- Notes:
- NVDA add-ons were active during testing. Add-on-specific warnings and freezes were treated as noise unless they blocked core Access Note behavior.

## Change Verified In This Session
- Status live-region behavior fix:
- `src/AccessNote/MainWindow.xaml`: removed static `AutomationProperties.Name="Status"` from `StatusText`.
- `src/AccessNote/MainWindow.xaml.cs`: `Announce` now sets `AutomationProperties.SetName(StatusText, message)` before raising `LiveRegionChanged`.
- Goal: NVDA should announce actual status message content (for example `Saved.`, `Settings. Work in progress...`) instead of only `Status`.

## Key Evidence From Logs
- `20:47:13.920`: `Settings. Work in progress. Press Escape or Back to return.`
- `20:47:16.916`: `Main menu. Settings selected.`
- `20:47:38.379`: `Saved.`
- `20:47:51.722`: `Unsaved Changes` dialog announced, initial focus on `Save changes`.
- `20:51:38.220`: Back button path returned to `Main menu. Settings selected.`
- `20:51:44.642`: focus moved to `Discard changes` in Unsaved dialog.
- `20:51:44.993`: after discard selection, returned to `Main menu. Notes selected.`
- `20:51:50.728`: `Notes search box` announced after `Ctrl+F`.
- `20:51:50.732`: `Search.` status announcement spoken.
- `20:51:51.760`: `Notes List` announced after `Ctrl+L`.

## Case Status (From This Session)

| Case ID | Status | Evidence |
| --- | --- | --- |
| `NAV-001` | Pass | Startup announces app window and main menu item (`Notes`). |
| `NAV-002` | Pass | Main menu movement announces item names and positions. |
| `NAV-003` | Pass | Enter on `Notes` opens notes workspace and announces `Notes.` |
| `SET-001` | Pass | Settings entry announces `Work in progress` message. |
| `SET-002` | Pass | `Esc` in settings returns to main menu with focus on `Settings`. |
| `SET-003` | Pass | `Enter` on `Back` returns to main menu with focus on `Settings`. |
| `NOT-001` | Pass | Notes opens with list/editor focus behavior and selected note announcement. |
| `NOT-003` | Pass | `Ctrl+S` speaks `Saved.` after fix. |
| `NOT-004` | Pass | Rename flow announces `Renamed.` and updated note list focus. |
| `NOT-005` | Pass | Delete flow opens confirmation dialog and handles responses. |
| `NOT-006` | Pass | `F6` cycles focus across notes regions. |
| `NOT-007` | Pass | `Ctrl+L` and `Ctrl+E` move focus to expected regions. |
| `NOT-008` | Pass | `Ctrl+F` focuses search box and announces it. |
| `NOT-009` | Pass | Unsaved dialog opens with initial `Save` focus; discard path verified. |
| `NOT-010` | Pass | `Esc` returns to menu when no deeper context is active. |
| `A11Y-001` | Pass | No keyboard trap observed in covered flows. |
| `A11Y-002` | Pass (Observed) | Core transition announcements were present and understandable. |
| `A11Y-003` | Pass (Observed) | Role/name/state speech output matched expected controls in tested flows. |

## Open Items / Limits
- `NOT-002` (`Ctrl+N` create note) was not the focus of this final pass; behavior was previously observed in earlier runs.
- Full clean-room run without add-ons was not performed in this session.

## Addendum: Clean NVDA Run (Add-ons Disabled)
- Run mode: `--disable-addons --debug-logging`
- Clean-run baseline executed with manual keyboard flow.
- Observed in first clean run:
- Core flow executed end-to-end.
- False-positive `Command not available.` announcements occurred on valid shortcuts (`Ctrl+E`, `Ctrl+L`, and once on `Alt+F4`).
- Action taken:
- Keyboard fallback logic in `src/AccessNote/MainWindow.xaml.cs` was patched to normalize `Key.System`/`SystemKey` and avoid misclassifying valid command keys.
- Build verified after patch.
- Manual re-test setup used:
- a 5-second wait after window activation before key flow,
- lowercase control chords when using any synthesized keyboard tool.
- Manual flow completed end-to-end.
- One clean NVDA re-run is still required to confirm speech output is fully resolved.

## Addendum: Clean NVDA Re-Run (Pass)
- Date: 2026-02-10
- Run mode: `--disable-addons --debug-logging`
- Result: PASS for the manual clean baseline flow (see `NVDA_TEST_DATA.md`).
- Key confirmations:
- `Settings. Work in progress...` announced and Back returned to main menu with `Settings` selected.
- `Ctrl+E`, `Ctrl+S`, `Ctrl+F`, and `Ctrl+L` all worked without false `Command not available.`.
- Unsaved dialog opened; `Discard changes` path worked and returned to main menu.
- `Alt+F4` opened `Exit Access Note` dialog and exit confirmation succeeded.
- `Saved.` announcement present.
- Residual noise considered non-blocking:
- expected clean-mode synth fallback (`ibmeci` unavailable -> `oneCore`),
- benign UIA terminal warnings after app exit.

## Recommendation
- Use this file as the rolling test execution record for NVDA runs.
- Keep `NVDA_TEST_PLAN.md` as the stable plan and case catalog.
