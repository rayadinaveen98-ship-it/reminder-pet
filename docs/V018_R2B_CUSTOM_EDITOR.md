# PetPing v0.18 — Custom Repeat Editor (draft, not released)

**Stack:** PR #21 recurrence calculation → PR #22 versioned persistence → this PR custom WPF editor.
**Release status:** source prototype only. Current user-test installer remains the unsigned v0.17 preview.

## Functional design
- Preserve the simple familiar Reminders page and One time / Every day / Every week choices.
- Add a fourth repeat choice: **Custom schedule**, opening an inline panel rather than replacing the original editor.
- Two custom modes:
  - **On selected weekdays**, with Monday–Sunday checkboxes and every 1–52 weeks.
  - **Every few days**, every 1–365 days.
- Date becomes **Start date** in custom mode; time is the intended local wall clock. A text preview shows the computed next actual occurrence. The custom options use a single shared domain calculator and validated v1 schema.
- Keep Quick +1min only for standard schedules; never silently turn a custom schedule into One time.
- Edit an upcoming *unsent* custom reminder via the same form. Historical occurrences cannot be edited, and the user cannot silently switch between custom and regular schedule kinds. Previous history and SeriesId are preserved.
- Save returns success only after the store successfully writes. Errors remain on the screen without discarding current form values.
- New first-time custom creation writes `reminders.pre-custom-v017.json` in local PetPing data storage; this validated, explicit legacy-readable snapshot is never overwritten. If the snapshot cannot be created or validated, custom creation fails rather than risking a misleading downgrade.

## Security and data compatibility
- No cloud, network account, medical advice or reminders outside the local store.
- **Downgrade constraint:** v0.17 does not understand `RepeatPattern.Custom`; opening new JSON with the old binary will refuse. A recovery copy is available for *manual, explicitly confirmed* restoration only. Copying that old snapshot back would remove reminders created after the snapshot. NEVER auto-roll back.
- Existing one-time/daily/weekly reminders retain their original serialized enum values and behavior. The ordinary quick editor cannot reinterpret custom schedules.
- This is experimental and should remain on the stacked draft branch until the versioned release/rollback policy and physical Windows user acceptance are approved.
- Physical input/DPI/accessibility testing is still outstanding, including localized font scaling, keyboard focus, accessibility names, screen readers, lock/sleep and multiple displays.

## Automated test coverage
- Added first-custom snapshot from legacy JSON, no overwrite after further creation, persistence of future custom edits, SeriesId stability, invalid edit no-write, failed edit rollback, historical occurrence cannot change.
- Inherits PR #22 tests for custom continuation/idempotence, unknown schema fail closed, interval anchors, restart and invalid options.
- Windows CI must pass WPF XAML parsing + code-behind compilation, core regressions, installer and real predecessor upgrade.
- **Do not market or distribute this as v0.18 production:** advanced schedule UI still needs explicit user QA and PR-stack integration after all independently green.

## Windows acceptance checklist
1. Open Reminders, select Custom schedule; verify custom panel opens and Quick +1min is disabled.
2. Choose weekdays Mon/Wed/Fri, every 1 week, appropriate Start date and HH:mm; verify next occurrence preview and save.
3. Choose Every few days, every 3 days; confirm preview, save, restart, and status/series remain correct.
4. Edit an unsent custom reminder, change time and interval; verify it reopens with its selections and updated next date. Confirm a delivered historical occurrence refuses editing.
5. Leave all days unchecked, enter 0/53 weeks, 0/366 days, bad HH:mm or blank title: see an error and no saved data change.
6. Confirm first custom save creates the never-overwritten `reminders.pre-custom-v017.json` backup. Never overwrite the active data with the backup without reviewing later changes.
7. Test at 810×640, at 125–200% DPI, Tab/Enter, keyboard checklist selection, and reminder popup Done/Snooze/restart/sleep.
