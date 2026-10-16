# Architecture — Windows first, Android ready

## Architecture Decision Records

### ADR-001: Native Windows first (C#/.NET 10 WPF)
**Accepted.** Windows-first transparent topmost pet popup, integration with session state and tray operation are simpler to control using WPF/Win32 than through an assumed cross-platform overlay abstraction. Android will use Kotlin + Compose. We share domain semantics/schema, not UI widgets or a shared background process.

### ADR-002: Local-first storage
**Accepted for proof of concept.** JSON at `%LOCALAPPDATA%\\PetPing\\reminders.json`, written to a temp file and replaced on save. No network calls. Evolve to SQLite/Room when migration, recurrence and event logs become necessary. Corrupt JSON is a startup error, not interpreted as an empty reminder list.

### ADR-003: Running tray agent; no magic background work
**Accepted.** Closing the UI hides it. The process remains open via a visible system-tray icon. Explicit Quit terminates reminders. Future iteration may introduce Windows Task Scheduler for startup, wake and reactivation, but this v0 doesn't. No silent persistency claims after quitting or shutdown.

### ADR-004: Reminder identity and delivery
**Accepted for v0.** Each one-time reminder has UUID, original due time, current due time, status, delivered time, completed time and snooze count. On tick, due reminders are persisted as Delivered **before** UI callback, preventing repeated sends on the next tick. On wake/start, reminders <=15 minutes late show; older reminders are marked Missed. This is *at-most-once attempted delivery* in current agent; it is not exactly-once human-visible delivery if a crash occurs after persistence but before the popup.

### ADR-005: Time representation
**Accepted for v0.** One-time reminders store `DateTimeOffset` and are compared by absolute instant (UTC-equivalent comparison). The local date/time UI rejects DST-invalid and DST-ambiguous timestamps rather than silently scheduling unexpected times. For daily/weekly repeats, introduce an IANA/Windows timezone mapping and explicit local-wall-time recurrence semantics later. No recurring UI in v0.

### ADR-006: Alert delivery and fallback
**Accepted for v0.** Popup renderer and voice are separable. Missing voice doesn't block the visible alert. Session lock causes queued visual presentation after unlock; Windows secure screen cannot be overlaid. Focus assist, game fullscreen, notifications and automatic recovery after OS shutdown are subsequent quality phases.

## Runtime modules
```
UI Dashboard (MainWindow)
  -> ReminderStore -> JSON local persistence
  -> ReminderScheduler (1 second DispatcherTimer)
      -> marks Delivered -> queue of pending pet popups
          -> PetWindow -> Done/Snooze -> ReminderStore
          -> VoiceService (System.Speech optional voice output)
App lifecycle
  -> System-tray NotifyIcon (Open, Quit)
  -> PowerModeChanged wake reconciliation
  -> SessionSwitch defer overlay while locked
```

## State transitions
`Pending -> Delivered -> Completed`
`Pending -> Missed` (late > 15m when resumed/start)
`Delivered -> Pending` (snoozed; due date shifted)
`Delivered -> Dismissed` (reserved; no dismiss button in v0)
`Pending -> Completed` (manual action)

For the current implementation, "Done" completes and snooze schedules again. Dismissing the popup via OS close action leaves it `Delivered` in history rather than pretending it was done. There is no recurrence yet.

## Storage contract example
```json
{
  "Id": "9c1b3d4d-bd50-401b-96ef-b83d852361d9",
  "Title": "Time to work on my short film",
  "DueAt": "2026-10-16T19:00:00+05:30",
  "OriginalDueAt": "2026-10-16T19:00:00+05:30",
  "Status": "Pending",
  "DeliveredAt": null,
  "FinishedAt": null,
  "SnoozeCount": 0
}
```
The file holds a JSON *array* of entries of this shape. These names match the current C# PascalCase serializer; a future cross-device transport format can use a versioned, normalized schema.

## Android adapter (design only, not built)
- Kotlin Jetpack Compose for app UI.
- Room database for reminders and occurrence ledger.
- `AlarmManager` for user-requested exact reminders; if allowed use `setExactAndAllowWhileIdle` / `setAlarmClock` per policy and need.
- `PendingIntent` -> explicit broadcast receiver -> persistence change -> notification always, permitted overlay only when eligible and appropriate.
- Runtime POST_NOTIFICATIONS for modern Android and explicit check of exact-alarm special permission.
- Recreate outstanding alarms on reboot (`BOOT_COMPLETED`) and app updates; exact alarm settings might change.
- Separate notification channel(s) based on urgency. Alarm-style fullscreen intents only if the app qualifies for platform policy.
- Android power-management and OEM restrictions must be tested; never promise force-stop recovery.

## Cross-device future
Local identity per device first; later encrypted sign-in/sync as an optional capability. Remote sync only updates reminder state; delivery is scheduled independently on each device, and devices can have user-selected delivery rules to prevent multiple pets announcing simultaneously.

## Quality caveats
This repository contains initial Windows source but has not been compiled and run on a real Windows machine in this environment. Several production hardening features are intentionally deferred: process-crash recovery, startup registration, application signing, overlay collision handling, DST recurrence, telemetry/diagnostic consent, malware-safety review, unit tests and automated CI.
