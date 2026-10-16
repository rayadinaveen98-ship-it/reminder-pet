# Windows v1.0 RC1 — product/release checklist

_Proposed 2026-10-16. This is a plan, **not** a claim that v1.0 RC is built._

## Product intent
Ship one reliable offline-first Windows reminder companion, not an expanded game. Keep WPF/.NET 10 local desktop and the original floating orange cat. User broadly reported v0.9 working on their Windows laptop; detailed per-scenario certification remains incomplete.

## RC1: required engineering output
- [ ] Version the desktop assembly, visible title/about information, installer and artifact consistently as **v1.0.0-rc1**; remove stale alpha labels.
- [ ] Create a distinctive owned app icon and confirm tray, shortcuts and installer use it at appropriate resolutions. No borrowed character artwork.
- [ ] Add a genuine predecessor installation test to GitHub Actions: install **v0.9**, seed verified reminder/settings fixtures, install RC1 over it, verify content/recurrence/appearance data remains, uninstall and verify local data remains. Keep fixture data synthetic.
- [ ] Keep existing recurrence/delivery tests and installer data-preservation checks green.
- [ ] Write clear release notes and install/upgrade/uninstall guide with signed/unsigned disclaimer, backed-up personal data advice and no cloud/telemetry claims beyond implementation.
- [ ] Publish CI artifacts and SHA-256 checksums; make an explicitly labeled **pre-release** on GitHub after passing gates.
- [ ] Review failure paths: invalid primary backup recovery, disk full/write denial, multiple reminder collisions, mixed local timezones, startup/tray single instance.

## Manual Windows acceptance (record evidence; don't assume)
- [ ] Verify fresh per-user installer flow, Start Menu link and uninstall using regular Windows user.
- [ ] Verify upgrading an actual previously-installed v0.9 to RC1 without losing reminders or voice/pet preferences.
- [ ] Verify app launch after reboot, tray-only reminder handling, closing dashboard, and second-instance behavior.
- [ ] Verify delay policy: active, lock/unlock, suspend/wake within 15 minutes and wake after >15 minutes.
- [ ] Verify 100/125/150/200% text scaling, more than one monitor, video/fullscreen behavior.
- [ ] Verify keyboard navigation, visible focus, screen reader labels and motion-off option.
- [ ] Verify optional speech toggling, pet mouth feedback, Done/Snooze, recurring tomorrow/next week.
- [ ] Test recovery prompt with **disposable reminders** only; never intentionally corrupt genuine user data.

## Publication policy
- Windows Authenticode signing and SmartScreen reputation are not available automatically. An unsigned prerelease can be tested privately with explicit source verification, but is **not** equivalent to signed production distribution.
- No unrequested installer auto-launch, telemetry, forced startup or auto-update promise.
- If user chooses public distribution without signing, clearly document the friction/risk instead of claiming a trusted verified publisher.

## Exit criteria
1. CI is green for current code and cross-version installer upgrade fixtures.
2. P0 data-loss, wrong recurrence, duplicate reminder and startup issues resolved.
3. Manual QA reviewed with explicit pass/fail/deferred, not inferred from a single broad message.
4. User approves stable Windows v1.0 designation.
5. Then create the Android Kotlin/Compose + Room project, keeping OS-native notifications as the reliable fallback for restricted overlays/lock screens.

## Resuming in a new chat
Read `docs/PROJECT_STATUS.md`, `docs/ENGINEERING_LOG.md`, and this checklist. Check the latest commit/open PRs/CI, then begin v1.0 RC1 on a new branch. Keep milestone progress committed after each meaningful build.
