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
- **v0.16 product blueprint (docs-only):** PR #15 merged `9d3b6b0a42963c4b181c7b3d68436145ef46f085`. Windows CI https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38907615968 **success**, 122 legacy core checks, existing Windows executable/installer and prior-version synthetic-data upgrade. No app logic was changed; **v0.15 remains current actual Windows executable version**.
- **v0.15 Windows integration preview:** PR #14 merged `3ec779352c0a35f83981ac01d01a7947429b0e2f`. GitHub Actions https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38903316350 **success**, 122 core smoke checks and Windows x64 compile. Rebuilt real predecessor v0.14, installed/updated to v0.15 (confirmed new FileVersion) and verified synthetic reminders + three pet profiles by SHA-256 before/after upgrade/uninstall. Same-version repair/uninstall sentinel preserved. Portable `PetPing-Windows-v0.15-unsigned` artifact ID 12046563227; installer `PetPing-Installer-v0.15-unsigned` ID 12046104323; both ZIP CRCs good. Standalone setup SHA-256 `d37e434ed7428a412021c74e2abb31af8bf89f86d4b2d0cbfdc15823ae2377cb`. Physical laptop acceptance pending.
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
**v0.17 Stage A — Premium brand and rich UI visual exploration. DESIGN SOURCE COMMITTED; user visual review pending; Windows executable still v0.15.**

- User approved advancing from merged v0.16 blueprint. Branch: `design/v0.17-brand-ui-stage-a`; keep source/reminder schemas and installer unchanged until visual review.
- Original editable SVG candidate marks: `design/petping-3/logos/candidate-a-friendly-signal.svg`, `candidate-b-companion-monogram.svg`, `candidate-c-companion-beacon.svg`. No final brand/logo has been selected. Concepts intended for tiny tray icon and larger installer/start screen testing; originality and brand-name/trademark review still open.
- Editable Canva eight-page PetPing Concept Review: https://www.canva.com/d/rljAMk15QUasrej . Earlier 7-page `PetPing 2.0 UX` visual reference preserved: https://www.canva.com/d/0LRU2hT9b1v6vBO . The new deck is **exploratory and NOT approved**: some generated sample copy mistakenly describes pet care, health, analytics, connected/online settings and unrelated features. Do not implement those. See corrections in `docs/V017_UI_SPEC.md`.
- Exact visual/interaction spec: `docs/V017_UI_SPEC.md` — Home and Reminders hierarchy, 5-screen navigation, sample-vs-live data distinction, keyboard/error/empty states, layout at WPF logical DIP dimensions and future v0.18 controls clearly deferred.
- Refined draft visual tokens: `design/petping-3/tokens.v017.candidate.json` with selected semantic contrasts. Contrast screening: primary lavender with white text 4.71:1; ink/ivory 13.85:1; muted on white 5.73:1; **mint/white fails at 1.93:1**, so use dark ink on mint. Full rendered accessibility checks still pending.
- No WPF/source model/scheduler/pet actor/installer changed. All new visual files are candidates rather than production assets. User selects icon A/B/C or asks refinements and approves UI direction before native WPF repaint. Remaining Stage B = implement brand and responsive theme, then My Pets/Studio/Settings.

## Known constraints and risks
- The tray **must** remain running. Quitting, force-stop, signing out or shutdown prevents continuous reminders. Optional Windows-startup registration begins it on future login.
- WPF cannot display on Windows' secure lock screen. Sleep/power-off cannot guarantee an exact-time popup.
- NotifyIcon balloons/Windows notifications can be suppressed by OS settings; never call them guaranteed delivery.
- The 15-minute late policy marks much older reminders Missed rather than speaking hours after due time.
- On high DPI, multiple monitors, fullscreen games, text scaling or battery pressure, overlay quality is not yet verified.
- A passing CI build does not prove Windows physical lock/sleep/voice behavior.
- Never store reminder text in diagnostics unnecessarily.

## Release path
- **Latest completed Windows code preview: v0.15**, PR #14 merged at `3ec779352c0a35f83981ac01d01a7947429b0e2f`. Windows CI run 38903316350 passed 122 checks, WPF/installer compilation, same-version data-preservation and a **real pinned v0.14→v0.15 upgrade** preserving synthetic reminder/pet files. Artifacts: portable 12046563227, installer 12046104323. Build is unsigned and not a public-certified release.
- User accepted recent functionality broadly, but this is **not a detailed manual pass** for all Windows scenarios or a claim of user v0.15 installer acceptance.
- **Current planning version v0.16 is documents/tokens only**, with no new executable or paid design tool dependencies. Windows source remains version 0.15 until future code phases are built and tested.
- v0.17 focuses original logo concepts, native design tokens and screens; v0.18 advanced reminder semantics; v0.19 art & animation; v0.20 onboarding; v0.21 integrated QA. After Windows v1.0 quality signoff, Android native project follows.
- Keep `docs/ROADMAP.md`, `docs/TEST_PLAN.md`, `README.md`, this file and `docs/ENGINEERING_LOG.md` synchronized at each milestone.

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
Review three **original logo candidate SVGs** and the editable 8-page Canva concept deck. Note and reject the off-product pet-care/analytics examples in the automatically generated slides. Obtain preference A Friendly Signal / B Companion Monogram / C Companion Beacon and layout direction. Then v0.17 Stage B: final approved logo/ICO, WPF ResourceDictionary, responsive Home/Reminders real screens, no placeholder data, preexisting reminders/profile compatibility, Windows CI and physical DPI/readability QA. Keep v0.15 signed-off functionality as fallback; don't call v0.17 finished until WPF and user visual QA pass.

