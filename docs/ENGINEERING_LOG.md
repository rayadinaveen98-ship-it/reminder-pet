# Engineering work log

Keep this append-only by milestone. Link commits and CI runs; note failures as well as passes.

## 2026-10-16 — v0.5 kickoff
- Owner request: continue reliability roadmap and **maintain GitHub progress and an explicit handoff** so a new ChatGPT conversation can resume cleanly.
- Start branch: `feature/windows-v0.5-reliability`.
- Wrote `docs/PROJECT_STATUS.md` and this log.
- v0.4 latest verified baseline: CI Windows compiled, 21 core tests passed; user verified floating pet over apps. Not yet manually tested: lock/wake, multi-monitor, voice consistency.
- Next: reliability implementation, PR, Windows CI, artifact and documentation refresh.

## 2026-10-16 — v0.5 implementation checkpoint
- Committed initial reliability source to the feature branch: `3e4453a3ec2000df83836e9d9abaa883e5644f71`.
- Added local diagnostics with no titles/message content, scheduler error isolation + 30-second disk retry, store rollback on failed writes, TTS test and stop-on-lock/suspend, and generic fallback when popup rendering fails.
- Expanded smoke checks to cover inaccessible-write simulation, retryable unchanged reminder state, diagnostics privacy and simultaneous reminder IDs. Updated documentation/roadmap and CI artifact name.
- CI has **not yet been confirmed** at this checkpoint. Physical Windows tests remain open.

## 2026-10-16 — v0.5 CI verification
- Pull request: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/4 .
- Windows Actions run: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38864475272 — **success**.
- Core smoke checks: **32 passed** (previous baseline 21). Includes fault-injected storage write denial, state rollback, diagnostic privacy, and same-time reminder identity.
- Windows x64 .NET 10 self-contained publish **succeeded**; unsigned alpha ZIP uploaded as `PetPing-Windows-v0.5-unsigned` (artifact ID 12029599446).
- Do not equate CI pass with actual Windows sleep, locked session, spoken voice or multi-monitor behavior. User testing remains pending.
- Next: synchronize handoff, merge only after green CI, share the alpha ZIP and collect physical test outcomes.

## 2026-10-16 — v0.5 merged and handoff finalized
- Documentation-only final CI run: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38864751672 — green, 32 core checks passed, Windows x64 publish and unsigned ZIP uploaded.
- Merge: PR #4 squash to `main` at commit `4bf88f2ba8329435c760c8c10af3817f199f9a99`.
- v0.5 package: `PetPing-Windows-v0.5-unsigned.zip` (artifact 12029754579); ZIP verified readable with executable inside. The ZIP is not an installed/signed Windows package.
- Required next step: physical Windows QA for tray/voice/lock/sleep and report any failures. Do not mark manual testing as done by inference.
- User's continuity requirement: this log + `docs/PROJECT_STATUS.md` and README are the durable reference for future chats. Update them whenever milestone code/test/status changes.

## 2026-10-16 — v0.6 kickoff
- User approved proceeding after v0.5 and said that the test is good; detailed locked-screen/sleep/voice device-by-device evidence is not yet recorded, so the production reliability gate remains open.
- Scope: Windows-first edit **pending** reminders and repeat daily/weekly; retain individual occurrence history and prevent duplicate future occurrences. The first implementation has no custom weekly-day selection or multi-device sync.
- Plan: add a recurrence engine with timezone-aware local wall time, atomic occurrence advancement during delivery/miss; UI edit/create controls; regression tests including restart, long downtime and DST. Run Windows CI, review and merge. Update PROJECT_STATUS and ROADMAP on completion.

## 2026-10-16 — v0.6 recurrence/edit implementation checkpoint
- User approved moving to next phase and wants every meaningful progress checkpoint in GitHub for smooth cross-chat continuity.
- Branch: `feature/windows-v0.6-edit-recurrence`. Model + recurrence and atomic persisted store: commit `66dd1a1c411b90a6ecf0bd8321522effd60fdc56`. Dashboard create/edit picker and controls: commit `fec743e2a611283f378d08d608bca8b6b2fe0b07`.
- Design: each delivery/miss creates one *new* future record transactionally; prior occurrence remains in history. Old snoozed occurrences retain continuation marker; long offline gaps jump forward rather than producing unbounded missed instances.
- Local wall-clock recurrence stores zone ID and time of day. Spring gaps use next valid minute, autumn ambiguity earlier instant. Existing old JSON defaults to one time.
- Added tests for daily/weekly, edit/cancel recurrence, DST, historical records, rollback and migration. CI/build pending at this checkpoint; do not state passed until verified.

