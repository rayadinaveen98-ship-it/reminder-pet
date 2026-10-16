# PetPing — Desktop-first Reminder Companion

**Current working release:** Windows **v0.9** user-tested alpha. **Latest Windows test build:** v0.13, genuine Cat/Dog/Bunny selection and separate saved customization for each species. GitHub Windows CI passed 105 core checks, compiled portable x64 and optional installer, and verified installer preserves user data. Actual on-device UI acceptance still pending. GitHub Actions passed 86 core checks and compiled both installer and portable build; real laptop visual/voice QA is pending. This is unsigned and not a production release. Android is next in the roadmap; no Android APK has been produced yet.

**Continuity:** See [docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md) for current progress, CI evidence, known issues and new-chat handoff; and [docs/ENGINEERING_LOG.md](docs/ENGINEERING_LOG.md) for a chronological implementation log.

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

> GitHub CI compiles on a Windows runner and runs core checks, but interactive popup, tray, lock/unlock and sleep/wake behavior still require hands-on Windows testing.

## How delivery works
The UI and reminder engine are hosted in a single resident app process. Hiding the dashboard doesn't stop the scheduler. Quitting the tray program, signing out, shutting down, or force-ending the process stops in-memory polling; the store remains on disk. On restart the app catches up reminders up to 15 minutes late and marks older ones Missed. It does not guarantee wake-up from sleep or secure lock-screen overlays.

## Zero-cost boundary
No hosting or paid APIs. Uses free .NET SDK and locally installed Windows speech engine. Distribution signing/store publication may cost money later. No subscription is required to run the source.

## Current limitations
- One-time reminders only; no daily/weekly recurrence yet.
- A Windows `.exe` can be downloaded from a successful GitHub Actions run; no `.msix` installer or Android `.apk` exists yet.
- Windows source requires real-machine validation, accessibility QA, and reliability testing.
- Optional autostart at login and persistent voice setting are in v0.2; fullscreen avoidance and notification fallback remain pending.
- Codename and cartoon cat are engineering placeholders; final branding and mascot need creative design.

## License
No license granted yet. Product source is created for this user project; select a repository license before any public upload.

## No-SDK visual preview (Windows only)
Open `preview/RunPreview.cmd` to see the pet pop up after 3 seconds and hear an optional voice preview. This standalone PowerShell/WPF script is a **visual/overlay capability probe**, not the actual reminder application: it does not save or schedule real reminders. Snooze in this preview is deliberately 10 **seconds**. Inspect the source first if you wish; it does not require admin rights and does not download anything.

## v0.7 build and testing
- The Windows build workflow runs on pushes to `main` and can also be triggered manually in **Actions → Build Windows Alpha**.
- When the run succeeds, open its **Artifacts** section and download `PetPing-Windows-v0.9-unsigned`.
- The EXE is a self-contained unsigned **alpha**, not an installer or a verified consumer release; Windows may display a SmartScreen warning.
- Reminders recover on restart when they were delivered but not acknowledged within 15 minutes. Older ones remain visible in the reminder history.
- Optional **Start with Windows** registers this executable for the current user with `--background` (without automatically showing the dashboard).
- Voice setting is persisted locally. Existing reminder JSON is backed up when overwritten.
- A passing GitHub build does not substitute for real lock, sleep, notification and voice testing on Windows devices.

## v0.3 attention recovery
- The dashboard counts reminders that are **missed** or **unanswered**, including those whose popup timed out after 45 seconds.
- Select a missed or unanswered reminder and choose **Snooze selected 10m** to schedule another attempt.
- A catch-up scan finding newly missed reminders may display one system tray balloon summary. Windows notification settings can suppress balloon tips; the dashboard remains the source of truth.
- If the Windows session locks while a pet is onscreen, the popup is closed and that reminder is queued for after unlocking (without claiming secure lock screen overlays).
- Completing or snoozing an onscreen reminder from the dashboard also closes its corresponding pet popup.

## v0.4 floating companion redesign
- The large white notification card is replaced with an original orange vector cat and a separate compact speech bubble.
- The cat slides/hops in, gently floats and wags its tail, then exits naturally. It does **not** have a full walking sprite-sheet yet.
- On maximized/full-screen-sized foreground windows (such as a maximized Netflix browser), the popup moves to **upper right**, away from lower playback controls. Otherwise it sits above the lower-right taskbar area.
- Snooze 10m, Done, and × (hide without completing) remain available. Hover pauses automatic dismissal. After timeout the reminder remains in Needs Attention.
- See `docs/V0_4_FLOATING_PET.md` for interaction details, limitations and hands-on acceptance checks.
- The `preview/RunPreview.cmd` experiment is a **legacy v0.1 preview**; use the compiled v0.4 EXE to evaluate the redesigned pet.

