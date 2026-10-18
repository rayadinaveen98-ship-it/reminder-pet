# PetPing Android v0.1 — native reminder foundation

**Status:** first real Android project. Must pass CI and physical Android device QA before a production release.

## Locked Windows-derived decisions
- Cat (Milo), Dog (Buddy), Bunny (Luna), **no walking**, no floating across screen.
- Android v0.1 uses recognizable **emoji placeholders**, not the actual user-approved premium art. Import the original static art in a later approved phase, do not silently redesign it.
- Native Kotlin + Jetpack Compose, Android API 26+; target/compile **API 36**, current 2026 Google Play new-app target requirement.
- No account, analytics, internet permission, tracking, hosting, paid APIs or Windows profile migration.
- Functional app: one-time reminders, notifications, Done, Snooze 10m, remove, permission statuses, pet picker. Custom repeats, Windows-specific pet rigs, and automatic background speech NOT included yet.

## Reminder reliability and privacy
- Private atomic JSON persistence, schema-v1, fail-closed on corrupt version or malformed JSON. Pet selection local SharedPreferences.
- AlarmManager one pending alarm for earliest due reminder; setExactAndAllowWhileIdle if exact permission approved, otherwise setAndAllowWhileIdle inexact fallback (can run late).
- Android 13+ POST_NOTIFICATIONS permission must be granted to show notification; app's local needs-attention inbox works even without it.
- Android 12+ SCHEDULE_EXACT_ALARM special access requested explicitly; app does NOT use restricted USE_EXACT_ALARM shortcut. Android 14+ normally denies exact access to new installs. Caller sees current timing mode.
- Receiver stores DELIVERED before showing Android HIGH notification with immutable Done/Snooze action PendingIntents. After 15 minutes overdue, mark MISSED instead of presenting an excessively stale alarm.
- BOOT_COMPLETED, MY_PACKAGE_REPLACED, TIME_SET, TIMEZONE_CHANGED trigger pending reconciliation; permission-grant change re-arms schedule. No background screen overlays or full-screen intents.
- Force-stop, Doze/battery saver, OEM scheduling restrictions and denied permissions can still delay notifications. Real phone testing required.

## Build and device QA
CI: `.github/workflows/android-alpha.yml` installs Android SDK API 36, Gradle 8.13, AGP 8.13.2, Kotlin/Compose 2.2.21, Compose BOM 2026.08.00. Tests run before generating debug APK.
Manual QA scenarios: Android 12/13/14/15/16, notification permission denied/granted, exact alarm denied/granted/revoked, reboot, Doze, force-stop, system clock changes, 15min catch-up boundary, multiple alarms, notification action Done/Snooze, corruption/atomic rollback, 100–200% display scaling.
Debug APK is not a Play-signed release; no real-device testing claimed until performed.

## Next phases
1. Verify CI-green APK and real Android device alarm/permission delivery; fix all reported issues.
2. Room database schema/migrations, reliable recurrence, search/edit/priorities and alarm history.
3. Import approved Cat/Dog/Bunny **static** artwork, richer Compose UI, no walking.
4. Optional user-controlled voice and appropriate notification/lockscreen experience, with Play policy review.
5. Beta reliability testing, signing, packaging and privacy/Play release.
