# Access Note Modularization Plan

## Goal
Make the codebase as modular as possible so it is easier to maintain, test, and extend when we add or work on other applets later.

## Why This Matters
- Reduces coupling between screens and workflows.
- Makes feature changes safer and faster.
- Improves testability of behavior without full UI integration.
- Lets future applets reuse patterns instead of duplicating logic.

## Next Modularization Steps (Priority Order)
1. Split `NotesActionsCoordinator` into focused handlers:
   - Status: Completed
   - Progress notes:
     - Added `CreateNoteAction`, `SaveNoteAction`, `RenameNoteAction`, `DeleteNoteAction`, and `UnsavedNoteGuard`.
     - Refactored `NotesActionsCoordinator` into orchestration/delegation only.
     - Verified with Windows test run (`18/18` passing).
   - Scope: `CreateNoteAction`, `SaveNoteAction`, `RenameNoteAction`, `DeleteNoteAction`, `UnsavedNoteGuard`.
   - Estimated effort: 0.5-1 day.
   - Done when:
     - `NotesActionsCoordinator` is primarily orchestration, not full behavior implementation.
     - Each action has its own class/file with narrow dependencies.
     - Existing note behaviors (create/save/rename/delete/unsaved prompt flow) are unchanged.
     - Build and tests pass.
2. Split `InputCommandRouter` into focused maps/policies:
   - Status: Completed
   - Progress notes:
     - Added `GlobalInputMap`, `MainMenuInputMap`, `NotesInputMap`, and `IgnoredCommandPolicy`.
     - Kept `InputCommandRouter` as a small facade to avoid broad call-site churn.
     - Verified with Windows test run (`18/18` passing).
   - Scope: `GlobalInputMap`, `MainMenuInputMap`, `NotesInputMap`, `IgnoredCommandPolicy`.
   - Estimated effort: 0.5 day.
   - Done when:
     - Command mapping responsibilities are separated by screen/concern.
     - Existing key behavior remains unchanged.
     - Build and tests pass.
3. Move nested input contract classes out of `MainWindowCompositionRoot` into standalone files.
   - Status: Completed
   - Progress notes:
     - Added top-level composition contract types in `MainWindowCompositionInputs.cs`.
     - Removed nested contract classes from `MainWindowCompositionRoot`.
     - Updated factories and composition wiring to use the extracted contracts.
     - Verified with Windows test run (`18/18` passing).
   - Scope: extract `Inputs`, `CoreInputs`, `ShellInputs`, `NotesInputs`, `SettingsInputs`, and other nested contract types.
   - Estimated effort: 0.5 day.
   - Done when:
     - `MainWindowCompositionRoot` no longer contains nested contract classes.
     - Contract types are grouped and discoverable in dedicated files.
     - Build and tests pass.
4. Add tests for modular boundaries:
   - Status: Completed
   - Progress notes:
     - Added `MainMenuModuleTests` for typed action activation paths.
     - Added `SettingsOptionListBuilderTests`, `SettingsSelectionSynchronizerTests`, and `SettingsOptionAnnouncerTests`.
     - Added STA/WPF test helpers for UI-bound adapters.
     - Verified with Windows test run (`25/25` passing).
   - Scope: tests for `MainMenuModule` typed action mapping, `SettingsOptionListBuilder`, `SettingsSelectionSynchronizer`, `SettingsOptionAnnouncer`.
   - Estimated effort: 0.5 day.
   - Done when:
     - New tests cover expected behavior and edge cases for each target.
     - Tests are deterministic and do not require UI runtime.
     - Test suite passes in Windows PowerShell run.
5. Move controller wiring out of `MainWindow` constructor into a dedicated composition/factory step.
   - Status: Completed
   - Progress notes:
     - Moved controller construction (`ShellNavigationController`, `NotesEventController`, `SettingsEventController`) into `MainWindowCompositionRoot`.
     - `MainWindow` constructor now assigns composed controller instances only.
     - Verified with Windows test run (`25/25` passing).
   - Scope: constructor only receives/assigns composed objects; controller creation moved to composition/factory layer.
   - Estimated effort: 0.25-0.5 day.
   - Done when:
     - `MainWindow` constructor does minimal wiring.
     - Controller construction lives in dedicated factory/composition code.
     - Build and tests pass.
6. Reorganize files into feature folders (`Shell`, `Notes`, `Settings`, `Composition`) to improve discoverability.
   - Status: Completed
   - Progress notes:
     - Moved core feature `.cs` files into `src/AccessNote/Composition`, `src/AccessNote/Shell`, `src/AccessNote/Notes`, and `src/AccessNote/Settings`.
     - Kept namespaces stable to avoid behavioral changes.
     - Verified with Windows test run (`25/25` passing).
   - Scope: file moves only, namespace-safe, no behavior changes.
   - Estimated effort: 0.5-1 day.
   - Done when:
     - Core feature files are grouped by domain folder.
     - References/usings compile cleanly after moves.
     - Build and tests pass.

## Effort Summary
- Total estimated effort: ~2.75 to 4.0 engineering days.
- Preferred execution sequence: `1 -> 2 -> 3 -> 4 -> 5 -> 6`.
- Risk profile:
  - Lower risk: `2`, `3`, `5`, `6` (mostly structural).
  - Medium risk: `1`, `4` (behavior-sensitive and test depth).

## Current Plan Status
- Step 1: Completed
- Step 2: Completed
- Step 3: Completed
- Step 4: Completed
- Step 5: Completed
- Step 6: Completed

## Applet Plan Status
- Step 1: Completed
- Step 2: Completed
- Step 3: Completed
- Step 4: Completed
- Step 5: Completed
- Step 6: Completed
- Step 7: Completed

## Working Rule
For every new change, prefer adding logic to modules/controllers/policies instead of `MainWindow` code-behind unless there is a strong reason not to.

## Applet Modularity Plan (Next Wave)
Goal: shift from screen-specific wiring to applet-first architecture so future applets can be added with minimal shell changes.

