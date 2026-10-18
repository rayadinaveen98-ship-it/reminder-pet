# PetPing Cat V2 — true quadruped walk, production gate

**Status:** ANIMATION REBUILD SPEC — NOT IMPLEMENTED, NOT RELEASE READY.
**Why:** User reviewed actual Cat V1 app on a Windows laptop and found the walking motion lifeless. The prior generator `/mnt/data/build_cat_walking.py` simply rotated the *entire raster image* -1.15° to +1.05° and shifted by 1–2 px. It created four altered stills, not moving feline limbs. Four frames, 150 ms/frame, 600 ms/cycle cannot establish paw contact or body weight. The V1 walking animation is **REJECTED BY USER** as not lifelike; do not describe PR #28–30 as approved walking.

## Locked identity (not negotiable)
- Premium original orange/cream tabby from the user's **original waving kitten**, not a randomly restyled cat.
- Specific amber eyes, pink nose and inner ears, cream chest/paws, original facial proportions and recognizable markings.
- Burgundy collar + single golden bell, fluffy curled tail and friendly 3/4 presentation; style is premium soft 3D CGI, not angular low-poly.
- Avoid a new cat each frame. No text or scenery baked into alpha sprites.
- Dog and Bunny unchanged. Stable Windows v0.17 remains unchanged. All development stays experimental.

## Recommended asset-production route
**Rigged 3D source → baked transparent sprite animations → native WPF viewer.**
Use Blender (free) to author or adapt a true cat quadruped skeleton with foot IK targets, shoulder/hip/knee/ankle chain, at least 4 tail controls, head/neck pivots, face/eye controls. Fit geometry/material/coat/collar to locked master reference. Rig must preserve proportions from front, three-quarter and lateral views.

Open-source option for motion reference (NOT final art): Quaternius Animal Pack Vol. 2 CC0 `https://opengameart.org/content/animated-animales-low-poly`, which explicitly includes a cat/animal model with animation and commercial/public-domain terms. If its feet and gait match, use it as a *gait motion reference or skeleton prototype* only. Its low-poly cat is NOT a visual substitute for our locked kitten.

If full custom 3D model generation is impossible without paid credits, use Blender/manual mesh and a CC0 quadruped rig rather than pretending raster distortion is walking. No paid generation is required for the essential gait design.

## Motion quality specification
1. **Contact and weight:** at least four separate foot contacts, two legs lift in coordinated nonuniform timing; each support paw remains planted relative to virtual ground during stance. NO visible sliding.
2. **True leg articulation:** shoulder/hip moves before paw swing; front/back legs have distinct silhouettes and paw paths; bent paws never stretch or warp entire torso.
3. **Natural center of gravity:** pelvis and chest shift subtly, tiny vertical oscillation timed to steps. Head lag, tail counterbalance with independent delayed movement. Do not bob the entire cat as a rigid card.
4. **Physical timing:** base walk cycle **0.9–1.2 seconds**, authored at 24 FPS or higher; use 24–32 unique rendered alpha frames per cycle (not four rotations). Variant Gentle 1.25x–1.5x slower, with reduced head/tail motion. Off displays static valid contact pose.
5. **Start/stop clips:** separate anticipation (eye direction/paw lift), approach, decelerate, plant feet, settle into approved premium sitting Idle (0.25–0.4s); no hard image jump into a different silhouette/camera angle. Preserve viewport pivot/feet baseline, use optical or composited tween for final settling if approved.
6. **Real root movement:** WPF screen-space translation must be synchronized with paw cadence/stride length; no walking-in-place while the sprite floats past the screen.
7. **Performance:** transparent trimmed atlas or PNG sequences; predecode/cache frozen bitmaps once; preload before first reminder alert; cap WPF animation to 24 FPS; avoid per-frame allocations, UI blocking and GDI decoder leaks.
8. **Clarity:** show whole cat at 150–300 display pixels on both dark and light desktop, 100/125/150/200% Windows DPI without clipping or halo. Ears, eyes, tail, collar and bell remain visible.
9. **Quality gate:** user visually approves actual walking footage at desktop size BEFORE replacement in reminder alerts or merging. Test 5+ full gait loops for foot sliding and aliasing. Long idle >10m without CPU/memory buildup.

## Proposed delivery phases
- **A — Motion reference:** Build one true skeleton (or retarget CC0 gait). Produce side profile and three-quarter silhouette tests + contact sheet marking 4 planted-paw events. Reject inconsistent anatomy before polishing fur.
- **B — Premium character model:** sculpt/texture the same approved orange kitten on that rig. Render clean turntable stills and get user approval versus locked reference.
- **C — Animations:** deliver 24–32 frame 1s walk loop, 6–10 frame start, 6–10 frame stop/settle, then Idle/Talking/Done/Snooze variants; all on the same renderer/camera/color grade.
- **D — App integration:** new `RiggedCatSprite`/asset format and manifest + SHA-256, 24 FPS playback and root-motion sync; keep Cat V1 selectable as fallback until QA. Update version string explicitly. No replacement of Dog/Bunny.
- **E — Acceptance:** CI validates dimensions/alignment/alpha/unique frame poses, frame count & memory budget. Physical laptop verifies actual natural walk, matched identity, no old-cat flash, actions, speech, sleep/wake, low-end performance.

## Why earlier attempts failed
- Four PNGs were near-identical full-image rotations; no animated paw joints.
- Current original walking illustration has ONE gait pose, not a fully modeled 3D cat or reconstructible anatomy.
- Simple raster deformation can create small paw motions but cannot generate correct occlusion/hidden anatomy or dependable paw planting.
- Beautiful contact/storyboard art is NOT a production-grade walking sprite sequence and does not prove functionality.
- A fully rigged 3D source is more work but is the repeatable way to keep identity across walking/turning/playful actions.

## Immediate project action
Keep this feature branch as design/research only. No claim that real rigged assets were already made. Keep current CI-green Cat Sequence QA as an optional **prototype**, but **do not ship current Walking V1 as accepted**. Next measurable checkpoint: **8 key poses with distinct leg positions, visible planted paws, and no pose drift**, then full 24 FPS loop. The storyboard/reference concept must not be confused with delivered animated frames.

Source/license reference: Quaternius Animated Animals Low Poly, https://opengameart.org/content/animated-animales-low-poly ; CC0 displayed at source.