## v0.5 reliability changes (pending CI and physical testing)
- Logging under `%LOCALAPPDATA%\PetPing\diagnostics.log`, bounded to approximately 256KB with one rotated previous file. Logs include event identifiers and exception types, not reminder names or exception messages. Local-only, no upload.
- Scheduler catches presentation and persistence failures; retries failed storage updates with 30-second cooldown while the tray process remains running, instead of crashing the dispatcher.
- Failed reminder writes restore prior in-memory state, preventing false completion/delivery claims.
- Generic tray fallback notice if the pet popup cannot render; Notifications may be suppressed by Windows and are NOT a guaranteed alternative alarm.
- Voice test in dashboard, local TTS failure diagnostics and stopping speech when session locks / machine suspends.
- Manual release gate remains Windows lock, wake, and speech confirmation.

### Latest verified CI and handoff
- v0.5 green Windows PR build: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38864475272 (32 core checks, Windows x64 compilation and unsigned ZIP artifact).
- After source changes, keep `docs/PROJECT_STATUS.md` and `docs/ENGINEERING_LOG.md` updated before merging. They are the source of truth for resuming future conversations.
- The user's v0.4 visual test confirmed the pet floats outside the speech bubble. The v0.5 changes have not yet been manually verified.

## v0.6 editing and recurring reminders
- Create a one-time, daily, or weekly reminder. Weekly means **same weekday**, not custom days yet.
- Select an ordinary upcoming reminder and choose **Edit selected**; save title, date, time and repetition. Historical Delivered, Missed, Completed events cannot be edited.
- Each repeat creates a **new occurrence** when the previous one is delivered or missed; past occurrences remain in history (and may still need attention).
- Snoozing an old occurrence does **not** generate another future copy. A long period offline creates only one next future scheduled occurrence, rather than every missed day/week.
- Uses the timezone from initial creation and preserves wall-clock time through daylight-saving transitions. Invalid spring-forward times shift to the next valid minute; ambiguous autumn time uses the earlier UTC instant.
- The UI currently changes only the next scheduled unsent occurrence, **not all past instances**. Future recurrence after that edit inherits the updated title and repeat rule. To stop a repeat, edit the upcoming instance to One time or delete it.
- See `docs/PROJECT_STATUS.md` and `docs/ENGINEERING_LOG.md` for current CI and testing status before resuming in a new chat.

## Latest v0.6 build evidence
- Windows GitHub Actions: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38868473793 — 49 core checks + executable publish succeeded.
- Artifact: `PetPing-Windows-v0.9-unsigned` (time-limited retention). The release is a test ZIP, not an installed/signed consumer build.

## v0.7 pet interaction and preferences
- Cat now blinks occasionally and displays a small talking mouth synchronized to **local Windows TTS speech-start/speech-complete events**.
- **Pet appears** preference offers Auto, Top right, Bottom right, Top left, and Bottom left. Auto uses top-right when a maximized foreground window is detected; otherwise bottom-right.
- **Motion** preference offers Playful (entry slide, floating, wagging, blinking and mouth animation), Gentle (minimal fade plus blink) or Off (no entry, float, wag, blink or fade; speaking mouth may show as a static state).
- Placement chooses the screen of the currently foreground application and maps monitor pixels into WPF logical coordinates. Mixed-DPI hardware still requires physical testing.
- Settings are saved independently at `%LOCALAPPDATA%\PetPing\pet-appearance.json`; old reminders and repeat rules are left unchanged. Settings apply to the next pet popup.
- The pet remains a single original orange vector cat; alternate animals and new illustrated packs are deferred, not silently claimed as implemented.

- Done/Snooze now show short confirmations **only after the reminder state has saved**; if saving fails the pet stays available for retry.

## Final v0.7 build evidence
- PR #6 merged into main: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/6
- CI: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38872547917 — 56/56 tests, WPF compile, unsigned ZIP uploaded.
- Artifact: `PetPing-Windows-v0.7-unsigned` (ID 12034680037). ZIP integrity checked and executable verified.
- For future chats read `docs/PROJECT_STATUS.md` and `docs/ENGINEERING_LOG.md` first, and update both after changes.