1. Introduce applet contract (`IApplet`, `AppletDescriptor`).
   - Status: Completed
   - Progress notes:
     - Added `IApplet`, `AppletDescriptor`, and `AppletId`.
     - Added `NotesApplet` and `SettingsApplet` adapters for existing applets.
     - Refactored `ScreenRouter` to operate through applet abstraction.
     - Verified with Windows test run (`25/25` passing at this step).
   - Scope: add applet abstraction and adapt current flow so routing/shell depend on applet interface rather than applet internals.
2. Add applet registry (`AppletRegistry`).
   - Status: Completed
   - Progress notes:
     - Added `AppletRegistry` with duplicate-id validation and required lookup API.
     - Routed `ScreenRouter` through registry lookups instead of direct applet fields.
     - Added `AppletRegistryTests`.
     - Verified with Windows test run (`28/28` passing).
   - Scope: register applets centrally; drive shell/menu/routing lookups from registry.
3. Migrate Notes and Settings to registered applets.
   - Status: Completed
   - Progress notes:
     - Updated main menu actions to activate applets by `AppletId` instead of hardcoded Notes/Settings delegates.
     - Updated navigation flow to `OpenApplet(AppletId)` and route activation via registry-backed `ScreenRouter`.
     - Updated/kept tests green (`28/28` passing).
   - Scope: route activations through registry-backed applet descriptors.
4. Split into projects (`AccessNote.Core`, `AccessNote.Shell`, applet projects).
   - Status: Completed
   - Progress notes:
     - Created `src/AccessNote.Core/AccessNote.Core.csproj` and moved shared domain types (`AppSettings`, `NoteDocument`) into `AccessNote.Core`.
     - Moved persistence infrastructure (`NoteStorage`, `SettingsStorage`) into `AccessNote.Core` and relocated SQLite package dependency to core.
     - Created `src/AccessNote.Shell/AccessNote.Shell.csproj` with internals visibility to `AccessNote` and tests.
     - Moved shell contracts/router/input/help-policy types into `AccessNote.Shell`:
       - `AppletContracts`, `AppletRegistry`, `MainMenuEntry`, `ScreenRouter`
       - `GlobalInputMap`, `MainMenuInputMap`, `NotesInputMap`, `InputCommandRouter`, `IgnoredCommandPolicy`
       - `HelpTextProvider`, `HintAnnouncementPolicy`
     - Updated project references so app and tests compile through the new project boundaries.
     - Verified with Windows test run (`28/28` passing).
     - Completed extraction into applet assemblies:
       - Moved Notes applet behavior and dialogs into `src/AccessNote.Applets.Notes`.
       - Moved Settings applet behavior and dialog into `src/AccessNote.Applets.Settings`.
       - Moved `ShellViewAdapter` into `src/AccessNote.Shell` so applet assemblies do not depend on `AccessNote` host.
       - Added `AccessNote` references to both applet projects and shell internals visibility for both.
     - Verified with Windows test run (`33/33` passing).
   - Scope: compile-time boundaries for shell/core/applets.
5. Extract cross-cutting interfaces into core contracts.
   - Status: Completed
   - Progress notes:
     - Added shared dialog contracts in `AccessNote.Core`:
       - `UnsavedChangesChoice`
       - `INotesDialogService`
       - `ISettingsDialogService`
     - Updated Notes/Settings orchestration (`NotesModule`, `NotesActionsCoordinator`, `RenameNoteAction`, `DeleteNoteAction`, `UnsavedNoteGuard`, `SettingsModule`, `SettingsActionsCoordinator`) to depend on contracts instead of concrete dialog services.
     - Added shared storage contracts in `AccessNote.Core`:
       - `INoteStorage`
       - `ISettingsStorage`
     - Updated sessions (`NotesSession`, `SettingsSession`) to depend on storage interfaces.
     - Kept navigation/announcement crossing points as delegate contracts to avoid over-abstracting the module API.
     - Verified with Windows test run (`33/33` passing).
   - Scope: navigation, announcement, storage, and dialog abstractions shared across applets.
6. Add architecture/dependency tests.
   - Status: Completed
   - Progress notes:
     - Added `tests/AccessNote.Tests/ArchitectureDependencyTests.cs`.
     - Enforced dependency boundaries:
       - `AccessNote.Core` does not depend on shell/host/applets.
       - `AccessNote.Shell` does not depend on host/applets.
       - Applet assemblies depend on core/shell and do not depend on host.
       - Host depends on core/shell/both applets.
     - Verified with Windows test run (`33/33` passing).
   - Scope: enforce layering so shell cannot directly depend on applet internals.
7. Add new-applet template/scaffold.
   - Status: Completed
   - Progress notes:
     - Added reusable scaffold templates under `templates/applet`:
       - `AccessNote.Applets.__APPLET__.csproj.template`
       - `AssemblyInfo.cs.template`
       - `__APPLET__Applet.cs.template`
      - `__APPLET__Module.cs.template`
      - `{{APPLET_NAME}}ArchitectureTests.cs.template`
     - Added `templates/applet/README.md` with integration steps and boundary rules.
     - Updated templates after shell contract simplification to use `AppletId` routing (no `AppScreen` dependency).
   - Scope: standardized starter for future applets + required test hooks.

## Shell Boundary Hardening (Current Pass)
Goal: finish moving shell orchestration out of host project and remove Notes/Settings-specific routing entry points.

1. Move shell orchestration classes into `AccessNote.Shell`.
   - Status: Completed
   - Progress notes:
     - Moved these classes from `src/AccessNote/Shell` to `src/AccessNote.Shell`:
       - `MainMenuModule`
       - `ShellInputController`
       - `ShellNavigationController`
       - `StartupFlowCoordinator`
       - `StartupHost`
       - `ExitFlowCoordinator`
       - `ExitHost`
       - `ShellStartupBinder`
       - `StatusAnnouncer`
     - Added `AccessNote.Core` project reference to `src/AccessNote.Shell/AccessNote.Shell.csproj`.
     - Verified with Windows test run (`33/33` passing).
