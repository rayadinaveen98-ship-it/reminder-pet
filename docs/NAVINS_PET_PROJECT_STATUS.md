# Navin's Pet — project continuity/status

_Last updated: 2026-10-19. Branch-only experimental work; this document is not present on stable main until a separately approved merge._

## Product decision
- **Name:** Navin's Pet
- **Platform:** Windows desktop **first**, no cloud account or paid service required for core prototype.
- **Promise:** minimal, quiet reminder application with a beautifully drawn pet that genuinely enters, travels, stops, speaks a reminder name naturally, waits, responds to Done/Snooze, and leaves.
- **Style:** clean warm-ivory small one-window application; premium illustrated animal, not pet-care game, dashboard or busy widget.
- **Pets roadmap:** puppy first; cat and butterfly only after puppy reaches quality gate.
- **Critical animation requirement:** stable foot contact and actor translation tightly coupled; otherwise reject the gait. User previously rejected PetPing Cat V1/V2 walking as unrealistic, and now explicitly requests a fresh, higher-quality motion attempt for Navin's Pet.
- **Do not replace existing approved Cat/Dog/Bunny**, change reminder data or use/reintroduce rejected walking experiment assets in production.

## Work delivered 2026-10-19
- Isolated development branch `feature/navins-pet-v01-character-foundation` based on main commit `9fdb4b289a3d7f569a811cd65557c5e282c41523`.
- Draft PR #35: https://github.com/rayadinaveen98-ship-it/reminder-pet/pull/35 .
- Original 560×440 transparent SVG 3/4-view **draft** puppy rig, editable groups for body, tail, four legs, head, ears, eyes and alternative mouth states.
- Static HTML art inspection utility for multiple desktop-background colors and sizes.
- Character anatomy/motion specification, reminder performance state machine and phase-gate QA checklist.
- Six design files re-read from the branch; basic SVG start/end tag structure balanced with 12 key groups detected. **This is not a compiled or rendered device QA pass.**

## Design source paths
- `design/navins-pet/v01/README.md`
- `design/navins-pet/v01/puppy-side-rig.svg`
- `design/navins-pet/v01/art-review.html`
- `design/navins-pet/v01/CHARACTER_AND_MOTION.md`
- `design/navins-pet/v01/REMINDER_PERFORMANCE.md`
- `design/navins-pet/v01/PHASE1_ACCEPTANCE.md`

## Still pending — do not claim complete
1. Final high-fidelity original character art with consistent front/side/rear and correct articulation; screenshot concept generation does not provide editable animation assets.
2. Genuine contact/passing/stop/turn key poses and reviewed walk cycle. No operating Windows movement yet.
3. Real WPF transparent actor loop, idle/speech/confirm/exit, per-monitor focus/DPI/lock tests.
4. Offline neural voice evaluation and text-to-audio caching with waveform/phoneme-based mouth behavior.
5. Minimal main app shell and proven scheduler integration.
6. Installer + physical Windows UX and reliability pass.

## Next executable step
Refine/approve the source art anatomy. Author separated contact and passing poses and build a nonproduction WPF pet motion test harness under the same experimental branch. Keep gait unmerged until recorded Windows motion (normal speed and 0.25x) passes. Then voice + full reminder proof and only then dashboard integration.

## User expectation management
User asked to proceed immediately and values quality over superficial velocity. Give truthful committed status, direct review links, and actual Windows motion proof before saying walking is complete. **Do not equate a concept image or art board with a working animation.**