# PetPing v0.18 integration — R2-A + R2-B

**State: experimental development branch.** v0.17 (main) remains the current Windows laptop-test preview, and physical acceptance is still uncompleted.

## Integrated functionality
- Flexible snooze from dashboard (5/10/15/30/60 minutes, custom whole minutes 1–1440). Floating pet retains one-tap 10 minutes and offers 5/15/30/60-minute menu.
- Shared editor name/time/repeat validation (future date, DST skipped/repeated hours, 120-character titles) and truthful next-occurrence previews.
- Custom weekday and every-N-day recurrence, immutable original local-calendar anchor and wall clock, idempotent next-occurrence generation, fail-closed schema validation.
- Actual WPF Custom schedule UI with selected weekdays/every-N-weeks and every-N-days controls; create and edit upcoming unsent custom reminders.
- Same original Logo C and native offline tray app; no account, remote services or telemetry.

## Merge resolution
- Kept R2-B's complete Custom schedule editor and reused R2-A's duration picker on Reminders; retained R2-A floating-pet action/event signature as a matched trio (PetWindow XAML + codebehind + App callback).
- Kept R2-B's atomic store changes, custom schema and validated backup. Applied R2-A snooze bounds and shared name validation without changing custom record formats.
- Legacy `ReminderStore.Add(..., RepeatPattern.Custom)` now explicitly refuses. Call `AddCustom` with fully validated metadata; this prevents corrupt incomplete records.
- Both R2-A and R2-B tests are compiled in one core regression suite, rather than claiming separate green PRs prove the merged implementation works.
- **Important**: source implementation is staged in a development branch, not merged into `main`.

## Migration & downgrade safety
- First-time creation of a Custom reminder makes a non-overwriting `reminders.pre-custom-v017.json` containing a validated snapshot of the user's earlier reminders. This snapshot is a safety net, **not** an automatically usable rollback. Restoring it later loses newer reminders.
- v0.17 does not understand the Custom enum and will refuse to open a newer `reminders.json` containing Custom. A safe actual downgrade requires exporting all current reminders or staying on the new build—not blindly restoring an old snapshot.
- Until an explicit data migration/export/restore UX and device acceptance exists, **do not publish this as a stable upgrade or encourage reverting an installed copy to v0.17**.
- A dev preview must be clearly marked **experimental, unsigned**, cannot replace v0.17 release artifacts and must warn before first custom schedule creation. Backup current `%LOCALAPPDATA%\PetPing` externally before testing.
- Existing daily and weekly JSON, recurrence IDs, voice/TTS settings, reminder/snooze history and uninstall-data preservation remain mandatory.

## Required gates
1. Core regression tests from both PRs green on the **combined integration tree**.
2. WPF Windows x64 publish plus native icon, installer and pinned v0.15→current preview legacy-preservation smoke.
3. Manual v0.17 laptop QA: popup Done/Snooze/mute, startup, tray, DPI 100–200%, keyboard/a11y, voice and lock/sleep.
4. Manual dev testing: custom weekdays, 2-week intervals, custom 3-day reminders, edit, restart, modified dates and DST, no cross-series duplicates; flexible snooze during an active custom recurrence.
5. Data recovery: actual restore/export of reminders after Custom was used, failure injection and backup preservation, safe downgrade/migration statement.
6. Only after these gates: merge R2-A/R2-B/integration in reviewed order and version a production v0.18 build.

## Follow-up
Review possible desktop time-shift/DST edge cases with a local time-zone test harness and extend automation to a real v0.17 → v0.18 experimental install. Do not claim physical QA from GitHub Actions alone.