2. Remove startup Notes-special-casing.
   - Status: Completed
   - Progress notes:
     - Changed startup host contract from `OpenNotesWorkspace()` to `OpenApplet(AppletId)`.
     - Updated startup flow to map start screen to applet id and route generically.
     - Updated `FlowFeatureFactory` and startup tests accordingly.
     - Verified with Windows test run (`33/33` passing).
3. Remove legacy router convenience methods.
   - Status: Completed
   - Progress notes:
     - Removed `OpenNotesWorkspace` and `OpenSettings` from `ScreenRouter` and `ShellNavigationController`.
     - Updated `ScreenRouterTests` to use `OpenApplet(AppletId)` only.
     - Removed unused `MainWindow.Navigation` wrappers.
     - Verified with Windows test run (`33/33` passing).
4. Move remaining shell dialog artifacts out of host.
   - Status: Completed
   - Progress notes:
     - Moved `ShellDialogService` from `src/AccessNote/Shell` to `src/AccessNote.Shell`.
     - Moved `ExitConfirmationDialog` (`.xaml` and `.xaml.cs`) to `src/AccessNote.Shell`.
     - Removed now-empty `src/AccessNote/Shell` folder from host project.
     - Verified with Windows test run (`33/33` passing).
5. Shift runtime routing/input checks to `AppletId`.
   - Status: Completed
   - Progress notes:
     - Added `ScreenRouter.ActiveAppletId` so shell and applets can query active applet directly.
     - Updated startup/help/event routing to use `AppletId?` checks instead of `AppScreen` equality.
     - Refactored `ShellInputController` to use applet input handler map keyed by `AppletId`.
     - Added assertions for `ActiveAppletId` transitions in `ScreenRouterTests`.
     - Verified with Windows test run (`33/33` passing).
6. Remove `AppScreen` contract from applet interface.
   - Status: Completed
   - Progress notes:
     - Removed `AppScreen` from `IApplet` and applet implementations.
     - Simplified `ScreenRouter` to track active state via `ActiveAppletId` only.
     - Updated applet registry and screen router tests accordingly.
     - Verified with Windows test run (`33/33` passing).
7. Make main-menu applet activation descriptor-driven.
   - Status: Completed
   - Progress notes:
     - Refactored `MainMenuEntry` to model applet entries via `AppletDescriptor` (`ForApplet`) instead of Notes/Settings-specific entry ids.
     - Updated `MainMenuModule` activation flow to open whichever applet id is attached to the selected entry.
     - Updated host/test menu entry construction to the new `MainMenuEntry` factories (`ForApplet`, `Utilities`, `Exit`).
     - Verified with Windows test run (`33/33` passing).

## Plugin Discovery Modularity (Current Pass)
Goal: make applet growth more modular by adding trusted startup discovery so new applet registrations can be added without changing host composition each time.

1. Add trusted plugin registration loader + allowlist validation.
   - Status: Completed
   - Progress notes:
     - Added `TrustedAppletRegistrationLoader` in `src/AccessNote.Shell/TrustedAppletRegistrationLoader.cs`.
     - Implemented allowlist parsing from `%LocalAppData%\\AccessNote\\applets\\allowlist.txt`.
     - Enforced filename-only `.dll` entries and skip-on-warning behavior.
     - Added warning aggregation for missing/invalid/unloadable plugin assemblies.
   - Scope:
     - `AccessNote.Shell` loader that reads `%LocalAppData%\\AccessNote\\applets\\allowlist.txt`
     - safe validation (filename-only `.dll` entries)
     - assembly/type discovery for `IAppletRegistration`
     - skip-on-failure behavior with warning collection
   - Done when:
     - invalid or missing plugin entries do not crash startup
     - successful entries return instantiated registrations
2. Wire loader into composition pipeline.
   - Status: Completed
   - Progress notes:
     - Updated `MainWindowCompositionRoot` to discover plugin registrations and append them to built-in registrations.
     - Added fallback path: if plugin composition fails, app starts with built-in applets only.
     - Added plugin warning diagnostics via `Trace.WriteLine` and one combined status announcement.
   - Scope:
     - append discovered registrations to built-in registrations in `MainWindowCompositionRoot`
     - add concise startup status announcement on discovery warnings
     - write detailed diagnostics to trace output
   - Done when:
     - registry includes built-in + discovered applets
     - warnings are visible without blocking startup
3. Add focused tests for discovery behavior.
   - Status: Completed
   - Progress notes:
     - Added `TrustedAppletRegistrationLoaderTests` covering:
       - missing allowlist file
       - invalid allowlist entry
       - missing assembly
       - successful discovery with duplicate allowlist dedupe
       - allowlisted assembly with no exported registrations
     - Verified with Windows test run (`55/55` passing).
   - Scope:
     - allowlist parsing/validation
     - successful discovery path
     - skip behavior for missing dll/invalid entry/ctor mismatch
   - Done when:
     - tests pass in the existing Windows test run
8. Add scaffold generator script for new applets.
   - Status: Completed
   - Progress notes:
     - Added `scripts/new_applet.sh` to generate `src/AccessNote.Applets.<Name>/` from `templates/applet/*.template`.
     - Added input validation for applet name format (PascalCase letters/digits) and safety checks for existing target directories.
     - Updated `templates/applet/README.md` with script-based workflow.
     - Verified script syntax (`bash -n`) and Windows test run (`33/33` passing).

## Modularity Next Wave (Current)
Goal: remove remaining hardcoded applet assumptions so future applets can be added with minimal host changes.