## 2026-10-16 — v0.6 first CI verification
- PR #5: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/5
- Windows x64 Actions run: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38868473793 — **success**.
- Static XAML, C# core regression and dotnet publish passed; **49 core tests** passed (vs. 32 in v0.5).
- Unsigned artifact PetPing-Windows-v0.6-unsigned uploaded (artifact ID 12032022285). Physical desktop QA is not inferred from CI.
- Next: merge PR #5 after handoff refresh, share test build, gather editing/repeat/voice/lock results.

## 2026-10-16 — v0.6 merged + ZIP verified
- Final PR CI: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38868752195 — green, 49 core tests, Windows x64 compile/publish and artifact upload.
- Merged PR #5 to `main`, squash commit `f75fef016eb650e25fc9d40248c73ec61d6208ca`.
- Final artifact `PetPing-Windows-v0.6-unsigned` ID 12032645056. Downloaded ZIP locally and validated its archive integrity; includes `PetPing.Windows.exe` (unsigned test build).
- No evidence yet of actual user interaction testing of the new v0.6 repeating/edit screens. Await results rather than claim production quality.
- Next: v0.6 Windows edit/recurrence acceptance QA; triage failures before v0.7 character animations.

## 2026-10-16 — v0.7 kickoff
- User reports Windows v0.6 is working fine. Detailed lock/sleep, multi-monitor and speech regression evidence remains unrecorded, so those formal release gates remain open.
- Target: make the existing orange-cat companion more expressive without altering reminder schema or recurrence: blinking/speaking feedback, reduced-motion mode, user-selectable corner placement, and safe settings persistence.
- Keep Windows first, Android second, local-first, zero-cost software/hosting. Commit code and resume documents, run green Windows CI, then merge and share the unsigned alpha.
- Branch: `feature/windows-v0.7-pet-experience`; PR and CI pending.

## 2026-10-16 — v0.7 engineering checkpoint
- Feature branch: `feature/windows-v0.7-pet-experience`.
- Initial commits: pet appearance preference store; vector-cat blink/mouth elements; WPF speech-linked expression, reduced-motion modes and monitor-corner positioning; app/dashboard integration.
- Settings are local and independent of the reminder JSON. Existing daily/weekly engine intentionally unchanged.
- Added smoke checks for preference persistence, invalid enum, disk write rollback and corrupt settings isolation.
- Windows compile and hands-on appearance tests pending at this checkpoint. Next: PR, CI, bug fixes, status refresh, merge.

## 2026-10-16 — v0.7 first CI and action safety refinement
- Pull request #6: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/6 .
- Initial Windows CI run https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38872180216 **passed**: 56 core tests, compiled executable, uploaded unsigned alpha.
- Subsequent safety fix: pet Done/Snooze now get explicit success from the persistence callbacks. On save failure pet remains actionable rather than closing as if completed. On success the pet briefly replies with a friendly acknowledgement before animating away.
- Final CI for the additional code change not yet verified at this checkpoint; do not merge until green.

## 2026-10-16 — v0.7 final CI, merge and Windows ZIP
- PR #6 merged: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/6 ; squash commit `3115b84ddea107fdd8606e45d187374866470a9e`.
- Final Actions run: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38872547917 — **success**, 56 core smoke checks; static XAML checks, Windows x64 compilation and unsigned ZIP upload passed.
- Artifact: `PetPing-Windows-v0.7-unsigned`, ID 12034680037. Downloaded ZIP, checked archive integrity, verified `PetPing.Windows.exe` present.
- Includes blink, speech-mouth, local pet placement/motion settings, and saved-action-only Done/Snooze confirmation. Existing reminder recurrence untouched.
- Hands-on physical testing remains open (animation visuals, voice timing, mixed-DPI monitors, lock/sleep). Do not treat passing CI as proof of those scenarios.
- Next: user v0.7 testing, P0 defects, then v0.8 Windows polish/packaging before Android.

