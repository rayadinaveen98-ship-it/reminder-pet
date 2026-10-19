# Navin's Pet — complete reminder performance contract

## Core story (ordinary reminder)
1. **Due:** scheduler atomically selects only one active reminder/session; never mark Done on display.
2. **Preflight:** load pet, voice/cached speech, monitor work area, DPI and user preferences. If scene unavailable, use fallback notification and retain Needs Attention state.
3. **Enter (0.3–0.6s target):** small anticipation from screen edge, only after overlay is ready; no stealing focus.
4. **Travel (1.2–2.5s target):** physically believable authored gait tied to translation, paws landing in correct positions, no fake scrolling sprite.
5. **Settle (0.2–0.4s target):** decelerate, paws plant, look toward user; plain text and two controls become visible.
6. **Speak (audio length):** e.g. “Hey, time to work on your screenplay.” One natural utterance. Mouth timing derived from real audio/viseme data where possible; if only amplitude is available, explicitly label that level of sync as an approximation.
7. **Wait:** soft breathing, occasional blink and look; no looping full-body bounce; Done, Snooze, Mute, and close/hide always accessible.
8. **Done (0.5–1.0s target):** persist completion FIRST. Then brief happy ear/tail/paw reaction. If persistence fails, keep reminder actionable, show error and never play success.
9. **Snooze (0.5–1.0s target):** persist next due time FIRST. Then a small understanding reaction; don't act disappointed or guilty.
10. **Leave (0.8–1.8s target):** turn/look out, walk away with real foot contact, clean up timers/audio/rendering. Then start next due reminder if queued.

Targets are *design targets*, not yet measured Windows implementation timings.

## If the user ignores it
* After configurable ~20s default, the pet can give one restrained reminder **only if reannounce is enabled**; never loop incessantly. Default should favor minimal disturbance.
* On auto-dismiss around 45s, hide scene without completing task; mark as Unanswered / Needs Attention, not Done. Hover and direct input may pause dismissal. The user must always be able to dismiss/mute immediately.
* When more than one reminder is due, do not stack pets, overlay voice, or pile up cards. Serialize and expose a compact “2 more reminders” indication without verbose reading.

## Scheduling reliability
Local-first persistent scheduler runs in tray, restoring due/overdue items after crash/restart. System may sleep or lock; no guarantee that speech or overlay will run during secure lock or shutdown. Resume/show missed reminders after unlock. Use stable reminder and occurrence IDs, session cancellation tokens and monotonic animation clocks.

## WPF overlay and screen behavior
* Bounded transparent top-level window, not a full-display transparent input blocker.
* Noninteractive surface hit-test transparent when possible; only controls intercept clicks. Do not take focus on enter or while speaking.
* Safe screen region chosen based on work area/foreground monitor; do not obstruct critical screen regions in video editing apps. Respect taskbar and fullscreen-app detection.
* Scale dynamically for per-monitor DPI. On disconnect, resume within active monitor bounds.
* Reduced motion disables travel/secondary effects; still announce. Voice Off, sound Off, text-only all functional.
* If renderer, art, or speech fails, ordinary visible actionable fallback persists. Acknowledgment always tied to durable save.

## Voice plan (not built in this phase)
* Evaluate local, commercially usable offline neural synthesis for pronunciation, licensing, size and cold start. Benchmark generated audio caching at reminder creation/edit time.
* Store/secure cache using a stable hash of text + voice model/version/options, invalidate on edit. No personal reminder names in diagnostics.
* Only one active audio output. Cancel/duck voice for lock, mute, next session or notification dismissal.
* Prebuilt voice amplitude envelope can drive 2–4 simple mouth shapes when real phoneme timing is unavailable; it is NOT phoneme-level lipsync. Prefer actual timestamps if voice engine supplies them.

## Finite-state machine
```
HIDDEN -> PREPARING -> ENTERING -> TRAVELLING -> SETTLING
       -> SPEAKING -> WAITING -> {SAVING_DONE | SAVING_SNOOZE | DISMISSING}
       -> REACTING -> LEAVING -> HIDDEN
                            |
                        NEXT_QUEUED
```
Save error transitions back to WAITING with explicit retry. Windows suspend/lock cancels scene and queues a catch-up event; do not resume a stale voice or old animation callbacks. Each state change checks its unique session ID and cancellation token.

## Preview acceptance
For any putative “walking works” claim, provide actual desktop screen-recording of full entry, travel, stop, speaking, Done, exit and Snooze alternative. At least 20 consecutive scenes, mixed reminder titles, different DPI, user activity in another foreground window, and safe dismissal.
