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
