# PetPing v0.18 — Phase 1: Reminder Reliability and Data Safety

**Implementation:** isolated experimental branch only. The current `main` v0.17 preview is unmodified. No claim of physical device QA or stable release readiness.

## Why this work
A normal Windows tray timer isn't a real-time clock alarm: ticks can be delayed by a busy UI, sleep, session lock or process restart. The app must reconcile late reminders deterministically and NEVER mark Done/Delivered before the save succeeds.

## Reliability implementation
- `ReminderDueProcessor` is UI-independent. It accepts a specific `DateTimeOffset` so missed/delivered decisions can be exercised with a frozen clock.
- WPF `ReminderScheduler` is a thin dispatcher adapter; startup, resume and session unlock already invoke `CheckNow` and still use the same processor.
- A guarded reentrancy flag prevents nested notifications when a UI callback triggers another scheduler check.
- Reminders are iterated from a due-time-ordered snapshot, then revalidated before each transition. A callback can remove/complete a later reminder without resulting in a stale delivery.
- Writes are committed *before* any presentation callback; a broken popup or dashboard refresh does not roll back a correctly saved reminder or cause a second delivery.
- On a failed save, no delivery is announced; the reminder stays Pending, a non-fatal fault is reported and further writes are delayed 30 seconds before retry. Fault observers cannot crash processing.
- Missed recurring reminders now use the **same reconciliation clock** for the stored FinishedAt and future-series successor, avoiding a different UtcNow boundary during recovery.
- **Still not guaranteed:** Windows cannot present an alarm while a PC is asleep, switched off or the app process isn't running. Reminders more than 15 minutes late are marked Missed/Needs Attention, not portrayed as exact-time deliveries.

## User-directed reminder backup
- New Settings action **Export reminder backup** invokes a native folder chooser. Only a user click initiates export. It makes a uniquely named JSON snapshot without modifying active reminders or replacing prior exports.
- The source JSON is validated first, the copied JSON is parsed again, and streaming SHA-256 equality of source and copy is checked before reporting success.
- Corrupt saved data is refused; no automatic import/restore is performed. A backup should be stored in a separate folder or external drive, not the active PetPing data folder.
- This exports reminder records **only**. Pet appearance, profiles and speech settings are not included in this particular backup. Full-profile export/import remains separately gated.
- Exported files contain private reminder names and schedules; users choose how/where they store or share them.

## Deterministic regression tests
- Due vs older-than-15-minute missed vs future pending, idempotent repeated tick.
- A UI callback that throws; a callback that recursively invokes the due check; an event handler that deletes a reminder from the snapshotted list.
- Failed temp-file write and same-clock 30-second retry boundary; fault observer throws but no scheduler crash or false delivery.
- Three-week downtime of a weekly series: a single next future occurrence after the actual reconciliation clock, no backlog or repeat duplication after reopening storage.
- Export verified, repeated exports are non-overwriting, old copies unchanged after new reminder creation, corrupt primary refused, no saved file refused, active data folder refused.

## Acceptance/release gates
- Green combined Windows CI: XAML/static validation, core tests, WPF x64, original multi-size icon, experimental Inno Setup, production/experimental data preservation through install/reinstall/uninstall.
- Manual Windows QA: system tray running/closed, 15-minute late reconciliation on resume, popup Done/Snooze, disk-permission failure messaging, use of a separate export destination, high DPI/accessibility.
- Full-profile backups/import/restore and controlled migration from v0.17 to v0.18 have **not** been implemented by this phase.
- Keep this work in draft and unmerged until user's laptop acceptance; never claim tested while suspended/off on a physical laptop.