1. Make shell fully applet-metadata driven (menu/start/help).
   - Status: Completed
   - Progress notes:
     - Expanded `AppletDescriptor` to include `ScreenHintText`, `HelpText`, and optional `StartScreenOption`.
     - Updated Notes/Settings applets to provide their own descriptor metadata.
     - Updated `AppletRegistry` with descriptor lookup/order/start-screen resolution APIs.
     - Added `MainMenuEntryBuilder` so applet menu items are built from registered applet descriptors.
     - Updated startup/help wiring to resolve applet metadata from `AppletRegistry` instead of hardcoded Notes/Settings mapping.
     - Added/updated tests:
       - `AppletRegistryTests` (start-screen and descriptor ordering)
       - `MainMenuEntryBuilderTests`
     - Verified with Windows test run (`36/36` passing).
2. Let applets self-register with shell.
   - Status: Completed
   - Progress notes:
     - Added shell registration contracts/composer:
       - `IAppletRegistration`
       - `AppletRegistrationContext`
       - `AppletRegistrationComposer`
     - Added applet-owned registrations:
       - `NotesAppletRegistration` in `AccessNote.Applets.Notes`
       - `SettingsAppletRegistration` in `AccessNote.Applets.Settings`
     - Updated composition root to build applet registry from registrations instead of host-side applet factory branching.
     - Removed now-obsolete host file `Composition/AppletFeatureFactory.cs`.
     - Added `AppletRegistrationComposerTests`.
     - Verified with Windows test run (`39/39` passing).
   - Scope: move applet registration ownership into applet projects and reduce host composition branching.
3. Move applet input handling behind applet contracts.
   - Status: Completed
   - Progress notes:
     - Extended `IApplet` with input handler contract:
       - `bool HandleInput(KeyEventArgs e, Key key, ModifierKeys modifiers)`
     - Implemented applet-owned input handling in `NotesApplet` and `SettingsApplet`.
     - Added `ScreenRouter.HandleInputForActiveApplet(...)` and updated shell input flow to delegate through active applet.
     - Simplified `ShellInputController` and `InputFeatureFactory` by removing shell-level applet-id input map.
     - Added router input delegation coverage in `ScreenRouterTests`.
     - Verified with Windows test run (`39/39` passing).
   - Scope: avoid shell-level per-applet input dispatch maps when adding applets.
4. Split applet UI into applet-owned `UserControl`s.
   - Status: Completed
   - Progress notes:
     - Created `NotesScreenView` UserControl in `AccessNote.Applets.Notes` with Notes XAML and event delegate properties.
     - Created `SettingsScreenView` UserControl in `AccessNote.Applets.Settings` with Settings XAML and event delegate properties.
     - Replaced ~250 lines of inline Notes/Settings XAML in `MainWindow.xaml` with two xmlns-referenced UserControl tags.
     - Updated `MainWindow.Composition.cs` to get controls via UserControl properties.
     - Wired UserControl event delegates to controllers in `MainWindow.xaml.cs` constructor.
     - Removed event handlers from `MainWindow.Notes.cs` and `MainWindow.Navigation.cs`.
     - Removed now-empty `MainWindow.SettingsMenu.cs`.
     - Both UserControls use `x:ClassModifier="internal"` for proper encapsulation.
     - Verified with Windows test run (`41/41` passing).
   - Scope: move Notes/Settings screen XAML blocks out of `MainWindow.xaml` into applet assemblies.
5. Harden boundaries (`InternalsVisibleTo`, surface area minimization).
   - Status: Completed
   - Progress notes:
     - Made applet-facing Shell types public (`AppletId`, `AppletDescriptor`, `IApplet`, `IAppletRegistration`, `AppletRegistrationContext`, `ShellViewAdapter`).
     - Narrowed `ShellViewAdapter` surface: constructor, `ShowMainMenuScreen`, `MainMenuSelectedIndex`, `SetMainMenuSelection` are `internal`.
     - Made `ExitConfirmationDialog` `internal` (added `x:ClassModifier="internal"` to XAML).
     - Removed `InternalsVisibleTo` grants to both applet assemblies from `Shell/AssemblyInfo.cs`.
     - Moved `NotesInputMap.cs` (Notes-specific input types) from Shell into Notes applet; removed dead `InputCommandRouter.TryGetNotesCommand`.
     - Verified with Windows test run (`41/41` passing).
   - Scope: reduce unnecessary friend access and narrow cross-project APIs.
6. Strengthen architecture tests for new boundaries.
   - Status: Completed
   - Progress notes:
     - Added `ShellAssembly_GrantsInternalsOnlyToHostAndTests` — validates Shell `InternalsVisibleTo` excludes applets.
     - Added `ShellAssembly_PublicSurfaceIsLimitedToAppletContracts` — asserts exactly 6 public types in Shell assembly.
     - Verified with Windows test run (`41/41` passing).
   - Scope: add tests for host/composition-only dependencies and applet isolation rules.
   - Scope: add tests for host/composition-only dependencies and applet isolation rules.
7. Upgrade `scripts/new_applet.sh` with optional auto-wire mode.
   - Status: Completed
   - Progress notes:
     - Added `--wire` flag to scaffold script.
     - Auto-wire handles: `AppletId` enum member insertion (with trailing comma fix), host project reference, architecture test stub copy, and TODO comment in composition root.
     - Each wiring step is idempotent-safe (checks for existing content before modifying).
     - Updated `templates/applet/README.md` to document `--wire` as the recommended workflow.
     - Verified with dry-run test (`TestApplet`), then reverted test artifacts.
     - Verified with Windows test run (`41/41` passing).
   - Scope: optional scaffold+wire path for project reference, enum extension, composition registration, and test stubs.

## Worklist Accessibility Conformance (Phase 1)
Goal: complete accessibility-first modular conformance for current applets before feature expansion work.

