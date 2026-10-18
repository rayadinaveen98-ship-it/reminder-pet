# PetPing v0.18 — Final clean companions (no walking)

## User decision — LOCKED 2026-10-18
The user rejected both Cat V1 image-bounce Walking and Cat V2 articulated leg animation. The current premium cat artwork and existing Dog and Bunny identities are sufficient as-is. **DO NOT build or ship further walk, puppet, gait, head-bounce, or locomotion experiments unless specifically requested.** Focus on reliable reminders and attractive stationary companions.

## Product behavior
- Companion appears **immediately** at the already-calculated reminder destination; no edge entrance walk, no moving/animated Window.Left and no gait or artificial floating/paw animation.
- PetWindow uses a small transparent reminder window with the existing Done, Snooze, Mute and Hide actions. Buttons, voice/repeat and reminder storage remain intact.
- Stationary Cat retains the already approved orange/cream premium Idle artwork when Cat + Apricot + No accessory is selected. The original WPF pet renderer is the fallback if artwork is missing or an alternate palette/accessory is selected.
- Dog and Bunny keep their original WPF characters. Gentle idle blinking, speaking expressions and finite action-response feedback are retained; **no screen locomotion**.
- Actual walking entry method `PetCharacter.Walk` is deleted, and real PetWindow no longer calls it. Live UI status does not advertise Walking-art readiness.
- Cat V2 is NOT included in this build, and the ZIP excludes ALL 4-frame/V2 walking PNGs. Only 4 locked cat Idle expressions and 1 mouth-rest derived from them are packaged.
- Old experimental profile art (if any) is ignored by the stationary popup; do not delete user files without consent. Existing reminders preserved.

## Build scope & upgrade safety
- v0.18 reminder features remain available: flexible snooze, custom recurring reminders, safe JSON persistence, reminders export and tested scheduler recovery.
- This final **unsigned** candidate is an isolated Windows experimental-profile build. It uses `%LOCALAPPDATA%\\PetPing-Experimental`, Inno AppId and mutex dedicated to the experimental version; the stable v0.17 profile at `%LOCALAPPDATA%\\PetPing` is untouched. Existing experimental reminders are preserved by installer smoke tests.
- User ZIP bundles CI-compiled Windows installer with **5 required Cat Idle/Talking PNG sprites** and a one-click artwork-first installer wrapper, so an Inno auto-launch sees the art without extra install steps. Do NOT link the raw CI installer by itself while claiming it contains the PNGs.
- The app window title must be `PetPing v0.18 — Original Companions (No Walking)` so that users can distinguish this candidate from stale Cat V1/Cat V2 builds.
- CI: Windows WPF compilation, static no-walking guard, core reminder regressions, isolated installer install/reinstall/uninstall checking both profiles unchanged.
- Physical Windows desktop acceptance still needed for actual popups, title and speech, but must not block delivering the requested testable installer.

## Manual verification
1. Exit PetPing Experimental from the tray (do not merely close dashboard; tray resident).
2. Extract entire final ZIP and run the one-click `INSTALL_PETPING_NO_WALK.cmd`. An alternative portable ZIP has `Assets/CatIdle` alongside EXE.
3. Confirm new title **PetPing v0.18 — Original Companions (No Walking)**. For approved premium cat select Cat + Apricot + No accessory; for custom cats, Dog, Bunny, keep the original appearance.
4. Click Preview on desktop, test reminder, mute, Done, Snooze. **Cat should appear directly at destination** with no approach walk or body bob. Blinking/voice and short response effects remain.
5. Check reminder persistence and no changes to v0.17 app. This is not a signed public release or a v0.17 in-place upgrade.

## Not included
No Cat V2 3D model, Cat V2 32-frame sprite animation, Cat V1 four-frame Walking, genuine rigged character, Cat V2 assets or experimental movement research are shipped in the final app package.