## v0.8 dashboard and installation
- Filter reminders into All, Today, Upcoming, Needs attention and History. These are read-only list views: filtering does not modify saved reminders.
- Friendlier empty states, adaptive wrap for action buttons, accessible control names and keyboard shortcuts: **Ctrl+N** new reminder, **F5** refresh, **Esc** cancel editing.
- Delete asks confirmation, including the impact of removing the next upcoming repeated occurrence.
- Two Windows choices once GitHub CI passes: portable ZIP or a **per-user Inno Setup installer** (does not request administrator privileges).
- Installer uses `%LOCALAPPDATA%\Programs\PetPing`, includes Start Menu shortcut and an optional desktop icon. The previous Start-with-Windows preference is redirected to the installed executable when already enabled.
- **Uninstall keeps `%LOCALAPPDATA%\PetPing`**: reminders, preferences and diagnostics are not removed. This also applies to future upgrades. This is an unsigned alpha so Windows SmartScreen may warn; a signed commercial installer is future work.
- Before upgrading from a portable build, quit PetPing from its tray icon. Do not run two copies simultaneously. Test installer smoke checks do not constitute complete interactive device QA.

## Final v0.8 build and installation evidence
- PR #7 merged: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/7
- Final CI: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38877847189 — 64 core checks passed, WPF compiled, installer built, silent install/uninstall and local data-preservation smoke passed.
- Portable ZIP: `PetPing-Windows-v0.9-unsigned` (artifact 12035474939). Installer ZIP: `PetPing-Installer-v0.9-unsigned` (artifact 12036600668), containing `PetPing-Setup-v0.8-unsigned.exe`.
- To upgrade from the portable alpha, **Quit PetPing from the tray first**. Run the installer with your regular Windows account, choose desktop shortcut if desired, and test that your saved reminders still appear. The Inno Setup uninstaller is under Windows installed applications.
- Installing/uninstalling never intentionally removes `%LOCALAPPDATA%\PetPing`. You must separately delete that folder if you explicitly want to erase your reminder data.
- Installer signing/SmartScreen reputation, screen-reader QA, Win10/11 sleep/lock and mixed-DPI acceptance remain unresolved. Never claim complete production reliability from CI alone.

## v0.9 backup recovery and diagnostics
- When `reminders.json` is unreadable but its `.bak` copy is valid, the desktop app asks **whether to restore**. It explicitly warns the backup may be older, so latest edits may not be present. No silent data loss or automatic blank reset.
- If approved, the damaged original is copied to a unique `reminders.unreadable-*.json` file in LocalAppData; the backup is validated and used as current reminders. If denied, files remain unchanged and startup stops.
- A broken/absent backup is not offered for restoration; manual support/inspection is required.
- The dashboard's **Open data and diagnostics folder** opens Windows Explorer so a user can make their own copies; no reminder texts are sent anywhere.
- Installer CI now also checks same-version reinstall/repair before uninstall, preserving a local data file. Upgrading an actual v0.8 installation to v0.9 remains a physical Windows acceptance test.

## Final v0.9 CI and download verification
- Merged PR #8: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/8 ; final CI: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38881511852 .
- **72** core smoke checks, WPF win-x64 build, and unsigned Inno Setup installer compile succeeded. Silent install, reinstall, uninstall and local data sentinel preservation passed.
- Portable `PetPing-Windows-v0.9-unsigned` (artifact ID 12038021245), installer `PetPing-Installer-v0.9-unsigned` (artifact ID 12037687579), containing `PetPing-Setup-v0.9-unsigned.exe`. Both archives verified and installer extracted.
- Upgrade: quit previous PetPing from the tray; install normally, then confirm reminders, recurring schedule, sound/pet preferences and Start with Windows. Data at `%LOCALAPPDATA%\PetPing` is designed to remain after uninstall.
- **Important:** recovery uses a backup from before the latest save, so some newer changes may not be present. Consent is required. Back up actual data before deliberate corruption tests; use disposable test data only. Unsigned installer may trigger Windows security prompts. Physical v0.8→v0.9 upgrade and sleep/lock/mixed-DPI tests remain open.

## PetPing 2.0 v0.11 developer preview
- New small transparent walking window enters from a random left/right edge on the active monitor, ends near screen center, then shows **unboxed** readable reminder text and Done, Snooze 10m, Mute and Hide action chips.
- Speech begins after arrival and repeats approximately 60 seconds after it finishes, until an action, Mute, Hide, lock/sleep or close stops it. No voice overlap or fullscreen click-catching overlay intended.
- Motion Gentle glides briefly; Motion Off appears in place. Previous Top/Bottom/Left/Right settings still influence direction/vertical placement.
- The first v0.11 uses our original orange cat. New dog/rabbit and Pet Studio are later milestones; the Canva redesign is only a design reference until implemented in WPF.
- Reminder JSON/history and existing local preferences are intentionally unchanged. Back up `%LOCALAPPDATA%\PetPing` before installing dev builds. Final live-device behavior not guaranteed by CI alone.

