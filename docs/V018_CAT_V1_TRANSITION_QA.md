# Cat V1 — end-to-end animation transition and reminder QA

**Implementation:** draft experimental descendant of Cat Talking V1 (PR #29). This is a source and laptop-preview candidate, not a stable v0.18 release.

## Fixed in this phase
1. **Walk → Idle → Talking → Done/Snooze:** A pure `CatAnimationFlowRules` transition table now enforces stages: Stopped, Idle, Walking, Talking, Done, Snoozed. Only explicit Reset begins a fresh cycle.
2. **Late voice events:** Canceled/completed/snoozed/stopped cats ignore queued SpeechStarted/SpeechStopped and word cues. Mute also blocks queued VoiceStarted animation in the popup after the user muted.
3. **Response frame freeze:** Previously `PetCharacter.React` called `Stop()` before starting a Done/Snooze expression. That is incompatible with a real terminal-state machine and risks an idle/talking frame remaining onscreen. The cat reaction now resets movement **without stopping** its renderer; then enters Done (happy) or Snoozed (sleepy) before its timed exit. Dog/Bunny still execute the original Stop and reaction path.
4. **Gentle walking:** The floating window already moves in Gentle mode, but previously the cat stayed in a seated idle pose. Gentle mode now uses the existing slower premium walking sprite loop only when premium walking art is available. Older Dog/Bunny and classic cat still use their previous reduced-motion behavior.
5. **Stop and reset:** Explicit close/Stop reaches Stopped stage, prevents resume from hidden/visibility changes and stops the frame timer. Applying the next pet profile explicitly resets to Idle. Pure logic can be tested on Linux/Windows without WPF.

## Regression checks
- Complete entrance, arrive, speak, stop speaking, Done.
- Snooze while speaking, ignore late voice and action events.
- Stop/close blocks all queued callbacks until explicit reset.
- Duplicate Arrive/SpeechStopped are harmless.
- Invalid enum stages/events fail closed.
- Core reminder tests, WPF build, isolated Inno installer and data-preservation smoke still required.

## What this does *not* claim
- No phoneme/viseme lip sync; word cues remain the approximation from Cat Talking V1.
- Static 2.5D walk frames are not rigged four-leg locomotion.
- PNG assets are still distributed with the separate user-facing test ZIP, not compiled into GitHub's Windows CI artifact. Full release packaging of source binary artwork is a separate future task.
- Actual visual continuity, mouth pacing, voice, scaling (100–200%) and wake/sleep behavior on a real Windows device still need user acceptance.
- Production main/v0.17 reminders and profiles are unchanged.

## Manual laptop verification
1. Quit existing *PetPing Experimental* from tray. Install the approved complete test ZIP (9 PNG assets) or launch its portable build.
2. Choose Cat + Apricot + no added accessory. On My Pets confirm all three Premium Cat art categories loaded.
3. With motion **Full** and then **Gentle**, trigger a preview reminder: premium Walking should show until arrival, followed by a stable Idle pose.
4. With voice on, check mouth response to each spoken reminder and that it settles to Idle when the voice completes. Test speaker muted during a word, and voice repeated after one minute without old stale talking frames.
5. Select Done; verify happy reaction then exit. Repeat and select Snooze; verify sleepy reaction then exit. Ensure the reminder is saved and no animation stays running.
6. Test **Off** motion: no walking/breathing/mouth motion, actions remain functional.
7. Test a Dog or Bunny and a Cat with a non-default palette/accessory: current vector art should remain and function.
8. Report actual device observations before declaring release ready.