## 2026-10-16 — v0.8 kickoff
- User confirmed v0.7 is good and approved proceeding. Do not infer that separate lock/wake, audio and mixed-DPI device acceptance tests passed.
- Branch `feature/windows-v0.8-dashboard-installer`. Scope: reorganize reminder inbox into readable Today / Upcoming / Needs Attention / History / All filters, friendlier dashboard spacing and keyboard/accessibility helpers. Preserve v0.7 pet, preferences, recurring scheduling and local saved reminder data.
- Distribution: add a per-user **unsigned** Inno Setup installer via Windows GitHub CI alongside the existing portable ZIP. The installer must never erase reminder data on uninstall. Avoid admin rights, paid certificate requirements, auto-updater promises or silent background actions.
- Next: commit source and CI pipeline, run Windows build, fix errors, test installer extraction/build, update docs/QA and merge only when green.

## 2026-10-16 — v0.8 implementation checkpoint
- Branch: `feature/windows-v0.8-dashboard-installer`.
- Implemented Today/Upcoming/NeedsAttention/History/All inbox filters, empty views, keyboard shortcuts, wrapped actions, accessible labels and confirmation before delete. Reminder scheduler and recurring data unchanged.
- Added first per-user Inno Setup package script (`installer/PetPing.iss`), Windows CI build, plus silent install/uninstall test verifying saved reminder data survives. Includes conversion of prior opt-in login startup path to installed executable, and avoids admin rights.
- Regression tests for filter membership/order and no mutation; v0.7 core tests remain. CI pending at this checkpoint. Do not claim installer works until verified on Windows runner.

## 2026-10-16 — v0.8 first installer build, fixes and final merge
- PR #7: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/7 ; squash commit on main `fc33f2264141ae71f20f582e1b9ad4bfd9344592`.
- Initial Windows run #38877154997 failed a too-specific test assertion about whether Missed or Delivered sorts first; both are Needs Attention, and the assertion was corrected.
- Run #38877419848 compiled core tests but WPF caught ambiguity between WinForms/WPF `KeyEventArgs`; fixed the fully qualified event type.
- Run #38877607535 passed core tests, compiled app and installer; installer smoke incorrectly read an empty PowerShell `$LASTEXITCODE` from a GUI process. Fixed using `Start-Process -Wait -PassThru` for install/uninstall.
- **Final green run:** https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38877847189 — 64 core tests passed; WPF win-x64 published; per-user installer built; silent setup/uninstall and persisted data sentinel test passed; both artifact uploads succeeded.
- Portable artifact `PetPing-Windows-v0.8-unsigned` (ID 12035474939; ZIP contains `PetPing.Windows.exe`). Installer artifact `PetPing-Installer-v0.8-unsigned` (ID 12036600668; ZIP contains `PetPing-Setup-v0.8-unsigned.exe`). Both ZIPs integrity checked in the active working environment.
- The unsigned installer is **not** a signed or SmartScreen-approved consumer release. Real interactive installation, mixed DPI, lock/sleep and accessibility testing still open. Uninstall smoke verifies that user data stays in LocalAppData.
- Next: user v0.8 device acceptance, P0 fixes, v0.9 Windows quality/beta gate, Android second platform after Windows release maturity.

## 2026-10-16 — v0.9 kickoff (Windows beta quality)
- User asked to proceed following v0.8. No detailed v0.8 manual installer, screen reader or sleep/wake evidence supplied; keep these as open acceptance gates.
- Branch: `feature/windows-v0.9-recovery-quality`.
- Release focus: protect reminders if the primary JSON becomes unreadable; offer **explicit opt-in restore from validated previous backup**, preserving the unreadable original for inspection. Add a clear **Open data folder** tool. Improve CI to test installer repair/reinstall without deleting personal data.
- Design constraints: never silently restore or wipe files; backup may be older than latest reminder and restore can lose newest edits, so confirmation must be clear. No cloud sync or account. Keep existing reminder model/pet/recurrence intact.
- Next: commit incremental source and tests, Windows CI, update handoff and merge only after green tests and install smoke. This does not automatically satisfy physical screen reader, lock/sleep and mixed DPI QA.