1. Define Accessibility Contract v1 and shared guardrails.
   - Status: Completed
   - Progress notes:
     - Added `docs/plans/2026-02-18-accessibility-contract-v1.md`.
     - Documented message style, announce-vs-native-speech guidance, one-call announcement rule, and `F1` help contract.
     - Added `tests/AccessNote.Tests/AccessibilityContractTests.cs` to enforce contract doc presence and prevent placeholder applet help text.
   - Scope:
     - add `docs/plans/2026-02-18-accessibility-contract-v1.md`
     - define message style, announce-vs-native-speech rules, one-call announcement rule
     - define minimum `F1` help/hint expectations
   - Done when:
     - contract doc is approved and referenced by applet/tests
2. Add minimal shared accessibility policies and validators.
   - Status: Completed
   - Progress notes:
     - Added `src/AccessNote.Shell/AnnouncementTextPolicy.cs` for message normalization and whitespace collapsing.
     - Updated `StatusAnnouncer` to normalize announcement text through `AnnouncementTextPolicy`.
     - Added reusable test validator `tests/AccessNote.Tests/HelpTextContractValidator.cs`.
     - Strengthened `AccessibilityContractTests` to parse and validate applet `helpText` and `screenHintText` entries consistently.
   - Scope:
     - add announcement text normalization/dedupe helper
     - add help/hint contract validation test helper
   - Done when:
     - shared helpers exist without adding heavy framework complexity
3. Conform Media Player accessibility behavior.
   - Status: Completed
   - Progress notes:
     - Added `MediaPlayerAnnouncementText` helper for consistent announcement wording.
     - Updated `MediaPlayerModule` to:
       - announce actionable message when no tracks are loaded
       - announce playback state with current track context
       - announce track changes consistently for next/previous and auto-advance
       - announce track additions after file/stream add flows
     - Added `MediaPlayerAnnouncementTextTests`.
     - Verified with Windows test run (`62/62` passing).
   - Scope:
     - track change announcements
     - playback state announcements (play/pause/stop)
   - Done when:
     - automated tests pass and NVDA behavior matches contract
4. Conform MIDI Player accessibility behavior.
   - Status: Completed
   - Progress notes:
     - Added `MidiPlayerAnnouncementText` helper for consistent playback/file-load announcement wording.
     - Updated `MidiPlayerModule` to:
       - announce actionable message when no file is loaded
       - announce playback state for play/pause/stop actions
       - announce MIDI file load success/failure
       - announce playback completion when track reaches end
     - Refined stop behavior:
       - key `S` now stops playback without tearing down the periodic UI timer
       - full module `Stop()` still performs timer teardown for screen exit
     - Added `MidiPlayerAnnouncementTextTests`.
     - Verified with Windows test run (`65/65` passing).
   - Scope:
     - file load announcements
     - playback state announcements
   - Done when:
     - automated tests pass and NVDA behavior matches contract
5. Conform App Launcher accessibility behavior.
   - Status: Completed
   - Progress notes:
     - Added `AppLauncherAnnouncementText` helper for mode/navigation announcement wording.
     - Updated `AppLauncherModule` to use contract-aligned announcements for:
       - screen entry mode summary
       - mode toggle changes (Favorites/Browse)
       - browse-directory navigation (enter folder / navigate up)
     - Added `AppLauncherAnnouncementTextTests`.
     - Verified with Windows test run (`68/68` passing).
   - Scope:
     - mode switch announcements
     - navigation feedback announcements
   - Done when:
     - automated tests pass and NVDA behavior matches contract
6. Conform Calendar, Notes, and Contacts accessibility behavior.
   - Status: Completed
   - Progress notes:
     - Added focused announcement helpers:
       - `CalendarAnnouncementText`
       - `NotesAnnouncementText`
       - `ContactsAnnouncementText`
     - Calendar updates:
       - announce events-list context when moving from grid to events
       - announce selected event on Enter
       - announce event created/deleted outcomes
     - Notes updates:
       - announce focus region when cycling between list/editor with `F6`
     - Contacts updates:
       - announce focus region transitions when cycling focus with `F6`
     - Added tests:
       - `CalendarAnnouncementTextTests`
       - `NotesAnnouncementTextTests`
       - `ContactsAnnouncementTextTests`
     - Verified with Windows test run (`75/75` passing).
   - Scope:
     - Calendar navigation announcement review/fix
     - Notes editing accessibility review/fix
     - Contacts list-navigation announcements (no feature expansion)
   - Done when:
     - automated tests pass and NVDA behavior matches contract
7. Add cross-screen-reader checklist and complete Phase 1 validation.
   - Status: Completed
   - Progress notes:
     - Added checklist and expected speech matrix:
       - `docs/plans/2026-02-18-screen-reader-checklist-phase1.md`
     - Completed NVDA log-verified validation run across target applets using scripted keyboard flows and NVDA `Speaking [...]` evidence.
     - Captured JAWS observation as blocked on this environment (not installed).
     - Captured Narrator runtime smoke observation and documented process-management limitation in this environment.
     - Captured Phase 2 backlog items in checklist doc (JAWS machine validation + Narrator transcript/parity follow-up).
   - Scope:
     - add JAWS/Narrator checklist doc and expected speech matrix
     - run NVDA manual validation pass for all updated applets
   - Done when:
     - NVDA pass complete, checklist published, and Phase 2 backlog captured

## AppLauncher Parity and Feedback (Current Pass)
Goal: align App Launcher favorites interaction with the `access-menu` pattern and improve spoken feedback outcomes.

1. Add AppLauncher dialog service and add-favorite dialog.
   - Status: Completed
   - Progress notes:
     - Added `IAppLauncherDialogService` and add-favorite payload contracts in `AppLauncherDialogContracts.cs`.
     - Added `AppLauncherDialogService` to own add-flow dialog orchestration.
     - Added `AddFavoriteDialog` (two source choices: current selection or file browse).
   - Scope:
     - add `IAppLauncherDialogService` and concrete `AppLauncherDialogService`
     - add `AddFavoriteDialog` with two sources:
       - use current browse selection
       - browse filesystem
   - Done when:
     - dialog returns a validated add-favorite result payload
