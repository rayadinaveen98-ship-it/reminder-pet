# PetPing — Desktop-first Reminder Companion

**Current deliverable:** Windows Alpha v0.2 source + automated Windows build workflow and reminder-core smoke checks. This is an **uncompiled engineering alpha scaffold**, not a verified app or ready-made installer. Android is next in the roadmap; no Android APK has been produced yet.

## Contents
- `docs/PRODUCT_BLUEPRINT.md`: product goals, UX principles, scope.
- `docs/ARCHITECTURE.md`: decisions, reminder model, process and mobile adaptation.
- `docs/ROADMAP.md`: staged work and release gates.
- `docs/TEST_PLAN.md`: QA checklist for real Windows computers.
- `desktop/PetPing.Windows/`: native WPF source.

## Build and run on Windows
1. Install the **.NET 10 SDK** (free) and optionally Visual Studio Community with .NET desktop development workload.
2. Open Windows PowerShell or terminal in `desktop/PetPing.Windows`.
3. Run `dotnet restore`, then `dotnet run`.
4. Enter a reminder name; click **In 1 minute** to test a real scheduled reminder, or choose a date and time.
5. Close dashboard: the system-tray program is meant to stay active. Double-click tray icon to reopen. Use tray **Quit** to end reminders.

> This build has not been compiled or tested on a Windows machine in the current environment. Treat code as source foundation and expect a compile/debug pass before production use.

## How delivery works
The UI and reminder engine are hosted in a single resident app process. Hiding the dashboard doesn't stop the scheduler. Quitting the tray program, signing out, shutting down, or force-ending the process stops in-memory polling; the store remains on disk. On restart the app catches up reminders up to 15 minutes late and marks older ones Missed. It does not guarantee wake-up from sleep or secure lock-screen overlays.

## Zero-cost boundary
No hosting or paid APIs. Uses free .NET SDK and locally installed Windows speech engine. Distribution signing/store publication may cost money later. No subscription is required to run the source.

## Current limitations
- One-time reminders only; no daily/weekly recurrence yet.
- No compiled `.exe`, `.msix`, or `.apk` is included.
- Windows source requires real-machine validation, accessibility QA, and reliability testing.
- No autostart after login, persistent voice settings, fullscreen avoidance or notification fallback yet.
- Codename and cartoon cat are engineering placeholders; final branding and mascot need creative design.

## License
No license granted yet. Product source is created for this user project; select a repository license before any public upload.

## No-SDK visual preview (Windows only)
Open `preview/RunPreview.cmd` to see the pet pop up after 3 seconds and hear an optional voice preview. This standalone PowerShell/WPF script is a **visual/overlay capability probe**, not the actual reminder application: it does not save or schedule real reminders. Snooze in this preview is deliberately 10 **seconds**. Inspect the source first if you wish; it does not require admin rights and does not download anything.

## v0.2 build and testing
- The Windows build workflow runs on pushes to `main` and can also be triggered manually in **Actions → Build Windows Alpha**.
- When the run succeeds, open its **Artifacts** section and download `PetPing-Windows-v0.2-unsigned`.
- The EXE is a self-contained unsigned **alpha**, not an installer or a verified consumer release; Windows may display a SmartScreen warning.
- Reminders recover on restart when they were delivered but not acknowledged within 15 minutes. Older ones remain visible in the reminder history.
- Optional **Start with Windows** registers this executable for the current user with `--background` (without automatically showing the dashboard).
- Voice setting is persisted locally. Existing reminder JSON is backed up when overwritten.
- A passing GitHub build does not substitute for real lock, sleep, notification and voice testing on Windows devices.
