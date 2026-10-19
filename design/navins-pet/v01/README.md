# Navin's Pet — Puppy 01 character foundation

**Phase:** 1, original character / animation-production foundation.  
**Status:** exploratory source artwork and specs; NOT an approved final mascot, functioning animation, TTS integration, or Windows installer.  
**Production boundary:** experimental branch only. Do not modify, delete, overwrite, or auto-merge previously approved PetPing characters or user reminder data.

## Product identity
**Name:** Navin's Pet  
**One-line promise:** Your reminders, with a little company.  
**Platform:** Windows first, native C#/.NET/WPF.  
**Core interaction:** a small expressive animal appears above desktop apps, approaches naturally, looks toward the user, speaks the reminder's name, waits without freezing, responds to Done or Snooze, and leaves.

### Creative direction
Premium illustrated 2D storybook, not flat emojis or exaggerated 3D/game assets. Warm ivory, golden apricot, gentle terracotta, dark cocoa; friendly, readable and calm, with no sterile dashboard and no cutesy clutter. Avoid animation spam, magical confetti in everyday alerts, long cinematic interruptions, or giant overlapping notification cards.

### Scope lock
* One small dashboard: add reminder (title/date/time/repeat), upcoming list, edit/delete, pet/voice/settings in restrained controls.
* The key product is the desktop companion scene, not a pet-raising simulation.
* Offline, local-first reminders; system tray after window closes.
* Puppy first. Cat and butterfly should be separately authored **after** puppy movement and voice pass device review.
* Earlier walking V1/V2 were explicitly rejected as unnatural. Do not reuse those gait frames or quietly put them back into the stable version.

## Original Puppy 01 rig study preview

![Original Puppy 01 SVG rig study](./puppy-side-rig.svg)

> This is an initial editable **vector draft**, not the cinematic reference illustration or finished walking animation. The premium final art and motion poses are pending.

## Artwork in this folder
* `puppy-side-rig.svg`: original editable SVG **3/4 side-view character design study**, transparent background, independently labeled layers.
* `art-review.html`: local no-dependency page to inspect the rig against different wallpapers and visual scales.
* `CHARACTER_AND_MOTION.md`: color system, anatomy, layer pivots, stance/locomotion plan.
* `REMINDER_PERFORMANCE.md`: scene choreography, voice sync, action, timeouts, accessibility, failure handling.
* `PHASE1_ACCEPTANCE.md`: what exists and what is blocked before Phase 2.

**Generated story images in the conversation are visual reference only**. They are not automatically rig-ready, approved, licensed imports, or included in the Windows runtime. The original source-art master is separate. The current SVG needs artist approval and further expression, side/front/back turnaround and keyframe refinement before it can be called premium final artwork.

## Milestones
1. Phase 1: lock art style, original character model, pivotal animation layers, storyboard and acceptance tests.
2. Phase 2: real transparent **Windows** overlay proof with body travel tied to leg contact; test entry/walk/stop/idle/exit at 60fps where hardware supports it.
3. Phase 3: offline voice pre-render/caching and authentic audio-driven mouth timing; reaction + action persistence.
4. Phase 4: minimal actual app and wired reminders, building on the safe existing scheduler.
5. Phase 5: human-tested Windows installer. Lock/suspend/monitor/DPI/audio/fullscreen checks are mandatory.

## Licensing
Original SVG is project-authored source. Do not claim external concept reference images are authored vector layers. Do not ship or redistribute external media unless its rights are checked. Project root currently has no selected public licence.
