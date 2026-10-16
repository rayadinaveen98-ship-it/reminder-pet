# PetPing 2.0 — Design workspace (Phase 1)

## Editable Canva concept

**Design:** [PetPing 2.0 UX — 7 editable pages](https://www.canva.com/d/26YBWLCIc7nE3nk)
**View:** https://www.canva.com/d/9HpQCIoGuCeN6Jp
**Canva design ID:** DAHYKnkPXWo
**Page count:** 7
**Design source of truth for implementation:** docs/PET_EXPERIENCE_2_DESIGN.md, not the generated illustration text.

Pages:
1. Home dashboard
2. Reminders
3. My Pets
4. Pet Studio
5. Preferences
6. Animation storyboard
7. Design system

The Canva design is a **visual reference, not final signed-off implementation artwork**. It was generated from the specification and needs user visual review. The generation included some generic pet-care reminder examples (vet appointments, pet meals, allergy medication) that are **not** our intended product scope: PetPing reminds its **human user** about work, study, daily life and anything else, with a virtual pet delivering those reminders. Those example labels should be replaced in the next design revision before WPF implementation. The Preferences mockup also shows placeholder repeat timing that must follow the spec: 30/60/120 **seconds**, not minutes.

## Design tool choice and budget

- Figma connected account currently reports **Starter / View seat**, without Code Layers, so we should not rely on it as a writable shared workspace.
- Canva design creation succeeded using the existing connected account; no extra subscription was requested.
- Final application does not run Canva or Figma. WPF implements the approved UI natively with local code and owned art.
- Keep all important spacing/color/action information in versioned Git files so tools and design access cannot block future development.

## Repository deliverables

- docs/PET_EXPERIENCE_2_DESIGN.md — feature behavior, UI architecture and pet system
- docs/PET_EXPERIENCE_2_STORYBOARD.md — frame beats, state machine, animations and interaction safety
- design/petping-2/tokens.json — reusable visual and motion system
- design/petping-2/pet-profile.example.json — validated future pet profile proposal
- This README — editable design link, caveats and exact page mapping

## Pending decisions for design signoff

1. Does the Home/Reminders/Studio navigation feel friendly and simple enough?
2. Prefer neutral warm ivory + lavender + apricot, or a different theme?
3. Is floating unboxed text readable and are Done/Snooze/Mute/Hide controls discoverable?
4. Cat/Dog/Bunny launch set and expressive personalities approved?
5. Re-announcement after 60 seconds by default with custom interval/mute: okay?
6. For walking direction, first engine supports random left/right. Are behavior-specific top/bottom hops for a later version acceptable?

After feedback, revise mockups and lock Phase 1. Phase 2 should implement a **safe small-window walking pet engine** with a separate TTS/session controller while preserving Windows v0.9. Do not prematurely declare v1.0 ready.
