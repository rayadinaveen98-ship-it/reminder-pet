# PetPing 2.0 — Experience and UI Design Specification

**Status:** Phase 1 design only, not implemented. **Baseline:** verified Windows v0.9 unchanged on main.
**Plan:** v0.10 design foundation → v0.11 walking and voice → v0.12 expression/effects → v0.13 pets and customization → v0.14 new WPF UI → v0.15 QA → v1.0.
**Decision:** Windows v1.0 release candidate is deferred until this new experience is completed.

## Product promise

An animated desktop companion walks in from a random safe direction, stops near the middle of the active display, talks about the due reminder, waits for an explicit choice, reacts, then leaves. Display floating reminder **text only** with accessible compact controls: no text box, card or speech bubble behind the words.

Never lose accurate reminder status or interrupt the locked Windows session to make an animation look nicer. Work without accounts or paid services.

## Navigation and screen redesign

| Destination | Core experience | Main action |
|---|---|---|
| Home | Today's overview, active companion, next 3 reminders and Needs Attention summary | Add reminder |
| Reminders | Today, Upcoming, Needs Attention, History, All filters; add/edit/repeat | Save reminder |
| My Pets | Original Cat, Dog and Bunny gallery, active selection and preview | Choose companion |
| Pet Studio | Large live pet preview, appearance, accessories, actions, voice, arrival and effects | Save and preview |
| Settings | Speech frequency and voice, motion/accessibility, quiet hours, placement, startup, local data | Update preference |

Left sidebar 220 DIPs, spacious content panels, consistent navigation, warm minimal whimsical style, not an overcomplicated game. Use plain language, not engineering jargon. Keep reminder task controls easy to discover. Minimum content width target 850 DIPs with responsive layout at increased Windows text scaling. New My Pets/Studio concept views should not touch v0.9 reminder schema.

## Desktop pet overlay: no reminder box

- Pet occupies a small transparent moving WPF window rather than a fullscreen click-catching overlay.
- Reminder text is painted near the pet directly over desktop, with adaptive contrasting text, subtle outline/shadow, max 2–3 lines and accessible full-text name. No filled text container.
- Done / Snooze 10m / Mute / Hide appear in small separate clearly clickable action chips near text.
- Hide is not Done: reminder stays in Needs Attention. Mute stops voice but retains the visible reminder.
- The existing 45-second dismissal becomes a configurable safety behavior. Requested default: wait visibly for a response, with accessible Hide and Mute controls.
- After Done or Snooze, animate success only after the local data save succeeds; if it fails, keep controls active for retry.

## Walking movement

- Select **left or right** screen edge at random for the first release. "Top/bottom" require different animations (hop/drop) and are later pet-specific entrance effects.
- Travel from the edge to around 50% of **active monitor** working-area width, with safe target range 40–60%.
- Preferred motion: 2.0–3.5 seconds, natural acceleration/deceleration, alternating feet, light bounce, facing according to travel direction.
- On arriving, stop, turn toward user, display text/actions and start local Windows narration.
- Motion Gentle: short calm glide. Motion Off or Windows reduced motion: appear directly at destination.
- Handle multi-monitor and DPI changes, laptop sleep/resume, taskbar, foreground exclusive-fullscreen apps; never steal focus or block the whole screen.

## Spoken reminders (behavior matters more than lip motion)

"Keep speaking until user chooses" means **re-announce the due text with configurable silence**, not an endless simultaneous stream of talking. Initial default 60 seconds between completed announcements; offer 30/60/120 seconds and Mute. After arriving: "Hey! Time to start editing your video." Wait for speech completion, then wait full interval. Repeat only while alert active and speech allowed.

- One active reminder actor + one voice owner, no overlapping TTS, no overlapping pets. Queue simultaneous reminders.
- On Done, Snooze, Hide, Mute, Windows session lock, sleep or quiet hours, cancel TTS promptly.
- A distinct reminder session identifier prevents stale SpeakCompleted events from animating a later pet.
- Speech is optional. If unavailable, text and interaction still work. Never mark a reminder Done just because audio played.

## Pet customization system

Curated starter pets are original **Cat, Dog, Bunny**, with compatible behavior manifests; no copyrighted character packs.

Each saved profile will specify pet ID/name/species, color palette, optional accessory IDs, size, voice selection/rate, entry direction, idle action, arrival action, Done action, Snooze action, exit action, effects intensity, motion level and speech repeat interval. Profile schema versioned with validation. Stored separately from reminders; migrate old pet-appearance.json without losing settings.

| Pet | Arrive | Idle | Speak | Done | Snooze |
|---|---|---|---|---|---|
| Cat | walk | blink, tail, stretch | head tilt, mouth | hop + stars | yawn + sit |
| Dog | bounce-walk | wag, sit | ears + mouth | happy hop + confetti | floppy yawn |
| Bunny | short hop | ears, blink | tiny mouth + ears | jump + sparkles | curl + sleep |

Selection is limited to supported pet actions; impossible combinations should not appear. Live preview for Walk, Talk, Done, Snooze, Idle must use synthetic text, never trigger the real reminder database. User-imported art packs are *later* and must be sanitized media-only, never executable code.

## Visual system

See design/petping-2/tokens.json. Warm ivory background, ink text, muted lavender primary, apricot pet accents, mint positive accent. Font: Segoe UI fallback. 8px spacing rhythm, card radius 18, clear focus rings. Pet Studio should prioritize the live pet stage, not a grid of dense technical fields. Respect reduced motion and battery use.

## Implementation architecture proposal

Separate ReminderScheduler and ReminderStore from the new UI. Introduce:
- ReminderPresentationCoordinator: queue, active reminder, lock and power cancellation.
- PetActor: renders one pet rig and supported action state.
- PetMotionController: bounded path and walk timing, frame-rate/power policy.
- PetSpeechController: one cancellable speech/repeat loop with session IDs.
- PetActionController: Done/Snooze persistence, success reaction only afterward.
- PetProfileCatalog and PetProfileStore: packaged pet assets and validated settings.
- PetStudioView + desktop NavigationShell: customizable UI and preview.

Existing v0.9 native WPF code has a 425x236 popup and single TTS utterance. The moving actor should not be implemented by just stretching that popup across the entire desktop: that would create bad hit-testing and layout behavior.

## Quality gates before approving later engineering

- Walks from both directions and stops within the target screen zone; natural step motion.
- No boxed message background and text readable over both light and busy wallpapers.
- Repeated narration does not overlap and immediately stops with Mute/Done/Snooze/Hide/lock/suspend.
- No invisible window intercepting unrelated clicks; keyboard access preserved.
- SFX and particle effects are optional and reduced-motion-safe.
- Cat/Dog/Bunny use original consistent art and behavior-specific cycles.
- Profile persistence and migration do not modify existing reminder data.
- Tests: single instance, repeat schedules, sleep/wake, DPI 100–200%, multiple displays, offline speech and disk write failure.

## Phase 1 acceptance

- [x] Product screen map and UI language specified.
- [x] Pet overlay, voice repeat and input safety contracts specified.
- [x] Customizable pet behavior system and supported launch pets specified.
- [x] Design tokens and sample pet profile drafted.
- [x] Animation state/storyboard documented separately.
- [ ] Review Canva editable mockups and record design link.
- [ ] Approve design direction before modifying Windows app.

See docs/PET_EXPERIENCE_2_STORYBOARD.md and docs/PROJECT_STATUS.md. v0.9 stays the functional fallback build until new Windows CI and manual QA pass.
