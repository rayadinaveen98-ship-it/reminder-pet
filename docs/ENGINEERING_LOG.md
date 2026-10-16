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
