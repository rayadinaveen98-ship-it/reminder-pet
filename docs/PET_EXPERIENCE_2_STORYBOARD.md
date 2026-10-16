# PetPing 2.0 — Walking, Speaking and Reaction Storyboard

**Status: UX / animation storyboarding only. No moving pet engine has yet been built.**

## Eight-beat reminder scene

| Beat | Pet animation | Text, sound and controls | Completion rule |
|---|---|---|---|
| 1. Due | Agent selects its pet/monitor and a safe entry edge | No abrupt foreground app switch | Reminder store has persisted Delivered before visuals |
| 2. Enter | Pet peeks in from a random left/right edge, turns toward goal | Optional soft arrival cue | Never intercept clicks across whole desktop |
| 3. Travel | Visible alternating feet, body bob and species-specific gait across roughly half monitor | Small optional footsteps, no full reminder narration yet | Target roughly 50% horizontal working area |
| 4. Settle | Character brakes gently and looks toward user | Text appears as **floating unboxed words** with subtle shadow | Show Done, Snooze, Mute, Hide controls |
| 5. Speak | Mouth, head, tail/ears respond to TTS events | “Hey! Time to start editing your video.” | Use local Windows speech, cancelable per reminder ID |
| 6. Wait | Idle blink/stretch, never a frozen sticker in Playful mode | Repeat after configured pause until action; Mute silences | One TTS at a time; no sound during lock/suspend/quiet hours |
| 7a. Done | Small jump, sparkles/confetti depending on pet | Optional “Nice work!” | Only after Completed saved to disk |
| 7b. Snooze | Yawn, settle, wave | Optional “Okay, later!” | Only after next due time saved |
| 7c. Hide | Brief wave then leave | Speech stops | Reminder stays Needs Attention |
| 8. Exit | Pet turns and walks away; actor window disposed | No voice by default | Then show next queued reminder |

## Screen choreography

Start coordinate: random left or right just inside chosen active monitor work area.
Destination: horizontal center zone, measured in device-independent pixels; avoid taskbar and known reserved desktop edges.

- Full motion: 2–3.5 sec walk, ease in/steady/ease out, footstep loop, head/tail/ear response.
- Gentle motion: shorter glide with occasional blink; minimal particle effects.
- Motion Off: static character appears at destination without walking or particle effects.
- Do **not** use a fullscreen transparent input window for the walk; move a pet-sized nonactivating transparent window or equivalent shape-limited presentation region.
- Text arrives once the pet settles, rather than covering unrelated app content during movement.

## State diagram

~~~mermaid
stateDiagram-v2
  [*] --> Queued
  Queued --> Entering: reminder agent ready
  Entering --> Walking: full motion
  Entering --> Presenting: motion off
  Walking --> Presenting: center reached
  Presenting --> Speaking: voice allowed
  Presenting --> Waiting: voice disabled
  Speaking --> Waiting: utterance finished
  Waiting --> Speaking: next repeat interval
  Speaking --> Waiting: Mute
  Waiting --> Reacting: saved Done or Snooze
  Speaking --> Reacting: saved Done or Snooze
  Reacting --> Exiting: reaction complete
  Speaking --> Exiting: Hide
  Waiting --> Exiting: Hide
  Walking --> Suspended: lock / sleep
  Speaking --> Suspended: lock / sleep
  Waiting --> Suspended: lock / sleep
  Suspended --> Queued: unlock / resume
  Exiting --> [*]
~~~

**Save failure rule:** remain actionable; do not show success reaction and do not mark reminder handled. **Speech failure rule:** keep visible text and buttons.

## Voice session ownership

One presentation session owns reminder GUID, pet ID, a generation token, timer for repeat, cancellation mechanism, and active utterance. SpeakStarted/SpeakCompleted must match the session; ignore stale callbacks. Stop voice on Done, Snooze, Hide, Mute, quit, lock, suspend, and (if configured) quiet hours. Repeated announcements should be separated by silence (recommended default 60 seconds) to avoid continuous intrusive audio.

## Character action packets

Cat: confident steps → blink/head tilt → paw wave → jump & tiny stars → turn and walk away.
Dog: jaunty stride → wag & ear tilt → sit → joyful bounce/confetti → trot out.
Bunny: hopping entry → ear twitch → attentive posture → high hop/sparkles → hop out.

All pet actions have: animation name, frames or bone transformations, duration, optionally allowed SFX, allowReducedMotion fallback, and an interrupt/cancel target. Users select **supported** pet actions; unrecognized action IDs fall back to safe idle.

## Test coverage to add during v0.11/v0.12

1. Random left/right without repeated off-screen motion; ends in 40–60% center zone.
2. 60-second post-speech repeat pause, cancellation stops audio immediately, no overlapping pet or voice when two reminders trigger.
3. Done/Snooze triggers saved status then reaction; failed disk write keeps choice active.
4. Mute stops voice, Hide leaves Needs Attention; Motion Off never walks.
5. WPF scaling at 100/125/150/200%; multi-monitor switches mid-walk.
6. Lock/suspend cancels presentation, no secure lock-screen popup; appropriately restores after unlock.
7. Text readable on light/dark/busy backdrops; button keyboard focus visible.
8. Tests measuring performance and verifying no leaks from animation timers after actor disposal.

No screen or sound assets are required from users for this design; original character artwork will be finalized in the later animation/art milestone.