2. Wire existing AppLauncher buttons to module handlers.
   - Status: Completed
   - Progress notes:
     - Wired `Add`, `Remove`, and `Launch` button clicks in `AppLauncherModule` with attach/detach lifecycle handling.
     - Updated `AppLauncherApplet.Enter(...)` to pass existing screen buttons to the module.
   - Scope:
     - wire `Add`, `Remove`, and `Launch` button click paths
     - keep keyboard shortcuts unchanged
   - Done when:
     - button actions match keyboard behavior
3. Implement two-source add-to-favorites flow in module.
   - Status: Completed
   - Progress notes:
     - Implemented add request building from current browse selection when valid.
     - Added browse fallback path and duplicate-by-path detection.
     - Added deterministic favorites refresh/selection behavior without breaking browse mode.
   - Scope:
     - current selection source (browse mode, launchable file only)
     - browse source fallback
     - duplicate detection and list refresh
   - Done when:
     - add flow supports both sources with deterministic outcomes
4. Expand AppLauncher spoken feedback contract.
   - Status: Completed
   - Progress notes:
     - Expanded `AppLauncherAnnouncementText` with add/remove/launch/validation messages.
     - Updated module outcomes to use single-call announcement helpers for success and validation failures.
   - Scope:
     - standard outcome announcements for add/remove/launch
     - validation/error messages for invalid add source and failures
     - single-call outcome announcements
   - Done when:
     - spoken feedback is consistent and test-backed
5. Add focused tests and validate.
   - Status: Completed
   - Progress notes:
     - Added `AppLauncherModuleTests` for button-driven add/remove flows and duplicate/no-selection outcomes.
     - Extended `AppLauncherAnnouncementTextTests` for new standardized messages.
     - Verified with Windows test run (`83/83` passing).
   - Scope:
     - announcement text tests
     - add-source decision tests
     - module flow tests for add/remove button paths
   - Done when:
     - Windows test suite passes and behavior matches design

## AppLauncher Start Menu Hybrid Browse (Next Pass)
Goal: replace filesystem browse mode with a modular Start Menu-first catalog and add Steam/Xbox/Heroic coverage while preserving accessibility and maintainability.

1. Add browse catalog contracts and core model.
   - Status: Completed
   - Progress notes:
     - Added browse contracts/model in AppLauncher applet:
       - `LaunchSpec`, `LaunchTargetType`, `AppBrowseSourceEntry`, `IAppBrowseSource`
       - `AppBrowseNode`, `AppBrowseCatalogBuilder`, `AppBrowseNavigator`
     - Added launch/favorite token infrastructure:
       - `AppLauncherProcessLauncher`
       - `AppLauncherFavoriteLaunchCodec`
       - `AppBrowseSourceFactory`
   - Scope:
     - add `IAppBrowseSource` and browse item model types
     - add `LaunchSpec` and target-type abstractions
   - Done when:
     - browse sources can return normalized app records through one contract
2. Implement source adapters.
   - Status: Completed
   - Progress notes:
     - Added `StartMenuBrowseSource`:
       - scans both Common and user Start Menu program roots
       - includes `.lnk`, `.exe`, `.bat`
       - applies root-level duplicate suppression when app name exists in subfolders
     - Added `SteamBrowseSource`:
       - includes Steam launcher
       - parses Steam libraries and `appmanifest_*.acf` for installed games
     - Added `XboxBrowseSource`:
       - reads installed packages from `Get-AppxPackage`
       - inspects package manifest applications and builds shell-app launch ids
     - Added `HeroicBrowseSource`:
       - includes Heroic launcher
       - parses Heroic `installed.json` metadata and maps games to executable/URI targets
   - Scope:
     - `StartMenuBrowseSource` (ProgramData + user Start Menu)
     - `SteamBrowseSource` (launcher + installed games)
     - `XboxBrowseSource` (installed package inventory)
     - `HeroicBrowseSource` (launcher + installed games)
   - Done when:
     - each source returns deterministic records and silently skips unavailable environments
3. Build merged browse catalog and navigator.
   - Status: Completed
   - Progress notes:
     - Implemented merged catalog/dedupe/sort flow in `AppBrowseCatalogBuilder`.
     - Implemented folder enter/up/current-entry navigation in `AppBrowseNavigator`.
     - Added tests for merge-dedupe and folder/app ordering in `AppBrowseCatalogBuilderTests`.
     - Added tests for navigation enter/up behavior in `AppBrowseNavigatorTests`.
   - Scope:
     - add `AppBrowseCatalogBuilder` merge/dedupe/sort behavior
     - add `AppBrowseNavigator` enter/up/current-folder mechanics
   - Done when:
     - module can render and navigate virtual browse tree without filesystem traversal
4. Integrate catalog layer into AppLauncher module.
   - Status: Completed
   - Progress notes:
     - Refactored `AppLauncherModule` browse mode to consume `AppBrowseNavigator` entries.
     - Removed filesystem directory traversal browse model from module.
     - Preserved keyboard interaction model (`Tab`, `Enter`, `Backspace`) with virtual tree navigation.
     - Kept browse/favorites mode announcements aligned with existing contract.
   - Scope:
     - replace `LoadBrowseEntries` filesystem logic with navigator-backed entries
     - keep existing `Tab`/`Enter`/`Backspace` behavior
     - keep spoken feedback contract intact
   - Done when:
     - browse mode shows Start Menu/Steam/Xbox/Heroic virtual tree and remains fully keyboard accessible
5. Extend launch + favorites for non-path targets.
   - Status: Completed
   - Progress notes:
     - Added launch dispatch for direct path, shell app model id, and URI targets.
     - Added favorite token encoding/decoding for non-path targets (`shellapp:`, `uri:`) with backward-compatible plain path support.
     - Updated add-to-favorites and duplicate detection to use normalized launch identity keys.
     - Updated favorite launch flow to resolve stored token/path into `LaunchSpec`.
     - Added `AppLauncherFavoriteLaunchCodecTests`.
   - Scope:
     - add launch dispatcher for `DirectPath`, `ShellApp`, `CustomCommand`
     - support favorite persistence and duplicate checks for launch tokens
   - Done when:
     - platform entries can launch and be favorited without breaking existing favorites
