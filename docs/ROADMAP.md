> **CURRENT LOCKED PLAN (2026-10-16):** See [v0.16–v0.21 Delivery Plan](V016_DELIVERY_PLAN.md), [Product Blueprint](V016_PRODUCT_BLUEPRINT.md), [Reminder Spec](V016_REMINDER_SPEC.md), [Character Bible](V016_CHARACTER_EXPERIENCE.md), and [Brand/Onboarding](V016_BRAND_ONBOARDING.md). Older W0/W1 milestones below are historical and may retain outdated checkboxes from prior roadmap iterations.

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

## v0.13 — genuine customizable pets
- [x] Versioned per-species local pet roster; individually saved looks and actions, atomic saves, validation, safe fallback, separate from reminders
- [x] Three original vector Cat/Dog/Bunny rigs shared by UI and real Windows overlay
- [x] Pet selection, four colors, accessories, size, personality and Done/Snooze actions
- [x] Configurable narrated repeat interval 30/60/120s wired to real speech engine
- [x] Core regression tests for profile persistence, corruption, invalid settings and failed writes
- [x] Windows CI green 105 checks, PR #12 merged, both unsigned artifacts verified
- [ ] User verifies species visuals/selection, Studio, live reminder, restart and audio timing on laptop
- [ ] v0.14 sophisticated species-specific character animation and particles

## v0.14 — expressive actions and safe effects
- [x] Deterministic species-specific gait and idle timing rules
- [x] Cat tail/steps, puppy trot/wag/ears, bunny hop/ears animations
- [x] Done Hop/Wave, Snooze Yawn/Stretch wired after saved reminder action
- [x] Finite species-themed star/sparkle effects with <=10 particles
- [x] Per-species Off/Gentle/Playful effects and global Motion Off/Gentle override
- [x] Pet Studio non-destructive reaction previews and safe reset/cleanup
- [x] Pure regression checks for motion/legacy settings, no reminder data change
- [x] v0.14 Windows WPF/test/installer CI passed 122 checks, PR #13 merged and unsigned ZIPs verified
- [ ] v0.14 on-device cat/dog/bunny rendering, effects, accessible controls and power QA
- [ ] v0.15 integration/battery/multi-DPI and Windows release QA

## v0.15 — Windows integration and upgrade reliability
- [x] v0.14 user reports general functionality good; detailed test case evidence still unrecorded
- [x] Stop active TTS + repeat timer immediately on voice Off
- [x] Separate durable Done/Snooze writes from dashboard refresh failures
- [x] Keep Done/Snooze actionable and show inline text if write fails
- [x] Pet action button keyboard focus outline and consistent v0.15 version labels
- [x] CI true prior v0.14→v0.15 upgrade verification defined, including synthetic reminder/pet profile SHA preservation
- [x] v0.15 full CI green (122 checks), real v0.14→v0.15 upgrade test passed, both unsigned archives CRC-verified, PR #14 merged
- [ ] v0.15 hands-on reminder/voice/reactions/accessibility/multi-monitor/power testing
- [ ] Windows v1.0 RC checklist, code signing/distribution decision and user approval

## Locked expansion — current delivery program v0.16 to v0.21
- [x] User-approved direction: premium brand/UI, reminder engine 2.0, character rigs/complete performance, setup/onboarding/guidance, reliability; Windows first then Android
- [x] v0.16 PRD, screen map, P0/P1/P2 backlog and quality targets
- [x] v0.16 reminder recurrence, priority, snooze and migration specification
- [x] v0.16 character art and full reminder-scene quality contract
- [x] v0.16 logo exploration / onboarding brief and design-token draft
- [x] v0.16 sequenced delivery plan with explicit exit gates and risks
- [x] v0.16 merged design-only PR #15 and Windows CI passed 122 legacy checks; no new EXE expected
- [ ] v0.17 original logo/icon concept families and high fidelity WPF UI visual designs; user art review
- [ ] v0.17 native UI component system, accessibility and responsive layout, Windows CI and manual test
- [ ] v0.18 stronger snooze/recurrence/priority/categories/quiet modes in gated slices; backwards-compatible migrations
- [ ] v0.19 approved original character artwork/rigs and smooth complete reminder scene, performance QA
- [ ] v0.20 branded welcome, skippable first-run setup and contextual guidance with upgrade safety
- [ ] v0.21 integrated mixed-DPI, sleep, lock, screen reader, memory/CPU, installer upgrade QA
- [ ] Windows v1.0 RC and stable with explicit signing/distribution decision and user acceptance
- [ ] Android native A1 only after Windows release gate

## v0.17 Premium Brand & UI — new implementation plan
- [x] Locate and preserve earlier 7-page Canva concept
- [x] Draft 3 source-editable ORIGINAL SVG identity candidates, not yet selected
- [x] Create new 8-page editable Canva design review, audit and document off-target generated copy
- [x] Create precise Home/Reminders WPF layout and interaction spec, plus original 1440×900 editable SVG screen mockups with explicit sample-watermark
- [x] Refine semantic UI tokens; flag failing mint/white color contrast
- [ ] User visual approval of logo A/B/C, general UI language and high-fidelity layout
- [ ] Stage B final logo master/ICO and coherent WPF resources, Home & Reminders implementation with real reminder data
- [ ] Stage C My Pets/Studio/Settings full UI polish; empty/error/loading/responsive/accessibility states
- [ ] Windows CI and physical 100/125/150/200 DPI QA before calling v0.17 complete
- [ ] v0.18 advanced reminder system per locked roadmap