### v0.11 verified build / user instructions
- Merged PR #10: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/10 . Green CI https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38890848806 — 86 core checks, Windows compilation, optional installer build and preserved-data smoke passed.
- Downloadable unsigned v0.11 artifacts: `PetPing-Windows-v0.11-unsigned` (ID 12042450420) and `PetPing-Installer-v0.11-unsigned` (ID 12041443215), containing `PetPing-Setup-v0.11-unsigned.exe`.
- Back up `%LOCALAPPDATA%\PetPing`, quit previous app from tray, install or extract v0.11, then test edge-to-center walk and on-arrival narration. No data migrations. Click **Hide** to dismiss popup without completing; **Mute** stops narration but leaves pet.
- CI does NOT verify physical per-pixel WPF click passthrough, DPI/multi-monitor fit or actual local TTS latency. Report those results before v0.12.

## v0.12 redesigned Windows interface (development)
- A new WPF native 5-screen app uses the approved Canva warm ivory/lavender direction and a reusable original orange cat portrait. Navigation: **Home**, **Reminders**, **My Pets**, **Pet Studio**, **Settings**.
- Home derives Today/Upcoming/Needs Attention and three next reminders from the actual offline store. No mock sample metrics or unrelated pet-care reminders in the native UI.
- Reminders includes functional date/time/one-time/daily/weekly creation and editing, list filters, Done/Snooze/Delete and existing keyboard shortcuts. Settings retains optional Windows speech, login startup and Open data folder.
- My Pets shows Milo as the active original cat, with Dog and Bunny explicitly marked coming in v0.13. Pet Studio provides existing real motion/arrival options and pet preview; new species/action customization will be implemented in subsequent versions.
- The walking v0.11 pet experience and reminder JSON data remain unchanged. No paid services or external Canva dependency is added to the compiled app.
- CI artifacts: `PetPing-Windows-v0.12-unsigned` (ID 12042344606) and `PetPing-Installer-v0.12-unsigned` (ID 12043370603), verified intact. Installer `PetPing-Setup-v0.12-unsigned.exe` SHA-256: `53a381461f24996d7f93dc979a31b435cc65742892cca1a93250f66492222dd6`.

## v0.13 multi-pet system (source in development)
- Three species: **Milo Cat**, **Buddy Dog**, **Luna Bunny**, distinct original vector anatomy and actual walking reminder behavior. My Pets selection genuinely changes the overlay.
- Pet Studio customization: 4 coats, 3 accessories, 3 sizes, 3 personalities, two Done and two Snooze action variations, 30/60/120-second voice repeat; existing placement/motion controls remain.
- Each pet retains its **own** saved appearance/actions when switching species; selections persist locally in a versioned `pet-profile.json`; old `reminders.json` and `pet-appearance.json` remain unchanged.
- Profile corruption falls back to defaults without wiping reminders or the corrupt file; invalid values and failed writes cannot change last saved settings.
- v0.14 adds richer actions/visual effects; v0.13 provides the functional foundation, not full effects. Windows CI passed; laptop appearance, interactions and audio testing still pending.

## v0.13 verified release checkpoint
- Merged PR #12. Windows Actions run 38897525278 passed 105 core checks and unsigned install/reinstall/uninstall data preservation.
- Portable Windows ZIP artifact: 12044223333. Installer ZIP artifact: 12045065431. Both archives verified intact and the installer executable extracted.
- v0.13 profile settings are independent from reminder data. User device acceptance is still pending.

## PetPing v0.14 expressive animations (Windows CI verified)
- Cat careful steps/tail, dog jaunty trot/wagging and moving ears, bunny small hops/ear twitches. Shared rig remains used on Home/My Pets/Studio and real walking reminder.
- Done choice (hop or wave) and Snooze choice (yawn or stretch) are now distinct visual reactions with short-lived species-colored sparkles/stars, **after the reminder is saved**.
- Per-pet Effects Off, Gentle, Playful stored in existing local profile without reminder-data changes. Global Motion Off suppresses movement/effects, Gentle motion reduces effects, and effect bursts are capped at 10 particles.
- Pet Studio includes Try Done/Try Snooze buttons that preview effects without changing task state. v0.14 Windows CI/installer verified (122 tests), physical Windows laptop acceptance still pending.

## Final v0.14 build and download evidence
- Merged PR #13: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/13 ; green CI https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38900092320 (122 tests, WPF, installer preservation).
- Portable unsigned ZIP: `PetPing-Windows-v0.14-unsigned` artifact ID 12045033789. Installer ZIP: `PetPing-Installer-v0.14-unsigned` ID 12044899354; contains `PetPing-Setup-v0.14-unsigned.exe`, SHA-256 `47414e3f92db6c52f2f5c6887f996de98f5d77f8a7b671340571c1f3b235ec77`.
- Back up `%LOCALAPPDATA%\PetPing`, quit previous tray copy, then install. Test Pet Studio effects previews before trying a live reminder. Real device visuals and accessibility are pending; installer remains unsigned.
