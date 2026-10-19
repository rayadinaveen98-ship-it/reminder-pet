# Character bible & animation prep — Puppy 01

## Personality
Warm, attentive, energetic but not chaotic. The companion appears with a sense of purpose and respects the user. Avoid overacting during work reminders. Its eyes lead the head, head leads shoulders, and tail follows the spine with delay.

## Visual dimensions and rig
Master source: `puppy-side-rig.svg`, 560 x 440 viewBox, transparent canvas; body contact line near y=382. Characters are designed for a 150–240 Windows-DIP actor footprint and must be visually checked at 64, 96, 160, 240 CSS px and on 100%, 150%, 200% display scaling.

Independently editable semantic groups and pivot coordinates (in source SVG units):

| Group | Pivot approx. | Expected controlled movement |
|---|---|---|
| tail | 169,236 | small delayed follow-through, damping |
| hind-leg-far | 220,280 | planted foot / rear swing |
| body | 290,258 | small pitch and vertical weight transfer |
| hind-leg-near | 210,290 | opposing rear foot cycle |
| foreleg-far | 350,280 | opposite front foot cycle |
| foreleg-near | 381,281 | leading planted foot then recovery |
| head | 390,227 | eyes-first orientation, restrained nod |
| ear-near | 347,144 | floppy follow-through after head movement |
| ear-back | 427,144 | independent small follow-through |
| eyes | move pupils within outline | gaze shift / blink; no impossible eyeball slides |
| mouth-idle, mouth-talk-small, mouth-talk-open, mouth-happy | head local | mutually exclusive speaking/reaction shapes |
| collar | follow shoulder, delayed tag swing | accessory secondary motion |

Current artwork is a **single design study**, not full production sprite pack. Eyes and mouth layers are separately selectable, but blinking eyelids and alternate 3/4-front/side/back sheets must still be authored. Animation tools may rasterize the grouped master to independent transparent textures; never flatten before extracting part pivots.

## Palette
* Apricot fur: #F7BD6F to #D9843D
* Cream highlights: #FFF9E9 to #F6E0BC
* Cocoa facial definition: #33231F / #48322E
* Ear warmth: #BE743F to #9F552F
* Collar: #AE5548; tag gold: #E9B75E
* UI backdrop: warm ivory #F8F6F2; high contrast ink #24272E
These are provisional design tokens, not the final user-approved palette.

## Foot-contact gait contract
1. The moving actor's x coordinate is derived from **physical step distance**, not an unrelated slide tween. A foot remains planted in world/screen space during stance.
2. Four feet have explicit contact intervals, with near/far foot ordering and crossing controlled in depth; at least one pair carries weight at slow walk.
3. Torso motion is a response to loading/propulsion (small vertical lift and pitch), **not** permanent sinusoidal bobbing when idle.
4. Head counter-rotates to stabilize gaze; ears and tail apply delayed spring-like response with damped amplitude.
5. Before reaching destination the pup decelerates, takes a short finishing step, places paws, settles and looks at the user. No instantaneous walking->sitting jump.
6. Turns must use separate authored head/body orientation poses, not horizontally flipped half-turn frames. If a full turn is not authored, exit facing outward with a carefully composed directional transition; do not claim a 3D turn.
7. Speaking begins only once the actor has settled; idle breathing is subtle and irregular, not a metronomic body float.
8. Separate reduced-motion/no-motion accessibility variants are first-class.

### Key poses still to draw
Contact left; passing left; contact right; passing right; decelerate; stop/plant; look up; eyes half blink; eyes closed; speak small/open; happy confirmation; sleepy snooze; leave/turn. Draw them as joint-layered authored poses and inspect seams/outline continuity.

## Performance acceptance (not yet measured)
* Desired active render target 60fps on test laptops without sustained excessive CPU/GPU/memory.
* Review slowed to 0.25x; reject visibly sliding paws, popping knees, detachments, clipped ear/tail, or position discontinuities.
* No pet-over-fullscreen-game forced overlay by default; safe notifications fallback.
* Require screen recording from real Windows on at least two DPI settings before art/motion lock.

## Art quality gate
Render at 64px silhouette, light and dark backgrounds, photographic wallpaper, 1080p and 4K high-DPI. Check outlines, contrast, eye highlights, foot contacts, tag collision, and facial readability. If any basic drawing/joint quality fails, fix that **before** turning it into animation.