6. Add focused tests and validate on Windows.
   - Status: Completed
   - Progress notes:
     - Added tests:
       - `AppBrowseCatalogBuilderTests`
       - `AppBrowseNavigatorTests`
       - `AppLauncherFavoriteLaunchCodecTests`
     - Verified with Windows test run (`90/90` passing).
     - Existing AppLauncher regression tests remain passing.
   - Scope:
     - catalog/navigator/source parsing/favorites integration tests
     - regression tests for existing AppLauncher behaviors
   - Done when:
     - Windows test suite passes and manual NVDA browse-mode validation is clean

## AppLauncher Game Title Quality (Current Pass)
Goal: keep `Steam`, `Xbox`, and `Heroic` sources while ensuring Games submenu entries announce real game titles.

1. Improve Heroic title fallback behavior.
   - Status: Completed
   - Progress notes:
     - Added title normalization fallback to prefer install-folder names when Heroic metadata title fields are opaque IDs.
   - Scope:
     - `HeroicBrowseSource` title resolution only (no source removal)
   - Done when:
     - Heroic games announce readable names (for example install-folder game titles) in Games submenu
2. Improve Xbox title resolution behavior.
   - Status: Completed
   - Progress notes:
     - Added Start Apps name resolution path to prefer real app/game names over manifest resource keys.
   - Scope:
     - `XboxBrowseSource` display-name resolution only
   - Done when:
     - Xbox games/apps in Games submenu announce Start Apps titles when available
3. Add regression tests and validate.
   - Status: Completed
   - Progress notes:
     - Added focused unit tests for Heroic and Xbox title resolution behavior.
     - Verified with Windows test run (`99/99` passing).
   - Scope:
     - parser/title fallback tests only
   - Done when:
     - Windows test suite passes with the new title-resolution tests

## AppLauncher 10x Do-Now Pass (Current Pass)
Goal: execute the 10x quick-win batch for browse performance, title trust, and browse productivity.

1. Add browse catalog cache + background refresh.
   - Status: Completed
   - Progress notes:
     - Added in-module browse root cache reuse on browse-mode entry for faster initial render.
     - Added background catalog refresh with UI-thread apply and path/selection restore.
   - Scope:
     - cache most recent browse catalog root
     - refresh in background without blocking input
   - Done when:
     - browse mode can show cached entries immediately and refresh asynchronously
2. Add title confidence/provenance surfacing.
   - Status: Completed
   - Progress notes:
     - Added `TitleConfidence` and `AppBrowseTitleMetadata` to browse source contract flow.
     - Threaded source label + title metadata through browse nodes.
     - Added detail rendering in App Launcher to include source and title confidence/provenance.
   - Scope:
     - source adapters provide metadata
     - details panel surfaces metadata for current browse app
   - Done when:
     - browse app details show source and confidence/provenance text
3. Add instant filter and one-key browse favorite.
   - Status: Completed
   - Progress notes:
     - Added type-to-filter behavior in browse mode with spoken count feedback.
     - Added one-key quick favorite in browse mode via `Insert`.
     - Added help text update for new shortcuts.
   - Scope:
     - inline browse filtering
     - quick add current browse selection to favorites
   - Done when:
     - user can filter by typing and favorite current browse app without dialog flow
4. Add focused tests and validate.
   - Status: Completed
   - Progress notes:
     - Added tests for:
       - browse filter behavior
       - quick favorite from browse
       - browse details source/title confidence
       - filter announcement text
       - navigator path restore
     - Verified with Windows test run (`105/105` passing).
   - Scope:
     - AppLauncher module announcements/filtering/shortcut behavior
     - browse navigator restore path behavior
   - Done when:
     - test suite passes with coverage for new quick-win behaviors

## AppLauncher Unified Games Superfolder (Current Pass)
Goal: add a source-agnostic top-level Games submenu while preserving per-source browse folders.

1. Extend browse entry contract for explicit game classification.
   - Status: Completed
   - Progress notes:
     - Added `IsGame` flag to `AppBrowseSourceEntry`.
     - Marked Steam/Heroic/Xbox game entries with `IsGame = true`.
   - Scope:
     - browse source contract update only
     - source adapters annotate game records
   - Done when:
     - catalog can identify game entries explicitly without relying only on category path
2. Build top-level unified `Games` folder in catalog builder.
   - Status: Completed
   - Progress notes:
     - Added root-level `Games` folder materialization in `AppBrowseCatalogBuilder`.
     - Preserved original source folder trees while duplicating game launch entries into unified `Games`.
     - Kept global launch-identity dedupe across sources.
   - Scope:
     - catalog builder only
     - no routing/input changes required in module
   - Done when:
     - browse root includes `Games` with merged game titles and source folders remain intact
3. Add tests and validate behavior end-to-end.
   - Status: Completed
   - Progress notes:
     - Added catalog tests for:
       - cross-source unified games aggregation
       - explicit `IsGame` flag when category path is empty
     - Added module test for keyboard entry into root `Games` folder and aggregated title rendering.
     - Verified with Windows test run (`108/108` passing).
   - Scope:
     - catalog and module tests for unified games experience
   - Done when:
     - test suite passes and unified games navigation is regression-covered

## AppLauncher Xbox Game Noise Filter (Current Pass)
Goal: keep real Xbox games discoverable while removing Xbox infrastructure/service entries from game lists.

1. Add Xbox infrastructure suppression rules.
   - Status: Completed
   - Progress notes:
     - Added infrastructure suppression for known helper/service apps (`Gaming Services`, `XboxIdentityProvider`, `XboxGameCallableUI`, `XboxSpeechToTextOverlay`, `Xbox TCUI`).
   - Scope:
     - `XboxBrowseSource` filtering only
   - Done when:
     - infrastructure apps no longer appear as game entries
