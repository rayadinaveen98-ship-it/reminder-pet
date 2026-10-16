# PetPing v0.17 Stage B — Implementation log

## Brand decision
On 2026-10-16 the user selected **Logo C — Companion Beacon**. Candidates A and B are archived as exploratory sources only.

## First native UI slice
- Shared Companion Beacon semantic light color resources for Windows WPF.
- Vector logo component derived from the source SVG; no third-party pet imagery.
- Home/Reminders native shell refresh, retaining all live data, five destinations, keyboard controls and reminder actions. The reminder entry fields and Home metric cards reflow at compact desktop widths.
- Existing v0.15 scheduler, offline reminder storage, TTS, tray lifecycle, profile JSON and installer preservation logic stay unchanged.
- App/installer version stays 0.15 until v0.17 release gates are met; a Stage B branch build is NOT the v0.17 final release.

## Gates
- GitHub Windows CI, WPF compile, 122+ core tests, real installer upgrade smoke and local-data integrity.
- Branded tray icon and window titlebar icon are now rendered locally from the selected original WPF vector, with a safe stock-icon fallback. Packaged multi-resolution ICO and installer artwork are still pending.
- Real Windows 810×640 and 1280×800; 100–200% DPI, keyboard and screen-reader check.
- Stage C: My Pets, Studio and Settings visual alignment, user review.

No physical device test is claimed completed.

## Verified integration
- PR #17 merged to main on 2026-10-16 as `37f1c447d6a2ff0df268c9b88410c8ec209757fb`.
- Final pre-merge Windows Actions: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38919598234 — **success** (static XAML validation, core tests, Windows x64 publish, installer, real v0.14→v0.15 upgrade data preservation, installation smoke).
- UI side effects not verified by CI: actual tray icon appearance, window titlebar, small-screen typography/DPI, reminders after sleep/lock, motion and narration. Do not mark these passed without physical evidence.
- Packaged multiresolution ICO and branded installer EXE remain open; no v0.17 signed release or version bump.

## 2026-10-16 — next-stage work in PR #18
- Multi-resolution Companion Beacon icon generation now uses original WPF `BrandMark.xaml` on a local Windows STA thread. Frames: 16/24/32/48/64/128/256px; no remote art, paid APIs, bundled third-party font or binary artwork dependency.
- `PetPing.Windows.csproj` regenerates the ICO before build and sets `ApplicationIcon`; Inno Setup uses it as `SetupIconFile`. The normal developer Windows build script implicitly runs that target as well.
- UI refinements: honest readable labels, highlighted selected pet, responsive stacked pet cards and two-column Studio groups on compact windows, accessible Settings help that expands only when requested.
- Installer and source version remain 0.15.0 in this integration slice; v0.17 release approval has NOT occurred.
- PR #18: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/18 . Await latest Windows CI, physical small-icon inspection and actual Windows layout/speech/lock testing. Preserve all local reminder and profile data.

## v0.17 preview candidate
- PR #18 merged brand packaging and companion screens, green Windows CI including seven-frame ICO verification, installer and v0.14→v0.15 data preservation.
- Candidate preview branch bumps WPF FileVersion, installer version and GitHub artifacts to 0.17; the upgrade smoke must now rebuild an actual pinned v0.15 installer and confirm surviving local JSON across an in-place v0.15→v0.17 upgrade.
- This is a test build, not a stable product release or proof of live reminders under sleep/lock.