## 2026-10-16 — v0.9 code and regression checkpoint
- Added ReminderStore backup validation and an explicit, opt-in restore routine that saves the unreadable primary under a unique `reminders.unreadable-*.json` name while retaining the prior `.bak` file.
- Windows startup asks a visible Yes/No recovery question only for unreadable primary + valid backup. Warns the backup can be older and newer entries might be missing; cancel leaves files unchanged.
- Dashboard includes **Open data and diagnostics folder** to let users inspect and copy local JSON/logs when troubleshooting. No automatic collection/upload.
- Added deterministic regression scenarios: backup eligibility, damaged original preserved, earlier version restored, invalid backup refused. Installer CI repair/reinstall now runs a second install before uninstall to test data preservation.
- Reminder schema and pet design remain unchanged. PR and CI still pending at checkpoint.

## 2026-10-16 — v0.9 final build, merge and verified packages
- PR #8 merged: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/8 ; squash commit `946e90be775afda0b06946f4b5331761942b9f58`.
- Green GitHub Actions run: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38881511852 — **72 core smoke checks passed**, XAML validation passed, WPF Windows x64 compiled.
- Inno Setup installer compiled. Windows CI silently installed, *reinstalled the same version*, uninstalled and verified the LocalAppData sentinel remained intact. This is NOT proof that an actual v0.8→v0.9 upgrade works on user's device.
- Portable artifact: `PetPing-Windows-v0.9-unsigned` (ID 12038021245); installer artifact: `PetPing-Installer-v0.9-unsigned` (ID 12037687579). Downloaded both, checked ZIP archive CRCs, verified `PetPing.Windows.exe`, extracted `PetPing-Setup-v0.9-unsigned.exe` as standalone installer (~50 MB).
- Backup feature is explicitly opt-in and warns earlier backup may omit recent entries; restores only a validated `.bak` and keeps unreadable primary. No cloud uploads.
- Physical Windows acceptance still pending: on-screen recovery prompts, real cross-version installer upgrade, Windows SmartScreen, accessible font/DPI, lock/sleep/multi-monitor and voice interactions. Do not call this production-ready.
- Resume from `docs/PROJECT_STATUS.md` and this engineering log on main. Next: user QA, P0 defect fixes or Windows v1.0 quality milestone; Android after Windows release gate.

## 2026-10-16 — user-reported v0.9 acceptance; v1.0 RC plan
- User said **"yeah all working good bro tested everything"** after receiving v0.9. Record as a broad positive user-device report, not as independent test logs for every unenumerated scenario.
- No v0.9 defect reported; proceed to v1.0 **release candidate** planning, not directly to Android or a declaration of certified stable release.
- Release-candidate feature freeze: preserve reminder scheduling/recurrence and pet UI; focus on versioning, branding/icon polish, installer upgrade from the previous real version, privacy documentation, accessibility QA, repeatable Windows artifact builds and signing/distribution strategy.
- The signing certificate and Windows SmartScreen reputation are unresolved; an unsigned personal alpha can work without treating it as ready for frictionless public distribution.
- Added `docs/V1_RELEASE_CHECKLIST.md` with measurable acceptance gates. Next step is a new release-candidate development branch; **no RC executable exists at this checkpoint**.

## 2026-10-16 — PetPing 2.0 design phase, user-approved
- The user requested and approved replacing the fixed reminder popup with a walking pet from a random screen edge to the mid-screen zone, repeated spoken reminders until action, unboxed text, new celebration/interaction effects, additional customizable pets and complete dashboard UI redesign.
- Kept tested Windows v0.9 as baseline and created separate design-only branch `design/pet-experience-2-phase-1`.
- Authored UI/architecture product specification, walking and voice state storyboard, design tokens, and proposed local pet profile JSON with Cat, Dog and Bunny actions.
- Canva produced **7 editable pages** at https://www.canva.com/d/26YBWLCIc7nE3nk . Canva imagery is a concept; some generated pet-care examples and voice-repeat timing labels require correction before it becomes implementation reference.
- Figma connected account reports Starter/View seat; chose the available Canva editing workflow without a paid upgrade.
- No new WPF feature code or compiled version generated yet. Design review is the next gate; v1.0 RC deferred until pet experience milestone is finished.

