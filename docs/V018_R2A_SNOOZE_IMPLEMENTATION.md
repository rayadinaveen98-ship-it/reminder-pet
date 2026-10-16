# PetPing v0.18 — R2-A / Flexible Snooze foundation

**Status:** Feature-branch implementation, not part of approved v0.17 release. Real-device v0.17 acceptance is still open.

## Implemented in this slice
- Pure, offline `SnoozePolicy`: preset intervals 5, 10, 15, 30 and 60 minutes; explicit default 10; custom integer minutes 1–1440 (24 hours).
- Reminder dashboard: select a preset or custom minutes, then Snooze. Input errors are visible and do not write.
- Floating companion: existing one-click 10m action stays; adjacent More button offers 5, 15, 30, 60m. Custom is supported from dashboard (not yet on tiny floating overlay).
- ReminderStore checks duration bounds before any state transition; it keeps the existing transactional in-memory rollback and on-disk safety mechanisms.
- Popup result is interpreted as successful only when the save callback explicitly returns true; failed/missing callbacks leave the reminder actionable rather than falsely celebrating success.
- Daily/weekly series continuation, original wall time and next-occurrence identity are not mutated by snooze. No schema migration and no network or paid APIs.

## Automated acceptance
- Pure policy rejects 0, negative, under-one-minute and over-24-hour durations.
- Five duration presets exist in the specified order.
- A delivered daily reminder snoozed for 5m then 24h preserves its original recurrence anchor and never creates duplicate future occurrences.
- Persistence reopens with correct SnoozeCount, status and next due; oversized invalid action cannot modify disk state.
- Existing .NET core checks, XAML validation, WPF build, installer and real v0.15→v0.17 upgrade must remain green.

## Pending
- Custom duration chooser in the floating companion (dashboard-only custom is intentionally documented).
- Physical Windows accessibility/keyboard/IME and display-scaling QA of the selector and popup menu.
- R2-A editor validation and polished due-at preview; R2-B weekday recurrence and interval support.
- v0.17 must receive physical laptop review before v0.18 is merged or labeled released.

## Manual test
1. On Reminders, select a missed/delivered item, choose 5m or 60m, click Snooze; confirm its new due label and persistence.
2. Choose Custom and enter 1, 1440 and invalid values (0, 1441, alphabetic, blank). Verify only valid entries succeed.
3. Show the floating pet; tap Snooze 10m, or More and 15m/30m. Confirm exactly one reminder reschedules with the chosen delay.
4. Verify first occurrence of a Daily/Weekly reminder can be snoozed without generating a duplicate next-day/next-week event.
