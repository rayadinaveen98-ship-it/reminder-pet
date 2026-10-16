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
