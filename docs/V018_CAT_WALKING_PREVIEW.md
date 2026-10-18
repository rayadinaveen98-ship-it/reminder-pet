# PetPing Premium Cat Walking V1 — Experimental desktop integration

## Problem resolved
The earlier cat prototype drew the **old flat/vector kitten while walking**, then switched to the approved premium 3D-looking kitten when the notification reached its destination. That mismatch is distracting and was explicitly unfinished. The earlier Windows CI artifact also did NOT contain the four idle PNG assets by default.

## What this phase changes
- `CatIdleSprite` preloads optional `Cat_Walk_{0,1,2,3}_164x218.png` walking keyframes **from the same isolated CatIdle folder** as the original four idle frames. All bitmaps are decoded once and frozen; no per-tick image allocation.
- `PetCharacter.Walk(active)` uses the premium walking keyframes when available, without flashing the vector cat at transition. `Walk(false)` hands control back to the premium six-second Idle frame cycle. Dog/Bunny and non-default cat appearance remain on the existing rig.
- Pure `CatWalkPlaybackRules` defines a four-frame cycle: Full = 150ms per frame (600ms cycle), Gentle = 300ms (1.2s cycle), Off = static first frame. Core regression tests validate frame boundaries, mode and invalid values.
- Walking artwork is based on the **previously approved walking pose**, with four restrained body-pivot/bounce keyframes, not independently animated paws. This is a 2.5D prototype, not genuine rigged quadruped animation.
- Missing walking frames retain the old walking rig, even when idle images load successfully. This remains a safe fallback for older prototype installs.
- No changes to reminder scheduling, Done/Snooze persistence, alarm speech policy or stable `main`.

## Approved art / delivery
Eight PNG files are needed for the full premium idle+walking experience:
- `Cat_Idle_{open,half,closed,happy}_164x218.png`
- `Cat_Walk_{0,1,2,3}_164x218.png`

All images are placed under `Assets/CatIdle` in the user-facing **portable preview ZIP** or copied to `%LOCALAPPDATA%\PetPing-Experimental\CatIdle` by the **all-in-one installer script**. They are kept outside GitHub source temporarily because this phase is pending real-device acceptance; the raw GitHub Actions installer alone does not include these 8 binary images. The finished user-facing package MUST include them; do not direct people to download the raw CI artifact expecting premium visuals.

Walking PNG SHA-256 digests:
- 0: `20040816366b745324b17c935ef40cb4cdcf994de75f4401a0fb3ab772e35971`
- 1: `985de73ca0754e2e9d5b9fd63ba17c682ba1f673ad45ab992c2607d86e4edf29`
- 2: `8239880f7bdcd6546c0b50f6bc028b48e6436163af49ce5795585f429a97c8c8`
- 3: `5dcf27a43d2fe0ef779096ac2e6d29211ebd48eeb5f4dc9350e2fc18f5d11d06`

## Acceptance
- CI: XAML/static validation, WPF x64 compilation, core checks, experimental installer install/reinstall/uninstall, and preservation of stable and experimental data.
- User laptop: confirm cat is consistent while approaching and waiting in the floating popup, verify Home/My Pets/Studio, face flip, screen scaling, touch/keyboard, motion settings, voice and Done/Snooze. Check CPU/memory over a long idle session.
- **Production release gate:** approved 2.5D walking prototype in real app, richer limb movement if desired, and Talking animation. Keep PR draft until device validation.