## 2026-10-16 — v0.10 Phase 1 design merged
- PR #9: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/9 ; squash merge `9e6c70b3084071d1bd77b30d4bd7b0dfaa1a1098` to `main`.
- GitHub Actions: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38888573514 — **success** (existing 72 reminder-core checks, WPF publish, installer compile, same-version reinstall/uninstall data persistence check). No production code changes in this design-only PR.
- Editable Canva UI concepts: https://www.canva.com/d/26YBWLCIc7nE3nk , seven pages. Specs/Storyboard/Design Tokens/Example Pet Profile now on `main`.
- DESIGN REVIEW PENDING: autogenerated Canva pet-care example text and minutes cadence need correction. User signoff required before UI styles are locked. v0.11 moving pet/TTS loop engine is not started.

## 2026-10-16 — v0.11 walking actor engineering checkpoint
- User approved the Phase 1 Canva UI direction and authorized implementation.
- Created branch `feature/windows-v0.11-walking-narration` from main. Replaced old bubble with unboxed reminder words and compact Done/Snooze/Mute/Hide buttons in PetWindow.
- Implemented window.Left walking from a monitor edge to its center and alternating foot movement. On arrival display text and begin voice. Gentle glide/Off static respects prior motion settings.
- Added speech session tagging in VoiceService, a single 60-second post-utterance dispatcher repeat timer, speech cancellation on action/hide/mute/sleep/lock and safe stale-callback filtering.
- Existing reminder storage and recurrence preserved. Corrected stale queued deleted reminder edge case before displaying pet. Added pure geometry/speech tests. New visual modes do NOT mean multi-pet or full dashboard redesign are implemented yet.
- Windows CI and physical usability tests pending at this checkpoint. No v0.11 user QA claimed.

## 2026-10-16 — v0.11 Windows CI, merge and packages verified
- PR #10: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/10 merged to main at squash commit `caf61816534e6bb2fd2cc0eaa6c6bdbea2d8bcd5`.
- Windows Actions https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38890848806 — **success**; **86 core smoke checks passed**, static XAML verified, Windows x64 WPF compiled/published; Inno Setup compiled and silent install/reinstall/uninstall preserved CI data sentinel.
- Portable artifact `PetPing-Windows-v0.11-unsigned` ID 12042450420; installer artifact `PetPing-Installer-v0.11-unsigned` ID 12041443215. Both GitHub ZIPs downloaded and checked for CRC corruption. Standalone `PetPing-Setup-v0.11-unsigned.exe` extracted from installer ZIP (~52 MB).
- Walking/unboxed text and speech-repeat features are implemented, but actual Windows full-window transparency hit-testing, visual layout and voice timing **not yet physically observed**. User v0.9 working baseline remains backed up; no data migrations.
- Next: user tests v0.11; fix issues, then v0.12 celebration and action effects. Leave v1.0 deferred and Android after Windows maturity.

## 2026-10-16 — v0.12 UI redesign implementation checkpoint
- User accepted the new Canva direction and requested implementation earlier than originally scheduled. Revised order: v0.12 UI, v0.13 multi-pet, v0.14 expressive actions/effects, v0.15 integration QA, Windows v1.0 later.
- Branch `feature/windows-v0.12-navigation-redesign` created from current main; preserves v0.11 walking/voice and all saved reminder schemas.
- Implemented new WPF app-wide ivory/lavender theme, sidebar navigation, Home live metrics and upcoming list, a restructured functional Reminders page, My Pets gallery, early Pet Studio, and Settings.
- Added original reusable WPF vector CatPortrait. Only Milo the Cat is selectable today; Dog and Bunny are future scope, clearly labelled rather than faked. Existing arrival and Playful/Gentle/Off motion preferences remain functional via Pet Studio.
- Added DashboardSummaryRules and deterministic smoke tests for accurate Home counts and no fake data. Installer/portable artifact bumped to v0.12.
- Windows compilation, executable output and hands-on user testing PENDING at this checkpoint.

## 2026-10-16 — v0.12 final Windows build and merge
- PR #11 https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/11 merged to `main`, squash commit `8d5b0ba00d41aea2d76e94ea4de86361a3c0076a`.
- First 2 CI builds caught WPF/WinForms type ambiguity in CatPortrait.UserControl and WPF Color; corrected fully qualified WPF types. Third run **success**: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38893597316 (91 core tests, WPF win-x64 publish, Inno Setup compile, install/reinstall/uninstall preservation smoke).
- Portable ZIP artifact `PetPing-Windows-v0.12-unsigned` (12042344606), installer ZIP artifact `PetPing-Installer-v0.12-unsigned` (12043370603). Both downloaded and ZIP CRC verified; installer setup extracted `PetPing-Setup-v0.12-unsigned.exe`, SHA-256 `53a381461f24996d7f93dc979a31b435cc65742892cca1a93250f66492222dd6`.
- Updated Home/Reminders/My Pets/Pet Studio/Settings UI now compiles; dog and bunny still future, current cat remains. Real laptop UI/Windows accessibility and DPI testing not claimed passed by CI.
- Next: user hands-on v0.12 validation, remediate UI/runtime bugs, then v0.13 real multi-pet characters + customization and v0.14 expressive reactions.

