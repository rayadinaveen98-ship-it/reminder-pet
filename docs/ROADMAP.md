# Roadmap and accountable milestones

## Milestone W0 — foundation (this delivery)
- [x] Product principles and locked desktop-first platform order
- [x] Architecture decisions (native WPF, local-first, tray agent)
- [x] Initial Windows project files
- [x] Local one-time reminder model and scheduler
- [x] Dashboard with add, test, complete and delete actions
- [x] Tray lifecycle and pet overlay with voice, done and snooze
- [x] Acceptance test plan
- [ ] Windows compilation, execution and visual QA (requires Windows/.NET SDK)

## Milestone W1 — Windows alpha
- [ ] Build on Windows 10/11 and confirm the CI compiler result; resolve any compiler/runtime failures
- [ ] Test exact-time behavior against clock with repeated one-minute reminders (core rules have automated smoke checks)
- [x] Auto-hide popup after 45 seconds without silently completing it
- [ ] Better tray icon and app launcher behavior for second process
- [ ] Installer / portable release and signed build decision
- [ ] Keyboard support, contrast, screen reader and reduced-motion checks
- [x] Persistent voice setting (notification settings remain pending)
- [ ] Critical tests: reboot, crash, logout/login, lock/unlock, sleep/wake

## Milestone W2 — genuinely dependable Windows beta
- [ ] Daily, weekly and custom repeat rules with DST-safe recurrence
- [ ] Edit reminder and multiple urgency levels
- [x] Windows startup opt-in (toast fallback remains pending)
- [ ] Quiet hours, fullscreen/focus assist sensitivity, configurable popup location
- [ ] More polished final 2D pet asset and animation states (idle, arrive, speak, leave)
- [ ] Scalable reminder queue, event ledger and database migration to SQLite
- [ ] Multi-monitor and DPI handling; benchmarks and diagnostic logs
- [ ] Update mechanism and abuse/privacy review

## Milestone A1 — Android foundation (only after Windows reliability gate)
- [ ] Android Kotlin project, Room entities, reminder creation/editing
- [ ] Exact-alarm eligibility/permission flow and notification fallback
- [ ] Boot rescheduling and lock-screen policy-safe display
- [ ] Pet overlay when explicitly authorized; TTS tests
- [ ] Device matrix covering Pixel, Samsung and budget devices

## Milestone A2 — Android beta
- [ ] On-device pet animations and accessible notification actions
- [ ] Battery constraints tests, no-duplicate occurrence ledger
- [ ] Optional export/import before eventual sync
- [ ] APK beta and store-policy review

## Deliberate scope control
No account, no backend, no AI APIs, no purchase flows, no child-themed RPG mechanics, no always-on mascot and no iOS work until Windows/Android experience has strong evidence of reliability.

## Milestone W1.1 — attention recovery (v0.3)
- [x] User-visible count of missed and unanswered reminders
- [x] Snooze missed/unanswered reminder from dashboard
- [x] Tray balloon for newly missed reminder batches (subject to Windows settings)
- [x] Requeue unanswered popup when desktop locks; present after unlock
- [x] Keep popup consistent with user actions in dashboard
- [ ] Real-device confirmation of focus, screen lock and notification behavior

## Milestone W1.2 — floating visual companion (v0.4)
- [x] Independent 2D vector pet with no rectangular enclosing notification card
- [x] Short sliding arrival, looping gentle hover and tail motion, animated exit
- [x] Compact reminder speech bubble with Done / Snooze / Hide controls
- [x] Popup positioning away from video controls for large active windows
- [x] Hover pauses popup timeout; hidden reminders remain Needs Attention
- [ ] Hands-on Windows QA for UI, screen scaling, fullscreen and speech
- [ ] Focus/quiet modes, user-selectable popup placement and mixed-DPI multi-monitor placement

## Milestone W1.3 — reliability & observability (v0.5)
- [x] Privacy-preserving bounded local diagnostics (no reminder titles in logs)
- [x] Dispatcher-timer error boundary and storage retry cooldown
- [x] Roll back in-memory changes on failed persistent writes
- [x] Generic tray fallback if pet popup rendering fails (not guaranteed by Windows)
- [x] Speech test button and stop speech on lock/suspend
- [x] Regression checks for blocked writes, rollback and diagnostics privacy
- [x] Windows CI compile and 32 core checks for v0.5
- [ ] Physical laptop validation: popup and speech on/off, lock/unlock, sleep/wake, startup, duplicate alert prevention

## Milestone W1.4 — v0.6 reminder editing & recurrence
- [x] Additive schema preserving older one-time JSON
- [x] Daily and weekly (same weekday) scheduling using saved timezone / local wall time
- [x] Separate historical occurrences and one future repeat on delivery or missed
- [x] Guard against duplicate next schedule on snooze/second delivery
- [x] Edit upcoming unsent reminder title, date, time and repetition
- [x] Repeat picker, edit/cancel controls and repeat labels in dashboard
- [x] Regression tests for DST, long downtime, legacy JSON and failed recurrence writes
- [x] Windows CI compiled and 49 core checks passed; PR #5 merged into main
- [ ] Device QA: repeat, edit, tray restart, sleep resume, voice and multiple monitors
- [ ] Custom weekly days, series-level history management and richer Today/Upcoming view (subsequent phase)

