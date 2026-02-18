# Accessibility Contract Phase 1 Design (2026-02-18)

## Goal
Deliver an accessibility-first, modular conformance pass across remaining `worklist.md` items by defining a shared contract first, then applying it applet-by-applet.

## Scope Decisions (Approved)
- Scope option: prioritized modular plan across all remaining `worklist.md` items.
- Phase 1 success model: accessibility-first conformance before feature expansion.
- Screen reader scope: NVDA fixes + create JAWS/Narrator checklist now.
- Contacts scope in Phase 1: accessibility/help consistency only (no feature expansion).
- Execution style: contract-first, then applet conformance.

## Architecture and Boundaries
1. Add an Accessibility Contract v1 as the source of truth for announcement and help behavior.
2. Keep behavior changes inside applet projects and shell helpers; avoid adding behavior logic to `MainWindow`.
3. Keep shared infrastructure minimal:
   - announcement text normalization/dedupe policy
   - contract validation helpers for tests
4. Keep applet-specific event-to-message mapping local to each applet module/coordinator.
5. Explicitly defer Contacts feature expansion (`full management`, `search/filter`) to Phase 2.

## Component Model and Data Flow
1. Contract source:
   - `docs/plans/2026-02-18-accessibility-contract-v1.md` (to be authored in implementation phase)
   - defines message style, announce-vs-native-speech rules, and single-call announcement rule
2. Shared primitives (small, reusable):
   - `AnnouncementTextPolicy`: normalize + optional duplicate suppression window
   - `HelpTextContractValidator`: tests for non-empty/helpful applet descriptor/help metadata
3. Applet-local conformance:
   - each applet maps key events to one contract-compliant announcement
   - flow: `input -> action -> outcome -> one announce call`
4. Help text flow:
   - F1 remains shell-routed through `HelpTextProvider`
   - applet descriptor help/hint content validated by tests

## Error Handling and Quality Gates
1. On applet action failures, announce one concise failure message and keep technical details in logs.
2. Prevent rapid multi-announce collisions by combining outcome text into one call.
3. Add regression tests that fail on:
   - missing/empty applet help text
   - missing/empty hint text
   - missing high-priority announcement events in target applets
4. Manual gates:
   - NVDA pass required for each updated applet
   - JAWS/Narrator checklist documented (implementation deferred)

## Phase 1 Target Applets
- Media Player
- MIDI Player
- App Launcher
- Calendar
- Notes
- Contacts (announcement/help consistency only)

## Delivery Plan
### Phase 1A: Contract and Shared Guards
1. Write Accessibility Contract v1 document.
2. Add shared announcement/help validation helpers.
3. Add JAWS/Narrator checklist doc.

### Phase 1B: Highest-Impact Applets
1. Media Player announcement conformance.
2. MIDI Player announcement conformance.
3. App Launcher announcement conformance.

### Phase 1C: Remaining Applets
1. Calendar navigation announcement review/fix.
2. Notes editing accessibility review/fix.
3. Contacts list-navigation announcement conformance.

### Phase 1D: Verification and Closeout
1. NVDA manual validation pass for updated applets.
2. Capture unresolved items in Phase 2 backlog.

## Out of Scope (Phase 1)
- Full Contacts management expansion.
- Contacts search/filter feature expansion.
- JAWS/Narrator parity bug-fixing (checklist only in this phase).
- Large framework rewrite for accessibility.

## Risks and Mitigations
- Risk: inconsistent message wording across applets.
  - Mitigation: contract-first wording rules + applet conformance tests.
- Risk: accidental announcement suppression from rapid consecutive calls.
  - Mitigation: single-call outcome policy and helper usage.
- Risk: scope creep into feature work.
  - Mitigation: strict Phase 1 scope guard and explicit deferred backlog.
