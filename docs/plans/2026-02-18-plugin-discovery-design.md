# Plugin Discovery Design (2026-02-18)

## Goal
Support modular applet growth without editing host composition for every new applet by adding trusted plugin discovery at startup.

## Decisions Locked
- Extensibility mode: dynamic startup discovery from a local plugin folder.
- Trust model: first-party trusted plugins only.
- Trust enforcement: explicit allowlist manifest.
- Plugin location: `%LocalAppData%\AccessNote\applets\`.
- Failure behavior: skip bad plugin, continue startup, emit clear diagnostics.

## Scope for This Pass
- Keep existing built-in applet registrations unchanged.
- Add a plugin registration loader that discovers additional `IAppletRegistration` types from allowlisted assemblies.
- Integrate loader into composition so built-in + discovered registrations flow through existing `AppletRegistrationComposer`.

## Loader Contract
- Input:
  - `pluginDirectoryPath`
  - `allowlistManifestPath`
- Output:
  - `Registrations` (`IReadOnlyList<IAppletRegistration>`)
  - `Warnings` (`IReadOnlyList<string>`)

## Allowlist Format
- File: `allowlist.txt` in plugin folder.
- One assembly filename per line, for example:
  - `AccessNote.Applets.Weather.dll`
  - `AccessNote.Applets.Tasks.dll`
- Rules:
  - blank lines are ignored
  - `#` starts a comment line
  - entries must be bare filenames (no path separators, no rooted/absolute paths)
  - entries must end in `.dll`

## Discovery Rules
For each allowlisted assembly file:
1. Build full path under plugin folder.
2. If missing, record warning and continue.
3. Load assembly.
4. Find exported, concrete types implementing `IAppletRegistration`.
5. For each registration type:
   - require a public parameterless constructor
   - instantiate and add to discovered registrations
   - on failure, record warning and continue

## Integration Path
1. Compose built-in registrations as today.
2. Run plugin loader.
3. Append discovered registrations.
4. Build registry with `AppletRegistrationComposer.CreateRegistry`.
5. On warnings:
   - write detailed diagnostics to trace output
   - announce one combined status message (single `Announce` call)

## Diagnostics Policy
- Never fail app startup due plugin load issues.
- Keep user-facing status concise.
- Keep technical details in diagnostics/trace lines.

## Test Plan
- Unit-test allowlist parsing and validation.
- Unit-test discovery success from a copied test assembly.
- Unit-test skip behavior for:
  - missing assembly
  - invalid allowlist entry
  - registration type without public parameterless constructor
- Unit-test deterministic registration ordering from allowlist order.

## Deferred (Not in This Pass)
- Signature/hash verification.
- Runtime unload/reload.
- Version compatibility negotiation.
- Sandboxed third-party plugin execution.
