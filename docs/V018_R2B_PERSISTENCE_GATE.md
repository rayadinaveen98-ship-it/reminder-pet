# PetPing v0.18 — R2-B custom recurrence persistence safety

**Development status:** draft stacked on R2-B pure calculation PR #21. **NOT** part of the released v0.17 preview. No custom recurrence UI enabled, no version bump or stable installer.

## Data contract — additive v1
- `RepeatPattern.Custom` is a NEW enum value appended after the previous `None`, `Daily`, `Weekly` values; none of the old numeric enum values change.
- Each custom occurrence carries `CustomSchedule` with `SchemaVersion = 1`, `Kind = Weekdays | EveryNDays`, `AnchorLocalDate` (local date without UTC offset), `Interval`, `WeekdayMask` (Monday bit0 through Sunday bit6).
- Time zone remains `RepeatTimeZoneId`, intended wall clock remains `RepeatLocalTime`, occurrence identity and history remain `Id`, `SeriesId`, `OriginalDueAt` and `RepeatContinuationCreated`.
- No new top-level JSON database, forced migration, data deletion, remote sync, or reinterpretation of preexisting daily/weekly entries.
- `ReadValidated` rejects custom entries with unknown schema version/kind, malformed anchor, missing weekday selection, invalid interval/time zone, missing zone/wall clock/series, or custom settings illegally attached to legacy entries. It leaves unreadable files untouched and requires explicit recovery rather than overwriting.
- Existing v0.17 binaries **cannot understand** `RepeatPattern.Custom`. A downgrade when actual custom records have been created may refuse to load them; this is deliberately fail-closed, not lossless backward runtime compatibility. Do not enable custom creation until the updater, compatibility instructions and rollback plan have been approved. Preserve a pre-upgrade backup.
- Editing custom reminders with the legacy WPF quick editor is blocked rather than converting them silently into one-time reminders.

## Domain integration
- `ReminderStore.AddCustom` is an opt-in **domain API only**, computes the first future schedule with original local anchor and wall time, validates before saving, and rolls back failed additions.
- `RecurrenceRules.NextAfter` dispatches validated custom options to the bounded calculator. Every successor is STRICTLY later than the current occurrence's original due and supplied reference time.
- The existing store's atomic `Change` persists delivered state, successor and `RepeatContinuationCreated` in a single save. The new custom metadata is copied unchanged to the next occurrence and saved/restored on rollback.
- Snoozing an already-continued occurrence never generates a second next occurrence, even when re-delivered. No unbounded "catch-up every missed occurrence" loop.
- `CustomSchedule` is immutable (init-only record), not a mutable list inside the occurrence.

## CI and physical gates
- Deterministic test fixtures: initial save/restart, safe legacy edit denial, one successor, re-delivery after snooze, child round-trip, N-day successor, simulated temp-file write failure+retry, corrupt schema fail-closed, invalid weekday masks/intervals, legacy daily/weekly JSON.
- Windows CI: WPF compile, core tests, seven-size original ICO, installer build and real predecessor version upgrade tests.
- Pending: dedicated custom editor, explicit "this occurrence" vs "all future" edit/delete policy, versioned safe downgrade/import behavior, cross-process single-instance conflict tests, accessibility, DST on actual Windows and real laptop sleep/wake/voice.
- **No merge/release** until PR #21 plus this stacked PR have green CI and user physical v0.17 acceptance criteria are met. Do not claim that v0.18 custom recurring reminders can be created through the app.
