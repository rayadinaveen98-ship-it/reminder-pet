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
