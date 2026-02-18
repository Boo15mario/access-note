# AppLauncher Dialog and Feedback Design (2026-02-18)

## Goal
Make App Launcher behavior closer to `../access-menu` for favorites management while improving spoken feedback reliability.

## Scope Decisions (Approved)
- Keep current App Launcher screen layout.
- Use a modular dialog adapter approach (no large refactor).
- Add an explicit button-driven add-to-favorites flow with dialog.
- Add standard spoken feedback level:
  - outcomes
  - mode changes
  - key navigation context

## Selected Approach
Use a focused **Dialog Adapter + Existing Module** pattern:
- Add `AppLauncherDialogService` to own add-favorite dialog UX.
- Keep `AppLauncherModule` as orchestrator.
- Expand `AppLauncherAnnouncementText` for consistent spoken outcomes.

## Architecture and Boundaries
- Keep behavior inside `src/AccessNote.Applets.AppLauncher`.
- Avoid adding behavior logic to host `MainWindow`.
- Wire existing `Add`, `Remove`, `Launch` buttons in `AppLauncherScreenView` to module handlers.
- Add `AddFavoriteDialog` in AppLauncher applet project.

## Component Model and Data Flow

### Entry
- Applet enters as today, loads favorites, announces current mode/count.
- Button events are bound on enter and unbound on leave.

### Add Favorite Flow (`Add` button / `Ctrl+N`)
- Module asks dialog service to show `AddFavoriteDialog`.
- Dialog provides two sources:
  - `Use current selection` (enabled only when Browse mode selection is a launchable file)
  - `Browse...` (file picker fallback)
- Dialog returns result payload (`path`, `displayName`, `source`).
- Module persists via `FavoriteAppStorage.Add(...)`, refreshes favorites list, announces one outcome.

### Remove Flow (`Remove` button / `Delete` in Favorites mode)
- If nothing selected: announce `No favorite selected.`
- On success: announce `Removed <name> from favorites.`

### Launch Flow (`Launch` button / `Enter`)
- Favorites mode: launch selected favorite.
- Browse mode:
  - directory => navigate in + announce directory/count
  - file => launch + announce launch outcome

## Spoken Feedback Contract (Standard)
- Keep existing mode-change announcements.
- Add clear add/remove/launch success/error announcements.
- Add explicit validation messages for invalid add source (e.g., directory not allowed for favorites).
- Keep one announce call per user action outcome.

## Error Handling
- Failures announce concise user-facing messages.
- Detailed exception text stays in logs where needed.
- Dialog cancel remains silent by default to avoid announcement noise.

## Testing Strategy
- Add/extend unit tests for:
  - announcement text helpers (new messages)
  - add-source decision rules (current selection valid/invalid, browse fallback)
  - button-triggered module flows:
    - add success
    - duplicate add
    - remove no-selection
    - remove success
- Preserve existing architecture boundaries and keep tests deterministic.

## Delivery Plan
1. Add dialog contract and implementation:
   - `IAppLauncherDialogService`
   - `AppLauncherDialogService`
   - `AddFavoriteDialog`
2. Wire `Add` / `Remove` / `Launch` button clicks to module commands.
3. Implement dual-source add flow in module (`current selection` or `browse`).
4. Expand `AppLauncherAnnouncementText` and align outcome announcements.
5. Add tests and run full Windows test suite.

## Out of Scope
- Full redesign of AppLauncher layout.
- Large coordinator-level refactor of entire AppLauncher module.
- Unrelated applet changes.
