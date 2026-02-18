# 10x Analysis: App Launcher (Games Submenu)
Session 1 | Date: 2026-02-18

## Current Value
App Launcher already delivers a strong accessibility-first baseline:
- Multi-source browse catalog (`Start Menu`, `Steam`, `Xbox`, `Heroic`) via `AppBrowseSourceFactory` (`src/AccessNote.Applets.AppLauncher/AppBrowseSourceFactory.cs:7`).
- Browse/favorites mode toggle with keyboard-first navigation (`Enter` to open, `Backspace` up) in `AppLauncherModule` (`src/AccessNote.Applets.AppLauncher/AppLauncherModule.cs:167`).
- Spoken feedback contract for mode changes, directory changes, and launch outcomes in `AppLauncherAnnouncementText` (`src/AccessNote.Applets.AppLauncher/AppLauncherAnnouncementText.cs:3`).
- Favorites support for non-path targets (`shellapp:` and `uri:`), which is critical for game launchers and packaged apps.

## The Question
What would make this not just a launcher, but a trusted "game command center" for screen reader users who need confidence that the right title will launch every time?

---

## Massive Opportunities

### 1. Canonical Game Identity Layer (Cross-Store Merge)
**What**: Introduce a canonical game identity model that maps Steam AppID, Heroic IDs (Epic/GOG/Amazon), Xbox package/app IDs, and Start-menu links into one deduped game graph.
**Why 10x**: Today users browse by source. Canonical identity shifts from "where is it installed" to "what game do I want," reducing cognitive load and duplicate hunting.
**Unlocks**: One game appears once, with "available launch targets" beneath it (Steam, Heroic direct exe, Xbox package). Future-ready for cloud sync and richer metadata.
**Effort**: Very High
**Risk**: Entity matching quality and long-tail edge cases (same title variants, DLC entries).
**Score**: 🔥 Must do

### 2. "Ask and Launch" Voice/Command Intent Layer
**What**: Add intent-driven launch commands (typed or spoken), e.g. "launch Hogwarts," "open latest played co-op game," "start Steam in Big Picture."
**Why 10x**: This removes deep tree navigation for frequent actions and is especially high-leverage for assistive workflows.
**Unlocks**: Personal shortcuts, adaptive ranking, context-aware commands.
**Effort**: High
**Risk**: Intent ambiguity and confidence UX.
**Score**: 👍 Strong

### 3. Reliability Mode: Observable Launch Pipeline
**What**: Move from fire-and-forget launching to a tracked launch pipeline: preflight checks, launch attempt, process/window confirmation, and spoken post-launch status.
**Why 10x**: Users gain certainty, not just action. Confidence is the core product moat in accessibility tooling.
**Unlocks**: Self-healing suggestions (bad path, missing package, stale shortcut), structured failure telemetry.
**Effort**: High
**Risk**: False positives when games spawn helper processes.
**Score**: 🔥 Must do

---

## Medium Opportunities

### 1. Cached Catalog + Background Refresh
**What**: Load last-known catalog instantly, then refresh sources in background with incremental UI updates.
**Why 10x**: Eliminates mode-switch latency and perceived freezes when source discovery is heavy.
**Impact**: Faster perceived performance and smoother NVDA interaction.
**Effort**: Medium
**Score**: 🔥 Must do

### 2. Title Confidence + Provenance
**What**: Track title confidence (`verified`, `inferred`, `fallback`) and source provenance (manifest, StartApps, install path).
**Why 10x**: Users know whether a title is exact vs inferred; trust increases.
**Impact**: Better spoken feedback and fewer launch mistakes.
**Effort**: Medium
**Score**: 🔥 Must do

### 3. Unified "Games" Superfolder (Source-agnostic)
**What**: Add top-level `Games` virtual folder aggregating all stores, while preserving per-source folders.
**Why 10x**: One place to browse games, optional deep source drill-down.
**Impact**: Major navigation simplification.
**Effort**: Medium
**Score**: 👍 Strong

