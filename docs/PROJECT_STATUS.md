# Reminder Pet — Project status & chat handoff

_Last updated: 2026-10-16. This file is canonical and is updated as work is committed._

## Identity
- Repository: `rayadinaveen98-ship-it/reminder-pet`. Keep the GitHub name **reminder-pet**.
- Working product codename: PetPing (not final branding).
- Product promise: a small animated companion announces reminders, offers Done/Snooze and does not require a cloud account.
- Platform order: **Windows desktop first, Android second**. iOS is later and technically restricted.
- Technology: C# / .NET 10 / WPF on Windows; offline local JSON store in `%LOCALAPPDATA%\PetPing`; Windows tray process; system SpeechSynthesizer. Android future: Kotlin/Compose, Room, AlarmManager/notifications.
- No paid APIs or mandatory hosted services for alpha.

## Last *verified* release (before v0.5)
- **v0.4 Windows alpha:** compiled Windows x64 using GitHub Actions and passed **21** core smoke checks. CI run: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38862763782 .
- User manually confirmed: the pet appears above desktop apps, and in v0.4 the character is outside the notification bubble and works. Whether spoken text is consistently audible remains **unconfirmed**.
- The downloaded build is an **unsigned portable Windows executable**, not an installer or production release. GitHub workflow artifacts have limited retention.
- UI: floating orange original WPF vector cat, small speech bubble, Done, Snooze, Hide, idle/movement animations, maximized-window placement heuristic.
- Reminder UX: one-off reminders, 10m snooze, Needs Attention inbox, sleep/restart catch-up (15m), tray opt-in autostart.

## Active milestone
**v0.5 — reliability and diagnostics**. Branch: `feature/windows-v0.5-reliability`.

Current status: **IN PROGRESS — source changes committed, v0.5 CI pending**. Do not call v0.5 complete until CI passes and manual tests are recorded.

Priority work:
1. Handle presentation failures safely and visibly; do not lose state because a pet popup or speech cannot render.
2. Prevent scheduler failures from silently stopping the app; add private local diagnostics and a clear manual recovery route.
3. Add deterministic tests for time boundaries, multiple reminders, restart/snooze/corrupt storage scenarios.
4. Improve resume/lock safety, avoid spoken alerts on the secure lock screen.
5. Produce new CI-verified unsigned Windows alpha ZIP.

## Known constraints and risks
- The tray **must** remain running. Quitting, force-stop, signing out or shutdown prevents continuous reminders. Optional Windows-startup registration begins it on future login.
- WPF cannot display on Windows' secure lock screen. Sleep/power-off cannot guarantee an exact-time popup.
- NotifyIcon balloons/Windows notifications can be suppressed by OS settings; never call them guaranteed delivery.
- The 15-minute late policy marks much older reminders Missed rather than speaking hours after due time.
- On high DPI, multiple monitors, fullscreen games, text scaling or battery pressure, overlay quality is not yet verified.
- A passing CI build does not prove Windows physical lock/sleep/voice behavior.
- Never store reminder text in diagnostics unnecessarily.

## Release path
- Current supported test build: v0.4 until a **new v0.5 artifact** is verified.
- After v0.5: v0.6 daily/weekly repeat, edit reminders and better dashboard; v0.7 expressive characters; v0.8–1.0 Windows packaging/QA; then Android foundation.
- Keep `docs/ROADMAP.md`, `docs/TEST_PLAN.md`, `README.md`, this file and `docs/ENGINEERING_LOG.md` synchronized at milestone boundaries.

## Resume prompt (copy into a new chat)
> Continue the **reminder-pet** project from GitHub `rayadinaveen98-ship-it/reminder-pet`. Read `docs/PROJECT_STATUS.md` and `docs/ENGINEERING_LOG.md`, check open PRs and the latest CI run, then proceed with the next unchecked milestone. Windows first, Android second; no paid backend. Commit meaningful increments, update handoff docs, verify CI before merging, and distinguish automated build success from actual device testing.

## Implementation progress — v0.5
- ✅ Git continuity: `docs/PROJECT_STATUS.md` and `docs/ENGINEERING_LOG.md` created and committed to the feature branch.
- ✅ Core changes: storage mutation rollback, crash-safe scheduler fault boundaries + 30s write retry cooldown, local bounded diagnostics, generic tray fallback, speech test, and stopping voice on lock/suspend.
- ✅ Regression tests added for failed writes, diagnostics privacy, multiple same-time IDs.
- ⏳ Windows Actions CI to be run, reviewed and fixed before merging.
- ⏳ Actual device tests; cannot be marked passed based only on CI.

## Next action
Open a PR from `feature/windows-v0.5-reliability`, run Windows CI, fix any compile/test failures, merge to `main` only when green, update this file and download latest v0.5 test ZIP. Then get the user's hands-on lock/wake/voice test results.
