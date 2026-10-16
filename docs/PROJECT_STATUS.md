# Reminder Pet — Project status & chat handoff

_Last updated: 2026-10-16. This file is canonical and is updated as work is committed._

## Identity
- Repository: `rayadinaveen98-ship-it/reminder-pet`. Keep the GitHub name **reminder-pet**.
- Working product codename: PetPing (not final branding).
- Product promise: a small animated companion announces reminders, offers Done/Snooze and does not require a cloud account.
- Platform order: **Windows desktop first, Android second**. iOS is later and technically restricted.
- Technology: C# / .NET 10 / WPF on Windows; offline local JSON store in `%LOCALAPPDATA%\PetPing`; Windows tray process; system SpeechSynthesizer. Android future: Kotlin/Compose, Room, AlarmManager/notifications.
- No paid APIs or mandatory hosted services for alpha.

## Verified build status
- **v0.8 Windows alpha (merged, CI-verified; laptop installation QA pending):** PR #7 squash-merged to `main` at `fc33f2264141ae71f20f582e1b9ad4bfd9344592`. Windows CI: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38877847189 — **64 core checks**, WPF publish, free Inno Setup installer compilation and successful **silent install -> installed EXE check -> silent uninstall -> user-data sentinel retained**. Artifacts: portable `PetPing-Windows-v0.8-unsigned` (ID 12035474939), installer `PetPing-Installer-v0.8-unsigned` (ID 12036600668). Both GitHub ZIPs checked for integrity; installer archive contains `PetPing-Setup-v0.8-unsigned.exe`.
- **v0.7 Windows alpha (merged, CI-verified, hands-on QA pending):** Windows run https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38872547917 passed 56 core tests, compiled and uploaded `PetPing-Windows-v0.7-unsigned` (artifact 12034680037); ZIP extracted and checked for `PetPing.Windows.exe`.
- **v0.6 Windows alpha (CI-verified, manual QA pending):** Windows x64 compilation and **49** core regression checks passed. GitHub CI: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38868473793 ; artifact `PetPing-Windows-v0.6-unsigned` (final artifact ID 12032645056; CI run 38868752195).
- **v0.5 Windows alpha:** compiled Windows x64 and passed **32** core regression checks. GitHub CI: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38864475272 ; artifact: `PetPing-Windows-v0.5-unsigned` (GitHub Actions retention applies).
- **v0.4 user-tested baseline:** compiled and passed 21 core checks. User confirmed the floating character is outside the bubble and shows over ordinary desktop apps. CI: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38862763782 .
- User manually confirmed: the pet appears above desktop apps, and in v0.4 the character is outside the notification bubble and works. Whether spoken text is consistently audible remains **unconfirmed**.
- The downloaded build is an **unsigned portable Windows executable**, not an installer or production release. GitHub workflow artifacts have limited retention.
- UI: floating orange original WPF vector cat, small speech bubble, Done, Snooze, Hide, idle/movement animations, maximized-window placement heuristic.
- Reminder UX: one-off reminders, 10m snooze, Needs Attention inbox, sleep/restart catch-up (15m), tray opt-in autostart.

## Active milestone
**v0.9 — safe reminder recovery and beta-quality checks**, feature branch `feature/windows-v0.9-recovery-quality`.

Status: **SOURCE + REGRESSION TESTS COMMITTED, PR/CI NOT YET VERIFIED**. Last confirmed merged build is v0.8 (64 core checks, installer install/uninstall smoke). User asked to continue; v0.8 interactive installer/sleep/lock QA remains unverified.

Planned v0.9 scope:
1. On unreadable `reminders.json`, offer a **user-confirmed** restore of `reminders.json.bak` only if its contents pass validation. The backup may be older and can omit recent changes. Cancel must leave all files unchanged. On restore, preserve unreadable primary as a separately named file.
2. Provide a visible dashboard button that opens the private data/diagnostics folder; never upload reminder contents.
3. Test backup eligibility, preserved damaged primary, non-destructive restore, failure on bad backup and reminder state migration.
4. Installer repair/reinstall smoke: execute same version twice, then uninstall, preserving a LocalAppData sentinel. True older-to-newer-version installation still needs manual QA.
5. Compile Windows app and optional unsigned installer in GitHub Actions, update this status and merge only on green CI.