2. Improve Xbox game package detection.
   - Status: Completed
   - Progress notes:
     - Extended package qualification to include install-location heuristics (`XboxGames`, `MSIXVC`) in addition to existing gaming-token/dependency checks.
   - Scope:
     - `XboxBrowseSource` package qualification only
   - Done when:
     - Xbox game packages using Xbox install roots are included more reliably
3. Add focused tests and validate.
   - Status: Completed
   - Progress notes:
     - Added tests verifying suppression of `Gaming Services` and non-suppression of real game titles.
     - Verified with Windows test run (`110/110` passing).
   - Scope:
     - Xbox source filtering tests only
   - Done when:
     - test suite passes with suppression behavior covered

## AppLauncher Xbox StartApps Qualification (Current Pass)
Goal: include tokenless Xbox game packages (for example Forza family IDs) in Xbox discovery using StartApps package matching.

1. Expand Xbox package qualification with StartApps package-family matching.
   - Status: Completed
   - Progress notes:
     - Added package qualification based on `Get-StartApps` AppID prefix matching (`<PackageFamilyName>!`).
     - This covers game packages without explicit `xbox/gaming/gamepass` name tokens.
   - Scope:
     - `XboxBrowseSource` qualification only
   - Done when:
     - tokenless Xbox game packages are no longer excluded by initial qualification
2. Add focused regression test and validate.
   - Status: Completed
   - Progress notes:
     - Added `XboxBrowseSourceTests.ResolveDisplayName_UsesStartAppsMatch_ForTokenlessPackage`.
     - Verified with Windows test run (`111/111` passing).
   - Scope:
     - Xbox source test coverage for tokenless package matching scenario
   - Done when:
     - tests pass and qualification logic is regression-covered

## AppLauncher Xbox Submenu Cleanup (Current Pass)
Goal: reduce Xbox submenu noise by removing utility/internal endpoints while keeping real game discovery and primary launcher entries.

1. Filter non-game Xbox submenu items to user-visible StartApps only.
   - Status: Completed
   - Progress notes:
     - Added exact StartApps gate for non-game entries to avoid internal package endpoints and duplicate launcher aliases.
   - Scope:
     - `XboxBrowseSource` non-game item filtering only
   - Done when:
     - Xbox submenu no longer includes internal duplicate app entries
2. Suppress additional Xbox utility entries.
   - Status: Completed
   - Progress notes:
     - Added suppression tokens for `Game Bar` / `XboxGamingOverlay` and `Xbox Series X` helper entries.
   - Scope:
     - infrastructure suppression list only
   - Done when:
     - Xbox submenu keeps focused launcher/game navigation and drops utility noise
3. Add regression test and validate.
   - Status: Completed
   - Progress notes:
     - Added test for `Game Bar` suppression.
     - Verified with Windows test run (`112/112` passing).
   - Scope:
     - Xbox suppression test coverage
   - Done when:
     - test suite passes with new submenu cleanup behavior covered

## AppLauncher Xbox Games Folder Decontamination (Current Pass)
Goal: prevent non-game/system packages from leaking into `Xbox -> [Games]` while preserving tokenless Xbox game packages (for example Forza family IDs).

1. Narrow tokenless package fallback to likely Store game package names.
   - Status: Completed
   - Progress notes:
     - Restricted tokenless StartApps package-family qualification to packages matching `Microsoft.<hex-id>` shape.
     - Prevents broad inclusion of unrelated system packages with generic StartApps entries.
   - Scope:
     - `XboxBrowseSource` package qualification only
   - Done when:
     - `Xbox -> [Games]` no longer fills with non-game `Global.*`/system entries
2. Add focused tests and validate.
   - Status: Completed
   - Progress notes:
     - Added tests for package-name shape classification (`true` for Forza-style, `false` for system package names).
     - Verified with Windows test run (`114/114` passing).
   - Scope:
     - Xbox source qualification tests only
   - Done when:
     - regression tests cover new qualification guardrail

## AppLauncher Xbox Defender Suppression (Current Pass)
Goal: remove Windows Defender from `Xbox -> [Games]` false positives.

1. Add Defender suppression token rules.
   - Status: Completed
   - Progress notes:
     - Added suppression tokens for `Microsoft Defender` and `SecHealthUI` in Xbox infrastructure filtering.
   - Scope:
     - Xbox suppression list only
   - Done when:
     - Defender entries are excluded from Xbox game lists
2. Add test and validate.
   - Status: Completed
   - Progress notes:
     - Added regression test: `ShouldSuppressInfrastructureApp_ReturnsTrue_ForMicrosoftDefender`.
     - Verified with Windows test run (`115/115` passing).
   - Scope:
     - Xbox suppression test coverage only
   - Done when:
     - tests pass with Defender suppression covered

## AppLauncher Xbox Rule Table Refactor (Current Pass)
Goal: make Xbox filtering rules easier to maintain by centralizing token-based heuristics in configuration-like tables.

1. Centralize Xbox filtering tokens.
   - Status: Completed
   - Progress notes:
     - Replaced scattered inline string checks with centralized token tables for:
       - gaming package detection
       - install-path markers
       - infrastructure suppression
   - Scope:
     - `XboxBrowseSource` maintainability refactor only
   - Done when:
     - rule values can be updated in one place without touching predicate logic
2. Consolidate predicate logic on shared matcher.
   - Status: Completed
   - Progress notes:
     - Added shared token-matching helper and routed `ContainsGamingToken`, `ContainsXboxToken`, and infrastructure checks through it.
     - Kept behavior equivalent to the current validated state.
   - Scope:
     - internal helper refactor only
   - Done when:
     - behavior remains unchanged and tests stay green
3. Validate no regression.
   - Status: Completed
   - Progress notes:
     - Verified with Windows test run (`115/115` passing).
   - Scope:
     - full regression run
   - Done when:
     - refactor ships with no behavioral regressions