## 2026-10-16 — v0.13 multi-pet customization engineering checkpoint
- Created `feature/windows-v0.13-multipet-customization` preserving merged v0.12 Windows baseline.
- Added `PetProfileSettings` local JSON with species, palette, accessory, size, personality, Done/Snooze action types and 30/60/120-second voice repeat; atomic writes and safe fallback. Does not change legacy appearance settings or reminders.
- Added original `PetCharacter.xaml` component supporting distinct Cat/Dog/Bunny visual silhouettes plus live palette, accessory, size, blinking, speaking and basic reactions. Shared by UI previews and actual moving PetWindow.
- Connected My Pets buttons to persisted species selection and Pet Studio controls to saved preference updates. Injected saved pet into real App/voice cadence, not just a mockup. Update applies to next reminder and preview.
- Added persistence/failure/corruption regression smoke checks and v0.13 artifact/version names.
- CI and laptop tests pending. v0.14 remains advanced particle effects and deeper animal reactions.

## 2026-10-16 — v0.13 per-pet personalization refinement
- Clarified the persistence model: Milo, Buddy and Luna each retain **their own** palette, accessory, size, personality and actions when user switches away and back; last selected species also persists.
- Modified the profile JSON to hold a validated species roster rather than one shared active profile. Added switch-and-restart regression tests and safely resized preview artwork through WPF Viewbox so no ears/feet are cropped in cards.
- First v0.13 CI run #38896987114 passed, but these additional per-pet edits require a new green run before merge and distribution.

## 2026-10-16 — v0.13 final CI, merge and download package
- PR #12 https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/12 merged into main at squash commit `6410b4400651c8a6ac473dafa6331aa8f0f1217c`.
- Final green GitHub Windows run https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38897525278: **105 core regression checks** passed, WPF app compiled, Inno Setup installer built, silent install/reinstall/uninstall preserved a LocalAppData data sentinel.
- Portable ZIP artifact `PetPing-Windows-v0.13-unsigned`, ID 12044223333; installer ZIP artifact `PetPing-Installer-v0.13-unsigned`, ID 12045065431. Downloaded and CRC verified both. Extracted `PetPing-Setup-v0.13-unsigned.exe` (51,926,601 bytes), SHA-256 `fbb7e06bac08ec1f5d600a529a127cba289759b213d26c953b56f5e40c71944f`.
- Now supports separate per-species profiles: selected pet and saved appearance/actions survive switching, restarting; actual moving WPF window receives the selected rig. Same default reminder and legacy appearance storage unaffected.
- Known limitation: CI does not visually inspect vector rigs, scaling or TTS timing on user's physical Windows machine. Test v0.13 before v0.14 advanced expressive pet effects.

## 2026-10-16 — v0.14 expressive animation branch checkpoint
- User approved proceeding from the v0.13 multi-pet implementation to v0.14 character movement and reactions.
- Created `feature/windows-v0.14-expressive-animations`; source now includes deterministic `PetAnimationRules` to budget gaits, reaction duration and at most 10 ephemeral particles per effect.
- Cat steps/tail, dog quick trot/tail wag/droopy ears, bunny hop/ear twitches have distinct animation timings. Done can hop or paw-wave with species-specific colored sparkles. Snooze yawns or stretches (including ear movements).
- Added v0.14 per-species Effects Off / Gentle / Playful setting with backwards-compatible read of v0.13 `pet-profile.json`; Motion Off suppresses all effects, Gentle uses short movement without particles.
- Added Pet Studio non-destructive Try Done / Try Snooze effect previews and guarded cleanup so a stale particle completion cannot affect a subsequent preview.
- Walk/speak/snooze/complete persistence remains in existing scheduler/actor; animation success plays **after** the database saves Done or Snooze. Popups and timers are cleaned on close or power/session events.
- Regression smoke checks for different gaits, effects budgets, invalid settings, round-trip and legacy compatibility. Windows CI and hands-on laptop QA pending at this checkpoint.

