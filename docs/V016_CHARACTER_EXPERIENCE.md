# PetPing — Character and Performance Bible (v0.16)

**Status:** Approved direction; exact artwork, rigs and timing are prototype targets, NOT delivered Windows assets.
**Implementation track:** v0.19 after v0.17 visual foundation and v0.18 reminder hardening.
**Baseline:** v0.15 contains real, original WPF Cat/Dog/Bunny vector rigs and basic effects; this specification raises their quality bar.

## A. A truly crafted character family

A premium **2D storybook** feel: warm, expressive, clearly legible at 64–200px, consistent line weight, lighting, proportions and animation design. Distinctive expressions and anatomical detail, not AI imagery pasted over the existing actor.

| Animal | Appearance and motion signature | Emotional tone | Standout gesture |
|---|---|---|---|
| Milo the Cat | Pointed ears, expressive whiskers, long elastic tail, deliberate footfalls | Alert, knowing, warm | careful paw wave, subtle glance and graceful exit |
| Buddy the Dog | Floppy ears, muzzle, substantial foot contact, lively tail | Caring, enthusiastic | excited bounce, sit, ears-flop on stop |
| Luna the Bunny | Tall articulated ears, round hind feet, genuine squash/stretch | Curious, playful | distinct hop anticipation, high celebration, sleepy ear droop |

**Non-negotiable:** unique gaits, anatomy and core poses for each species. Recoloring one animation doesn't count as a new character.

### Art acceptance sheet per pet
1. Black silhouette recognizable at 64px, full form crisp on 4K/high DPI.
2. Consistent neutral, happy, focused, curious, sleepy and concerned expressions.
3. Pivot-ready layered source: head, body, paired eyes, mouth shapes, left/right paws or feet, ears, tail and accessory anchors.
4. A convincing 2–4 key-pose walk/hop with true landing and foot-contact.
5. Correct talking mouth states synchronized to actual TTS events; no random autonomous jaw motion while silent.
6. Accessories avoid eye occlusion/clipping and work for each species.
7. Neutral static alternative for Motion Off.
8. License and originality review; no recognizable third-party mascot or unverified stock asset.

## B. Customization tiers

**Tier 1 — dependable:** species, independently saved palettes and outfit/accessory slots, size, personality (Playful, Calm, Curious), motion Full/Gentle/Off, Done/Snooze choice, effects Off/Gentle/Playful and speech recurrence.

**Tier 2 — rich Studio:** curated eye/ear variations suitable to species, coat/pattern overlays, user pet nickname, selected idle and entrance modes, voice selection and voice rate if Windows installed voices allow, per-pet audio/effects tuning. Each active control must work in both live preview and actual alert and persist across app restart.

**Tier 3 — post release:** many additional animals, custom packs/artist imports, seasonal themes and optional pet-raising extras. Import must accept safe media + validated manifest only, never arbitrary executable scripts.

Never show a customization control as usable until both live preview and real reminder actor support it. Preserve v0.15 PetProfile JSON with additive versioned migration.

## C. Full cinematic mini-scene for every reminder

| Scene | Duration target | Visual | Sound & user control |
|---|---|---|---|
| Queue | Immediate | one active pet scene; choose safe active monitor | no voice before ready |
| Enter | 0.25–0.45s | pet peeks from safe side with anticipation | optional soft enter sound |
| Walk / hop | 1.8–3.2s | unique pet feet and weight-shift, responsive ears/tail | skip for Motion Off / urgent accessibility |
| Settle | 0.2–0.5s | stop in safe mid-screen zone; face user | floating **unboxed** reminder text and Done/Snooze/Mute/Hide appear |
| Speak | length of phrase | subtle head tilt and mouth sync to voice events | local TTS; cancellable, optional |
| Await | until action / session interruption | low-energy nonrepetitive idle | reannounce after selected silence; always Mute/Hide |
| Done / Snooze | 0.4–0.95s | species-specific hop/wave or yawn/stretch, finite sparkles | only after persisted action succeeds |
| Leave | 0.45–1.5s | turn/walk away or gentle disappear | queued reminder may follow |

These are design targets, not current measured v0.15 latency.

### Engineering state contract
Scene states: Hidden → Entering → Travelling → Settling → Presenting → Speaking/Waiting → Reacting → Leaving. Suspension cancels voice and ensures correct queued state on unlock. A stable session token owns voice, animation and action results. Stale animation/speech callbacks from a previous reminder cannot affect a later pet.

- Render in bounded pet-sized WPF window, not full-screen click-catching transparent overlay.
- Never steal focus on entry; preserve usable keyboard focus if user intentionally interacts.
- Monitor work area and per-screen DPI are live inputs; recompute target on screen change/disconnect and clamp text/buttons.
- Actual reminder text stays visible/readable even if actor frame drops or no Windows speech voice exists.
- Errors do not mark tasks Done. Save completion must precede pet victory animation.
- Only one active voice and active pet at a time.

## D. Text readability and accessible contrast

Default: floating words without speech bubble, using ink/white adaptive text, contrast outline, shadow, sensible wrap, no random 3-line truncation of critical message. Controls may have opaque rounded backgrounds to retain clickability.

Test light/black/photographic wallpapers, bright movies, desktop icons, high DPI 100–200%, Windows enlarged text, remote display. Provide **optional accessibility high-contrast support** if unboxed text cannot pass contrast reliably; never claim adaptive text always succeeds on every image.

## E. Effects, sound and CPU budget

- Pet-themed star/confetti Done celebration; sleepy Snooze motes; subtle grounded arrival effect.
- V0.15 limit of **at most 10 finite particles per reaction** remains the starting constraint. More isn't automatically better.
- Gentle removes bright particles and shortens amplitude; Motion Off means no movement/particles.
- No flashes faster than 3 per second; no large foreground light bursts or seizure-risk strobing.
- Optional local sound FX controlled separately from voice, never autoplay during onboarding without action.
- Aim 60fps during active pet scene on target devices; profile sustained frame times, CPU, memory on a typical Windows laptop. Reduce animation/update rate when battery-saving, lock or hidden. **No benchmark has been performed here.**
- Clean up every animation, timer and effect when actor closes, user changes pet, machine locks or sleeps.

## F. Art + QA pipeline

1. Brand character style frames: each species silhouette, expressions, palette and accessory anchors.
2. User art approval; keep Canva/Figma visual inspiration separate from own rig assets.
3. Prove **one complete cat scene** (Enter → Speak → Done → Leave) with real text/voice on Windows.
4. Port motion language to Dog and Bunny with truly different gait and response.
5. Compare actual Pet Studio preview against on-desktop actor at two sizes and DPI values.
6. Test avatar scales, 20 repeated previews, continuous reminder queue, battery and sleep/wake; measure memory/timer cleanup.
7. Provide static pet/text fallback if high-quality rig fails; never lose scheduled reminder delivery.

## G. Gates to lock and ship

- **Art lock:** original consistent approved rig-ready character sheets and vector/layered assets.
- **Scene lock:** recorded on-device entrance, waiting, Done and Snooze for each species, readable text and no blocked app controls.
- **Persistence lock:** changes per pet survive reboot and user profile migration safely.
- **Release QA lock:** actual Windows run verifies no leaked effect timers, smoothness, visibility, accessible voice-off, animations disabled.

**No v0.16 character illustration or working art rig has yet been shipped.** This document sets the testable brief for v0.19.
