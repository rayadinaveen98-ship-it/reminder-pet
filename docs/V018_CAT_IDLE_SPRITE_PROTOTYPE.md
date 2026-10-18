# Cat Idle V1 — premium 2.5D sprite prototype (not a public release)

## Identity and consent
The user approved the orange and cream cat identity, the first three poses and four idle expression frames. This is a visual prototype only. The original cat illustration remains the locked art reference. Do not recolor or change the avatar's amber eyes, pink nose, cream chest/paws, burgundy collar or gold bell.

## Asset source and installation
The original 164×218 transparent PNG sprites are distributed as a separate, downloadable companion ZIP in the project conversation: `PetPing_Cat_Idle_Windows_Integration_Kit.zip`. It includes `CatIdle/Cat_Idle_{open,half,closed,happy}_164x218.png`, light/dark GIF previews and `Install_CatIdle_Experimental.ps1`.

**Sprite data is NOT included in this pull request:** it is intentionally a test sidecar. The local experimental app reads files only from `%LOCALAPPDATA%\PetPing-Experimental\CatIdle` or `Assets/CatIdle` next to its own EXE. No production assets, profiles or reminders are overwritten. The code keeps the previous WPF rig if artwork is missing or invalid. Running the experiment without the ZIP is safe but looks like the existing vector pet.

All four RGBA files must exist at **164×218** before activation. The Windows image decoder loads and freezes them once. The app never decodes GIF/WebP on each frame.

## Native state integration
- `PetCharacter` retains the vector rig, including its Dog and Bunny appearance/behavior. Only **Cat + Apricot palette + no added accessory** may use the image overlay, because changing the color of the approved fixed artwork is unsupported.
- One shared pet actor works in Home, My Pets, Pet Studio and actual popup. It uses `CatIdleSprite` while waiting and previewing. The sprite is flip/scaled with the original actor transform.
- Walking entrance uses the old cat vector until true approved walking frames are made; that transition is a **known temporary inconsistency** and must not be hidden in release notes.
- The cat's idle breathing/blink/happy expression uses a six-second deterministic frame schedule. The `PetMotion.Full` setting allows breath + two blinks + happiness, `Gentle` allows blinks only and `Off` shows the unanimated awake frame.
- Speaking currently alternates expressions without lip-sync; Done/Snooze use happy and closed-eye expressions after the reminder was successfully saved, with existing fixed-duration exit logic. This is NOT a rigged face, ear or tail animation system.
- Display does not add a full-screen transparent input layer, and keeps the existing reminder action chips clickable.
- `CatIdlePlaybackRules` is a pure domain timeline with CI tests at keyframe and mode boundaries.

## Verification and QA still required
- Verify Windows CI compiles WPF XAML/code-behind, regression tests, original logo assets, experimental installer, and does not modify production reminders.
- Manually install the artwork ZIP, restart experimental build, and check Home, My Pets, Studio and an actual 485×352 floating reminder at 100/125/150/200% DPI. Test front-back asset cutoff, sudden vector-to-sprite transition, battery/CPU use, window focus, speaking and reaction cleanup.
- Approve true walking frames and speaking-mouth layer before expanding the sprite renderer to those states. No production release until real-device acceptance.
- `main` remains v0.17. The experimental version uses separate `%LOCALAPPDATA%\PetPing-Experimental`; user-created reminders are never imported automatically.

## Approved sprite digest manifest (SHA-256)
- `Cat_Idle_open_164x218.png`: `6a9f209152dd0a37cae5326ba1220b4469720ec49abf68a0a2fcca07a9b6d5d0`
- `Cat_Idle_half_164x218.png`: `3bc33645dad425427aee7023243dcd699104acbda0a671e0c3025b048f989ae2`
- `Cat_Idle_closed_164x218.png`: `28a67adb44ad653fa8ac903183a343e61b43febdc5efa27b3109175789cdf78a`
- `Cat_Idle_happy_164x218.png`: `5aa1204f33c89c6d2cc0720114a00b8324972f975b4ef6dd6ac4c8ad13d595ae`

These hashes cover the separate original-sprite files inside the user ZIP. They are not hashes of the GitHub installer artifacts.
