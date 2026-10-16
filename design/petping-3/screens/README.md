# PetPing v0.17 — Static Native Desktop Screen Compositions

These **source-editable SVG visual concepts** are precision-drawn design references, not screenshots of the currently installed WPF app, not exported pet character rigs, and not simulated working buttons.

- [Home concept](v017-home-concept.svg) — truthful five-screen navigation, + Add reminder, local-ready indicator, current-pet visual space, Today / Upcoming / Needs attention, next reminders. Illustration and all task names/numbers are explicitly sample data.
- [Reminders concept](v017-reminders-concept.svg) — quick-create title/date/time, recurrence choices, list filters, multi-row reminders and future More options (v0.18) placeholder.
- [Implementation specification](../../../docs/V017_UI_SPEC.md) — exact hierarchy, text/contrast requirements, WPF DIP layout targets and user approval gates.
- [Editable Canva review](https://www.canva.com/d/rljAMk15QUasrej) — separate eight-slide exploration, includes automatically generated **off-product** pet-care/health/online references that are not approved and must not enter the implementation.

**Design safety:** sample task rows are visual examples only and must NEVER be seeded into real `reminders.json`. WPF v0.15 functionality and local profile files are unchanged by this stage. First approve screen direction/spacing; only then translate these pieces into reusable WPF ResourceDictionary and native pages with real data.

**Visual QA before finalizing:** review at 810×640 DIP minimum, 100/125/150/200% Windows scaling, 200% font scale, with and without large pet, high contrast and keyboard focus. Do not claim these two SVGs prove responsive WPF rendering.
