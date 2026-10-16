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
- [ ] Confirm Windows CI and actual unsigned installer artifact
- [ ] Real-device font scaling, multi-monitor, speech, lock and sleep/wake acceptance
- [ ] Official code signing and automated upgrades (not implemented in v0.8)