## 2026-10-16 — v0.14 complete and verified Windows packages
- PR #13 https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/13 merged on `main` as squash commit `4f8be7cae20651161651bbec444f412fa34902df`.
- Windows Actions https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38900092320 completed successfully: **122 core checks**, XAML static validation, WPF Windows x64 self-contained publish, Inno Setup compile, silent install/reinstall/uninstall and retained LocalAppData sentinel.
- Artifacts: `PetPing-Windows-v0.14-unsigned` (12045033789) and `PetPing-Installer-v0.14-unsigned` (12044899354); both archives downloaded and CRC checks passed. Extracted setup `PetPing-Setup-v0.14-unsigned.exe` (~52 MB); SHA-256 `47414e3f92db6c52f2f5c6887f996de98f5d77f8a7b671340571c1f3b235ec77`.
- Legacy v0.13 profile test passed with new default effects value; no reminder schema modifications. Previews change only visuals, not task state. Animated effects capped at 10 transient shapes; cleanup on exit and power/session shutdown.
- **Unverified:** actual movement aesthetics, WPF hit testing, keyboard screen reader, multiple monitors/DPI, real speech timing and Windows lock/sleep. User must test on laptop. Next v0.15 integrated release reliability.

## 2026-10-16 — v0.15 integration hardening implementation checkpoint
- User accepted v0.14 and authorized v0.15 focused reliability and final Windows integration before v1.0.
- Created `feature/v015-reliability`, preserving reminder data and 3 saved pet profiles without migration.
- Fixed voice toggle Off to cancel current Windows SpeechSynthesizer utterance **and** active reminder post-voice repeat timer; toggling back On cannot inherit an old repeat.
- Fixed source callback paths for actual pet Done/Snooze: durable store write is separated from best-effort dashboard/tray refresh, so a later presentation failure is not falsely returned as a storage failure. Failed store actions now show an inline retry message and keep controls actionable.
- Added visible high-contrast WPF keyboard focus border on reminder buttons, assembly v0.15 version metadata and installer 0.15.0 label.
- Expanded CI to rebuild pinned v0.14 original release source from Git history and execute a real v0.14→v0.15 installer upgrade with SHA-256 data checks for synthetic reminders and pet profiles; existing same-version preservation checks remain. CI has NOT yet passed at this checkpoint.
- No new pet species, app redesign or cloud services; Windows v1.0 remains dependent on QA and user signoff.

## 2026-10-16 — v0.15 integration merged, actual predecessor upgrade passed
- PR #14 https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/14 merged into `main` at squash commit `3ec779352c0a35f83981ac01d01a7947429b0e2f`.
- Windows Actions https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38903316350 **success**, **122** core checks, static XAML validation, self-contained win-x64 WPF publish and unsigned Inno Setup compiled.
- New upgrade test: `git worktree` checked out pinned v0.14 commit `4f8be7cae20651161651bbec444f412fa34902df`, rebuilt old app+installer, installed old, wrote valid synthetic `reminders.json` and 3-species `pet-profile.json`, upgraded to v0.15, asserted actual upgraded executable file version 0.15.0*, and verified files were **byte-for-byte unchanged by SHA-256 across upgrade and uninstall**. Existing v0.15 same-version install/reinstall/uninstall sentinel test also passed. No user data was accessed.
- Verified downloaded ZIPs by CRC: portable `PetPing-Windows-v0.15-unsigned` ID 12046563227 (~71.5 MB), installer `PetPing-Installer-v0.15-unsigned` ID 12046104323 (~51.4 MB). Extracted `PetPing-Setup-v0.15-unsigned.exe` 51,927,228 bytes, SHA-256 `d37e434ed7428a412021c74e2abb31af8bf89f86d4b2d0cbfdc15823ae2377cb`.
- User still needs real device voice-off and narrator-repeat test, keyboard focus, overlay hit test, multi-DPI, lock/wake and reinstall. Unsigned preview is not public signed stable. Next gate Windows v1.0 RC quality/branding/signing decision, after user confirms v0.15.