## Milestone W1.5 — pet experience (v0.7)
- [x] Independent local pet preferences with safe defaults and atomic saves
- [x] Select Auto, Top/Bottom Right/Left popup placement
- [x] Full, Gentle, Off motion settings, supporting reduced movement
- [x] Blinking eyes, Windows TTS event-driven speaking mouth, and brief successful Done/Snooze reactions
- [x] Use foreground monitor positioning with WPF pixel/DIP conversion
- [x] Test preference persistence, invalid/corrupt settings and rollback
- [x] Final Windows CI verified with 56 core checks; PR #6 merged into main
- [ ] Physical QA: appearance controls, blink/speaking, mixed DPI, lock/wake
- [ ] Alternate pet species/asset packs and full walking sprite animations (later work)

## Milestone W1.6 — Windows polish and packaging (v0.8)
- [x] Reminder list filters: All, Today, Upcoming, Needs Attention, History
- [x] Empty states, compact action layout, accessibility names and keyboard shortcuts
- [x] Confirm destructive Delete and explain next repeated occurrence effect
- [x] Per-user Windows installer script preserving `%LOCALAPPDATA%\PetPing`
- [x] CI installer build plus install/uninstall storage preservation smoke test
- [x] Pure filter regression tests in core smoke project
- [x] Windows CI green: 64 core checks, portable ZIP + installer and silent install/uninstall data-preservation test; PR #7 merged
- [ ] Real-device font scaling, multi-monitor, speech, lock and sleep/wake acceptance
- [ ] Official code signing and automated upgrades (not implemented in v0.8)

## Milestone W1.7 — v0.9 recovery and Windows beta-quality gate
- [x] Explicit opt-in validated backup restore; preserve unreadable primary
- [x] Do not silently overwrite corrupted files; warn earlier backup may miss recent edits
- [x] Dashboard opens local data/diagnostics folder for manual backup and inspection
- [x] Core regressions for valid/invalid backup and non-destructive recovery
- [x] Installer same-version repair/reinstall smoke (not a cross-version test)
- [x] GitHub Windows CI green: 72 core checks, portable ZIP and installer, reinstall/uninstall data smoke; PR #8 merged
- [ ] Real laptop test recovery with test data, installer upgrade from v0.8, lock/sleep and mixed DPI
- [ ] Windows beta/v1.0 release gating: signer, accessibility and actual device reliability

## Milestone W1.8 — Windows v1.0 RC / final release quality
- [x] v0.9 broad hands-on experience reported working by user (2026-10-16); detailed scenario results not individually logged
- [ ] Feature freeze, v1.0 RC branch and consistent application/installer version identity
- [ ] Original app icon, distribution metadata and coherent Windows startup/shortcut UX
- [ ] CI simulated **v0.9 → RC upgrade** using a saved-data fixture (not just same-version reinstall)
- [ ] Release notes, privacy/offline data note, troubleshooting and backup guidance
- [ ] Windows RC ZIP + installer, checksum, tests and tagged GitHub pre-release
- [ ] Physical RC checks for lock/sleep/wake, multi-monitor DPI, voice, keyboard/assistive technology and install/uninstall
- [ ] Explicit distribution/code-signing decision (unsigned limitations disclosed)
- [ ] User sign-off for stable v1.0; then create Android A1 foundation

## Milestone W1.9 — PetPing 2.0 experience (approved before Windows v1.0)
- [x] Stable, user-tested v0.9 preserved as baseline
- [x] v0.10 new Home/Reminders/My Pets/Pet Studio/Settings design specification
- [x] v0.10 walking, speaking, waiting, reaction and exit storyboard
- [x] v0.10 versioned design tokens and example pet-profile schema
- [x] v0.10 editable seven-page Canva concept created
- [ ] Review and correct Canva pet-care placeholder copy and speech timing
- [x] User approved Canva visual direction and authorized v0.11 work
- [x] v0.11 source: walking WPF pet, unboxed reminder text, Done/Snooze/Mute/Hide and cancellable repeated narration (CI/real-device QA pending)
- [ ] v0.12 expressive animations and action-specific effects
- [ ] v0.13 Cat/Dog/Bunny profiles and per-pet customization
- [ ] v0.14 full WPF dashboard/UI rework, keep reminder state compatibility
- [ ] v0.15 Windows QA, then Windows v1.0 release candidate

### v0.11 acceptance gates
- [x] Pure movement planning and cancellation rules; regression smoke tests added
- [x] New pet-sized WPF walking animation and no filled text bubble in source
- [x] Session-tagged local speech repeat after 60s of silence; interrupted on action/lock/suspend
- [x] Windows WPF / Installer CI passed with 86 checks; PR #10 merged
- [ ] User verifies screen-sized walking, text legibility, correct controls and voice on physical Windows device

## Revised PetPing 2.0 schedule — user-approved priority shift
- [x] v0.12 source: Home/Reminders/My Pets/Pet Studio/Settings real WPF navigation and cohesive visual styling
- [x] v0.12 Home counts/next reminders driven by actual saved data, with deterministic core tests
- [x] v0.12 retain existing reminder editor, filters, voice, start-at-login, pet appearance and tray flow
- [x] v0.12 honestly mark Dog/Bunny and behavior customization as coming soon, without fake state controls
- [x] v0.12 Windows CI green (91 checks), installer preservation and PR #11 merge
- [ ] v0.12 physical Windows layout, navigation, high DPI, voice and reminder acceptance
- [ ] v0.13 native Cat/Dog/Bunny characters, profile storage and genuine customization with previews
- [ ] v0.14 walk/action effects, pet-specific reactions, celebration and snooze animations
- [ ] v0.15 integration QA, accessibility, DPI, Windows upgrade data retention and perf
- [ ] Windows v1.0 release candidate after user approval. Android remains next platform.