## Known constraints and risks
- The tray **must** remain running. Quitting, force-stop, signing out or shutdown prevents continuous reminders. Optional Windows-startup registration begins it on future login.
- WPF cannot display on Windows' secure lock screen. Sleep/power-off cannot guarantee an exact-time popup.
- NotifyIcon balloons/Windows notifications can be suppressed by OS settings; never call them guaranteed delivery.
- The 15-minute late policy marks much older reminders Missed rather than speaking hours after due time.
- On high DPI, multiple monitors, fullscreen games, text scaling or battery pressure, overlay quality is not yet verified.
- A passing CI build does not prove Windows physical lock/sleep/voice behavior.
- Never store reminder text in diagnostics unnecessarily.

## Release path
- Latest CI-verified alpha: **v0.8**, optional per-user unsigned setup EXE and Windows portable ZIP (CI 38877847189, artifacts 12036600668 and 12035474939). User said v0.7 is good; no v0.8 hands-on QA yet.
- After v0.5: v0.6 daily/weekly repeat, edit reminders and better dashboard; v0.7 expressive characters; v0.8–1.0 Windows packaging/QA; then Android foundation.
- Keep `docs/ROADMAP.md`, `docs/TEST_PLAN.md`, `README.md`, this file and `docs/ENGINEERING_LOG.md` synchronized at milestone boundaries.

## Resume prompt (copy into a new chat)
> Continue the **reminder-pet** project from GitHub `rayadinaveen98-ship-it/reminder-pet`. Read `docs/PROJECT_STATUS.md` and `docs/ENGINEERING_LOG.md`, check open PRs and the latest CI run, then proceed with the next unchecked milestone. Windows first, Android second; no paid backend. Commit meaningful increments, update handoff docs, verify CI before merging, and distinguish automated build success from actual device testing.

## Implementation progress — v0.5
- ✅ Git continuity: `docs/PROJECT_STATUS.md` and `docs/ENGINEERING_LOG.md` created and committed to the feature branch.
- ✅ Core changes: storage mutation rollback, crash-safe scheduler fault boundaries + 30s write retry cooldown, local bounded diagnostics, generic tray fallback, speech test, and stopping voice on lock/suspend.
- ✅ Regression tests added for failed writes, diagnostics privacy, multiple same-time IDs.
- ✅ Windows Actions CI run #38864475272 passed: XAML/static checks, 32 smoke checks, x64 EXE publish and artifact upload.
- ✅ GitHub PR #4 merged to `main`; last CI run (docs updated): https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38864751672 — success, 32 checks, portable ZIP uploaded.
- ⏳ Actual device tests; cannot be marked passed based only on CI.

## v0.6 engineering checkpoint
- Repeating reminders: Daily and Weekly based on local wall time in saved timezone, including DST gaps/ambiguous times.
- Separate persistent occurrence records and atomic next-event creation on delivery/miss, with snooze continuation flag to prevent duplicates.
- Edit ordinary future unsent reminders and adjust repeat settings; previous history stays intact.
- Added deterministic recurrence, DST, long-offline, legacy-file, rollback regression tests. **49 core checks passed** and a Windows artifact was uploaded in GitHub run https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38868473793 .

## v0.7 scope / implementation
- Persistent `pet-appearance.json` independent of reminders, with Auto/Top/Bottom corner selection and Playful/Gentle/Off motion.
- Original vector cat blinking eyes and speech-mouth driven by Windows TTS started/completed events; friendly Done/Snooze reactions only when saving succeeds. Popup stays actionable if saving fails.
- Uses foreground-monitor coordinate conversion to improve placement, but physical mixed-DPI QA is pending.
- No alternate pet animals or compiled APK in this phase.

## v0.8 implementation checkpoint
- Inbox views: All, Today, Upcoming, Needs Attention, History; keyboard shortcuts, empty states, accessibility names and delete confirmation.
- Per-user Inno Setup installer alongside portable ZIP with a CI silent installation/uninstallation test preserving `%LOCALAPPDATA%\PetPing` data.
- Added pure filter rules and deterministic smoke checks. The reminder scheduling/store schema and pet behavior are unchanged.

## Next action
Open PR from `feature/windows-v0.9-recovery-quality`, run Windows CI, inspect failure logs and correct source/installer as required. Merge only with green core tests, portable Windows compile, per-user installer build, reinstall/uninstall storage-preservation smoke. Update this handoff with the final commit/run/artifact IDs and share a testable v0.9 build. User device QA: corrupt-file prompt Yes/No paths (with test data only), open-data-folder action, installer upgrade from v0.8, recurring reminders, tray, lock/unlock and sleep/wake.

**Release warning:** an automated CI pass does NOT prove that real laptop recovery or OS-specific notification behavior works. Windows v1.0 remains pending device acceptance; Android starts later.
