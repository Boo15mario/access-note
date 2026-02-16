# Applet Scaffold Template

Use these templates when adding a new applet so the solution keeps the same modular boundaries used by `Notes` and `Settings`.

## Usage
Option A (recommended): run scaffold script with auto-wire from repo root:

```bash
scripts/new_applet.sh --wire Utilities
```

This scaffolds the project files **and** wires the applet into the solution:
- Adds `AppletId` enum member
- Adds host project reference
- Copies architecture test stub
- Adds TODO comment in composition root

Option B: scaffold only (manual wiring):

```bash
scripts/new_applet.sh Utilities
```

Then complete these manual steps:

1. Add enum member to `AppletId` in `src/AccessNote.Shell/AppletContracts.cs`.
2. Add project reference in `src/AccessNote/AccessNote.csproj`.
3. Register applet in composition (`MainWindowCompositionRoot` / factories).
4. Copy architecture test from templates to `tests/AccessNote.Tests/`.

## Required Boundary Rules
- Applet projects may reference only:
  - `AccessNote.Core`
  - `AccessNote.Shell`
- Applet projects must not reference `AccessNote` host project.
- Host `AccessNote` may reference applet projects.

## Verification
Run:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' test 'F:\git\access-note\tests\AccessNote.Tests\AccessNote.Tests.csproj'
```

New applet changes are complete only when dependency tests and applet tests pass.
