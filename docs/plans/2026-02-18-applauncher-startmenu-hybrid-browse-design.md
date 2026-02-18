# AppLauncher Start Menu Hybrid Browse Design (2026-02-18)

## Goal
Replace filesystem-based Browse mode with a modular Start Menu-first catalog model, while adding platform coverage for Steam, Xbox, and Heroic in a maintainable architecture.

## Approved Scope and Decisions
- Browse mode becomes Start Menu-based (not raw filesystem traversal).
- Keep Favorites mode behavior and keyboard model intact.
- Use modular catalog architecture (Option 2: Browse Catalog Layer).
- Source strategy: Hybrid.
  - Start Menu source plus dedicated platform sources.
- Missing source behavior: silent skip.
- Xbox source: installed app package inventory only.
- Heroic source: launcher + installed Heroic games.
- Steam source: launcher + installed Steam games.

## Architecture
Add a browse-catalog layer in `AccessNote.Applets.AppLauncher`:

- `IAppBrowseSource`
  - Source contract for discovery providers.
  - Implementations:
    - `StartMenuBrowseSource`
    - `SteamBrowseSource`
    - `XboxBrowseSource`
    - `HeroicBrowseSource`

- `AppBrowseCatalogBuilder`
  - Merges discovered records from all sources.
  - Applies stable sorting and dedupe rules.
  - Produces a virtual browse tree.

- `AppBrowseNavigator`
  - Owns current browse location and stack.
  - Exposes current folder entries.
  - Handles folder enter/up behavior.

- `AppLauncherProcessLauncher`
  - Launch abstraction for browse/favorites entries.
  - Supports multiple launch target types.

- `AppLauncherModule`
  - Remains orchestration/UI-only.
  - Delegates browse entry discovery/navigation to catalog layer.

## Browse Data Model
Browse tree top-level folders:

- `[Start Menu]`
- `[Steam]`
- `[Xbox]`
- `[Heroic]`

Tree nodes contain either:

- folder node
- launchable app node (`LaunchSpec`)

`LaunchSpec` supports:

- `DirectPath` (`.exe`, `.lnk`, `.bat`)
- `ShellApp` (AUMID / shell app model)
- `CustomCommand` (e.g., Steam/Heroic command or URI)

## Source Behavior
### Start Menu
- Scan both:
  - `C:\ProgramData\Microsoft\Windows\Start Menu\Programs`
  - `%AppData%\Microsoft\Windows\Start Menu\Programs`
- Include `.lnk`, `.exe`, `.bat`.
- Apply access-menu-like duplicate suppression for root-level duplicates where feasible.

### Steam
- Add Steam launcher entry.
- Parse Steam libraries and installed app manifests for game entries.
- Map each to launch spec using Steam-compatible launch command/URI.

### Xbox
- Use installed app package inventory only.
- Build launch specs from app identifiers suitable for shell activation.

### Heroic
- Add Heroic launcher entry.
- Parse Heroic installed-game metadata and map to launch specs.

### Availability Rules
- If a source is unavailable/unreadable, skip it silently.
- No browse-mode warning noise for missing sources.

## Interaction and UX
- Keep existing key model:
  - `Tab`: switch Favorites/Browse
  - `Enter`: open folder or launch app
  - `Backspace`: navigate up
- Keep existing Add/Remove/Launch buttons.
- Browse mode list displays tree entries from navigator, not filesystem entries.

## Favorites Compatibility
- Existing favorites (`.exe/.lnk/.bat`) remain unchanged.
- New favorites from platform entries can store normalized launch tokens for non-path targets.
- Duplicate checks use normalized launch token/path, not display text.
- Launch pipeline resolves legacy path entries and new tokenized entries.

## Announcements
- Preserve one-call outcome announcements per action.
- Keep mode/entry announcements already in contract.
- Ensure launch/add/remove messaging works for path and non-path launch specs.

## Error Handling
- Discovery failures: silent skip by source.
- Launch failures: concise failure announcement and no crash.
- Stale entries: report unavailable target and keep UI responsive.

## Testing Strategy
Add focused tests for:

- catalog merge, dedupe, and sorting
- navigator folder traversal and activation behavior
- source parsing (Steam libraries, Heroic metadata, Xbox app mapping via mocked provider)
- favorites integration for launch tokens and duplicate behavior
- regression coverage for existing path favorites and browse announcements

## Non-Goals
- No redesign of AppLauncher screen layout.
- No global shell architecture changes.
- No feature expansion outside AppLauncher browse/favorites-launch behavior.
