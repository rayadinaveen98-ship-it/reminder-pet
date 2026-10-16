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