### 4. Smart Ranking (Recent + Successful Launches)
**What**: Rank entries by recency, launch success, and user pinning within folders/search.
**Why 10x**: Makes frequent launches almost zero-friction.
**Impact**: Daily speed gain for power users.
**Effort**: Medium
**Score**: 👍 Strong

---

## Small Gems

### 1. Instant Filter-in-Place
**What**: Type-to-filter current folder with immediate spoken count updates.
**Why powerful**: Removes repeated arrow navigation for large lists.
**Effort**: Low
**Score**: 🔥 Must do

### 2. "Source Badge" in Spoken Detail
**What**: Announce source/store in details and optionally speech (e.g., "Hogwarts Legacy, Heroic").
**Why powerful**: Prevents accidental wrong-launch-store confusion.
**Effort**: Low
**Score**: 👍 Strong

### 3. One-Key Favorite From Browse
**What**: Dedicated key (for example `F`) to favorite current launchable entry without opening the add dialog.
**Why powerful**: Cuts a multi-step flow used frequently.
**Effort**: Low
**Score**: 🔥 Must do

### 4. Launch Dry-Run Check
**What**: Optional pre-launch validation command that verifies executable/package/URI resolvability.
**Why powerful**: Catches stale entries before failure.
**Effort**: Low
**Score**: 👍 Strong

---

## Recommended Priority

### Do Now (Quick wins)
1. Cached catalog + background refresh — Why: removes latency/freeze feel at the interaction hotspot.
2. Title confidence + provenance — Why: directly addresses trust and spoken clarity for game names.
3. Filter-in-place + one-key favorite — Why: immediate speed gains with low engineering cost.

### Do Next (High leverage)
1. Unified source-agnostic `Games` folder — Why: reduces navigation depth for the primary use case.
2. Reliability launch pipeline — Why: turns launcher from "try" into "guaranteed outcome".

### Explore (Strategic bets)
1. Canonical game identity layer — Why: biggest long-term moat; enables cross-store intelligence.
2. Intent/voice-driven launch — Why: potentially transformative for accessibility-first interaction.

### Backlog (Good but not now)
1. Deep social/collaboration features (shared launch profiles) — Why later: high value, but depends on identity + sync foundations.

---

## Evidence Used
- `Get-StartApps` returns Name + AppID (AppUserModelID) for current user; supports name filtering/wildcards.  
  Source: Context7 `/microsoftdocs/windows-powershell-docs`, `StartLayout/Get-StartApps.md`.
- `Get-AppxPackage` returns installed package metadata (including `PackageFamilyName`, `InstallLocation`, `IsFramework`) scoped to user profile unless broader options are used.  
  Source: Context7 `/microsoftdocs/windows-powershell-docs`, `Appx/Get-AppxPackage.md`.
- `Get-AppxPackageManifest` exposes application IDs from package manifests.  
  Source: Context7 `/microsoftdocs/windows-powershell-docs`, `Appx/Get-AppxPackageManifest.md`.
- Current code path confirms synchronous source discovery on browse-mode entry (`ToggleMode -> RefreshBrowseCatalog -> source.Discover`).  
  Source: `src/AccessNote.Applets.AppLauncher/AppLauncherModule.cs:184` and `src/AccessNote.Applets.AppLauncher/AppBrowseCatalogBuilder.cs:22`.

## Questions

### Answered
- **Q**: Is a StartApps-based title mapping direction technically valid for Xbox naming?  
  **A**: Yes; docs confirm `Get-StartApps` returns app `Name` with `AppID` (AppUserModelID), which maps well to shell app launch IDs.
- **Q**: Is package/manifest fallback still required?  
  **A**: Yes; `Get-AppxPackage` + manifest gives robust fallback when StartApps entries are missing or ambiguous.

### Blockers
- **Q**: Should we allow optional online metadata enrichment (Steam/Epic/GOG/Amazon lookups), or remain strictly offline/local only?
- **Q**: Should title confidence levels be spoken by default, or only exposed in details/help mode?

## Next Steps
- [ ] Decide offline-only vs optional online metadata policy.
- [ ] Approve "Do Now" batch scope (cache+refresh, title confidence, quick filter/favorite).
- [ ] Draft implementation plan for the approved batch with test cases and NVDA validation checklist.
