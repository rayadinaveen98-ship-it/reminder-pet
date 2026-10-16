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
- **v0.14 expressive pets:** PR #13 merged as `4f8be7cae20651161651bbec444f412fa34902df`. Windows CI https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38900092320 passed **122 core tests**, WPF win-x64 publish and unsigned installer smoke (same-version install/reinstall/uninstall without removing LocalAppData test data). Portable artifact `PetPing-Windows-v0.14-unsigned` ID 12045033789; installer artifact `PetPing-Installer-v0.14-unsigned` ID 12044899354. Both ZIP CRCs passed. Extracted `PetPing-Setup-v0.14-unsigned.exe`, SHA-256 `47414e3f92db6c52f2f5c6887f996de98f5d77f8a7b671340571c1f3b235ec77`. Physical UI/motion test pending.
- **v0.13 multi-pet Windows preview:** PR #12 merged as `6410b4400651c8a6ac473dafa6331aa8f0f1217c`. Windows run https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38897525278 passed **105 core checks**, WPF compile and install/reinstall/uninstall storage-preservation smoke. Portable artifact `PetPing-Windows-v0.13-unsigned` ID 12044223333, installer `PetPing-Installer-v0.13-unsigned` ID 12045065431; downloaded both ZIPs and CRC verified; setup EXE SHA-256 `fbb7e06bac08ec1f5d600a529a127cba289759b213d26c953b56f5e40c71944f`. Laptop visual QA pending.
- **v0.12 redesigned dashboard preview:** PR #11 merged 8d5b0ba00d41aea2d76e94ea4de86361a3c0076a. CI https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38893597316 green, 91 core checks and Windows installer smoke. Artifacts 12042344606 (portable) and 12043370603 (setup). UI physical testing open.
- **v0.9 Windows alpha (merged and CI verified, laptop QA pending):** PR #8 merged into main as `946e90be775afda0b06946f4b5331761942b9f58`. GitHub Actions https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38881511852 passed **72 core checks**, compiled win-x64 WPF, built Inno Setup 6 installer, and passed silent install, same-version reinstall, uninstall and preservation of a test reminder-data file. Artifact IDs: portable 12038021245, installer 12037687579. Both ZIP archives verified intact and `PetPing-Setup-v0.9-unsigned.exe` extracted.
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
**PetPing 2.0 v0.14 — expressive pet actions and celebration effects. MERGED + WINDOWS CI VERIFIED (122 checks); laptop QA pending.**

- The user accepted v0.13 and explicitly authorized the next milestone. v0.13 baseline remains merged, installer built and CI-verified (105 core checks), though no detailed UI acceptance transcript has been recorded.
- Development branch `feature/windows-v0.14-expressive-animations` merged into main via PR #13, commit `4f8be7cae20651161651bbec444f412fa34902df`. Both v0.14 unsigned Windows artifacts were downloaded and integrity checked. This is a test build, not a signed production release.
- Extended shared WPF `PetCharacter` rig with independent animated paws, dog ear tilt, bunny ear wiggle, cat/dog tail behavior, and bounded short-lived sparkle/star burst stage. Pet actor still uses a **small transparent Window**; effects do not place click-catching overlays across the desktop.
- Real action reaction now honors pet-specific Done Hop/Wave and Snooze Yawn/Stretch choices. Bunny hops visibly higher than puppy, puppy's tail wags faster, cat uses smaller careful steps. Dog ears droop for yawn and bunny ears react.
- Added configurable **Effects Off / Gentle / Playful** per companion, versioned in existing `pet-profile.json` without destructive migration. Legacy v0.13 JSON missing the new property loads with Playful default.
- Global motion modes are honored: Motion Off = no walk, no particle bursts or animated reactions; Gentle = shorter/smaller movement without particles; Full allows per-pet effects intensity.
- Pet Studio offers non-destructive **Try Done effect / Try Snooze effect** buttons, plus an Effects picker that saves per species. Only actual reminder events update the reminder database.
- Pure motion rules `PetAnimationRules` define deterministic gait and finite particle/duration budget (<=10 per reaction); old animation timers are stopped on close/lock/suspend and transient effects cleaned.
- **122 regression checks passed**, including species differences, reduced motion, bounded particles, pet-profile round-trip and v0.13 compatibility. WPF compile and installer data preservation CI passed. Physical appearance, DPI, pet controls and voice QA remain unverified.

## Known constraints and risks
- The tray **must** remain running. Quitting, force-stop, signing out or shutdown prevents continuous reminders. Optional Windows-startup registration begins it on future login.
- WPF cannot display on Windows' secure lock screen. Sleep/power-off cannot guarantee an exact-time popup.
- NotifyIcon balloons/Windows notifications can be suppressed by OS settings; never call them guaranteed delivery.
- The 15-minute late policy marks much older reminders Missed rather than speaking hours after due time.
- On high DPI, multiple monitors, fullscreen games, text scaling or battery pressure, overlay quality is not yet verified.
- A passing CI build does not prove Windows physical lock/sleep/voice behavior.
- Never store reminder text in diagnostics unnecessarily.

## Release path
- **Latest CI-verified test preview: v0.11** (86 automated core checks, unsigned portable + setup), laptop QA pending. Prior user-tested baseline is v0.9.
- Latest previously user-verified alpha: **v0.9**, unsigned Windows installer and portable ZIP (CI 38881511852; portable artifact 12038021245, installer 12037687579). User confirmed earlier releases generally work, but detailed v0.9 recovery/upgrade/laptop QA remains pending.
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
Share verified v0.14 installer and portable ZIP. User should back up `%LOCALAPPDATA%\PetPing`, quit previous tray app, then test Pet Studio on all 3 pets, checking ears/paws/tails, Try Done/Snooze, effects Off/Gentle/Playful and global Motion Off. Try an actual due reminder, Done/Snooze persistence, TTS cancellation, Windows desktop click pass-through and optionally 125–200% DPI and lock/sleep. Record actual pass/fail and fix P0 issues. If stable, v0.15 is release-quality integration and battery/multi-display QA; Windows v1.0 still requires acceptance.
