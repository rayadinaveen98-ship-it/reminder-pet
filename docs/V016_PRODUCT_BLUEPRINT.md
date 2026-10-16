# PetPing — Product Blueprint 3.0 (v0.16 planning baseline)

**Status:** APPROVED DIRECTION / SPECIFICATION IN PROGRESS — design and engineering deliverables, **not an implemented Windows feature release**.
**Decision date:** 2026-10-16.
**Working product name:** PetPing; public trademark/name check and user approval required before final brand lock.
**Implementation baseline:** Windows v0.15, source merged to `main`; 122 CI core checks, Windows package and real v0.14→v0.15 synthetic-data upgrade passed. User reports the overall existing app works; formal per-device accessibility, audio, mixed-DPI, power-cycle acceptance remains outstanding.

## 1. North star

**PetPing is an offline-first, dependable reminder companion that brings a thoughtfully crafted virtual pet into an ordinary desktop workflow only when useful.**

Functional promise: **remember what matters, show it clearly, and let the user confidently act**.
Emotional promise: a **recognizable companion with warmth and personality**, never a nag, toy overlay, or guilt machine.
One-line introduction: **“Your reminders, with a little personality.”** (working copy, not final trademark/tagline).

**Product category:** local-first personal reminder / task utility with an optional animated character delivery layer. It is NOT a Tamagotchi or social game and should not ask users to feed a pet, accrue coins, or spend money to get dependable reminders.

## 2. Target audience and jobs to be done

**Primary:** Windows laptop/desktop users who work or study with other apps open; creators, students, freelancers, remote workers, and people maintaining personal routines.
**Secondary:** casual desktop users who need approachable, visual scheduling and optional spoken prompts.

Jobs:
1. “When I need to remember something later, create it in seconds with confidence that the selected date, recurrence and timezone are right.”
2. “When a reminder is due, let me notice it without losing my context or keyboard focus.”
3. “If I’m busy, let me Snooze, Hide, Mute or Complete clearly without deleting or accidentally resolving it.”
4. “Make the pet feel expressive, personally mine, and smooth—without stuttering, obscuring important controls, or repeatedly talking when I want quiet.”
5. “After sleep, restart or an upgrade, never silently erase my tasks or pretend delivery happened on time.”

**Non-goals:** guaranteed delivery with app quit / PC shutdown; running over Windows' secure lock screen; AI chatbot, pet leveling, forced accounts, cloud sync, purchasable decorations, invasive usage analytics.

## 3. Locked product principles (design & engineering constraints)

- **Reliability outranks delight:** precise persisted reminder state first, pet animation second, voice/effects third. The pet must not render a success reaction before durable Done/Snooze.
- **Beautiful but legible:** motion and painterly art cannot reduce reminder readability. No filled speech bubble behind reminder text by default; use adaptive outlines/shadows and accessible optional high-contrast mode if wallpaper demands.
- **Playful, never obstructive:** desktop overlay uses bounded pet/input geometry, not a transparent full-screen click-catcher. Never steal app focus on arrival; minimize interruption in fullscreen/focus mode.
- **True personalization:** Cat/Dog/Bunny each retain individually saved art, controls and behaviors; preview actions never change reminder state.
- **Zero-pressure onboarding:** “Skip for now” for nonessential setup, no forced notifications/voice/auto-start; skip can still create a working reminder.
- **Local privacy:** no login or cloud required; reminder titles do not go to logs. Errors should be actionable without leaking personal data.
- **Accessible by default:** complete keyboard navigation, visible focus, UI Automation names, system scaling, optional voice, reduced motion, sufficient contrast, usable controls.
- **Truthful platform capabilities:** describe session-lock, sleep and Windows Focus behavior realistically, never guarantee wake or always-on delivery.
- **One cohesive product:** every screen uses the same tokens, copy tone, typography and pet silhouette system.

## 4. Information architecture (Windows)

| Main destination | User question | Screens/functions |
|---|---|---|
| **Home** | What needs my attention today? | Greeting, companion preview, Today / Upcoming / Needs Attention, next reminders, quick add, optional friendly progress (without guilt) |
| **Reminders** | What, when and how should I be reminded? | List/agenda, search/filter/sort, fast editor + Advanced drawer, recurrence, priorities, snooze, categories, history and delete confirmation |
| **My Pets** | Which companion is mine? | Owned Cat/Dog/Bunny gallery, saved profiles, compare personality motions, choose, preview |
| **Pet Studio** | How does my companion look and behave? | Appearance, animation, voice, effects, preview stage; separate profiles with reset and reversible save |
| **Settings** | How intrusive/reliable should alerts be? | Speech, quiet hours, Windows startup, monitor behavior, notifications, accessibility, backup/recovery, data/privacy |
| **Help & About** | How do I get help or understand limits? | First-run replay, quick guide, keyboard help, troubleshooting, data/export, version and disclaimer |
| **First-run flow (conditional)** | How do I start safely? | Welcome → how it works → select pet → notification preferences → first reminder → preview/controls → Ready; skippable |
| **Desktop reminder overlay** | What do I need to do now? | Pet entrance, text reveal, voice, Done/Snooze/Mute/Hide, response, exit; never a filled reminder box by default |

**Navigation rule:** 5 main screens persist in sidebar; Help/About under Settings and contextual “?” guidance rather than a cluttered sixth primary menu. Advanced reminder fields are collapsed until the user requests them.

## 5. Product requirement hierarchy

### P0 — indispensable for the next stable release
- Offline reminder creation/edit/delete, one-time and existing daily/weekly recurrence remain correct and backward-compatible.
- New reminder engine architecture must never produce duplicate occurrences or inaccurate statuses after restart, snooze, or DST boundaries.
- Strong UX for “create first reminder,” helpful confirmation and recovery from invalid input.
- Clearly legible reminder text over light/dark/busy desktop content with manual accessibility fallback.
- Predictable, interruptible pet arrival/speech/action/exit; quiet and reduced-motion modes.
- Essential onboarding and first-time pet choice; an existing user upgrading skips onboarding and retains all saved data.
- Installer and user data preservation, correct local privacy/permissions messaging, keyboard/focus and mixed-DPI resilience.

### P1 — differentiating, but may ship incrementally once P0 is safe
- Custom recurrence: selected weekdays; every N days; monthly/yearly; interval-based follow-up with guardrails.
- Priority, categories, search, batch operations, flexible snooze, preset templates.
- Polished consistent pet art rigs, visual palettes, eye/ear/fur details, accessory layering, expressive gestures, species-specific entrance/idle/reaction/exit choreography.
- Better voice choice/rate, reminder delivery preferences, narrator frequency, sound FX with explicit off settings.
- Theme selector (Light/Dark/System) after contrast test matrix.

### P2 — post-v1.0 / explicit later scope
- Checklist/routine/habit schemas, optional local stats, export/import UI and advanced planner, rather than medical-treatment or adherence claims.
- Custom asset-pack imports, pet interactions outside reminders, larger species library, seasonal costumes.
- Natural language reminder parsing, calendar provider integrations, optional privacy-preserving sync.
- Android Kotlin/Compose + Room after Windows quality signoff; no promise of Windows-style overlays under Android restrictions.

**Not a promise:** all P1 and P2 items are not automatically included in a single v1.0 build; the release scope is controlled through the gates below.

## 6. UX tone / copy guidelines

Use simple human language: “Remind me”, “Repeat”, “Snooze”, “Needs attention”, “Choose your companion”, “Speak reminders aloud”, “Quiet hours”, “Test pet”. Avoid “recurrence algorithm”, “task state enum” and redundant descriptions. Keep messages short. For errors show the consequence and a safe next action (for example, “This reminder couldn't be saved. Try again.”). Never shame users about missed tasks.

## 7. Definitions and visibility

- **Scheduled:** saved due time and repeat policy; not a promise of notification if the PC is off.
- **Delivered:** software recorded an attempt; not proof the person saw or heard it.
- **Needs Attention:** unanswered or missed, even if an overlay was hidden.
- **Done:** user explicitly marks complete after successful local save.
- **Snoozed:** valid next due saved; does not duplicate recurrence series.
- **Hidden:** popup closes without completing the reminder.
- **Muted:** current speech stops, visible alert remains unless user separately hides it.

## 8. Quality objectives (acceptance thresholds; NOT measured current results)

- **Data loss:** zero accepted P0 cases during rollback, upgrade, recovery and simulated write failure. Existing user files must remain unchanged by onboarding/preview-only actions.
- **Duplication:** zero duplicate series occurrences in automated restart, two-simultaneous-reminder and snooze/recurrence regression fixtures.
- **Usability:** create a basic reminder in at most three meaningful inputs and under 30 seconds in moderated internal testing (target, to be measured).
- **Animation:** design target 60 FPS on common hardware while visible and capped resource usage when hidden; profile with realistic Windows laptops before claiming sustained performance.
- **Overlay legibility:** actionable text/controls at Windows 100%, 125%, 150% and 200% DPI/text scaling, on light/dark/busy wallpapers; where text-only contrast is insufficient, offer an explicit accessibility treatment without turning default into a text box.
- **Input:** keyboard operation, clear focus, screen-reader annotations, no full-screen invisible hit surface; test visually and with assistive technology.
- **Audio:** never two overlapping active narrators; immediate cancellation on Mute/Done/Snooze/Hide/voice Off/session lock; 30/60/120-second selected interval measured from previous utterance completion.
- **Recoverability:** backup before any schema migration, non-destructive fault states, explicit user consent for backup restore.
- **Privacy:** no reminder titles in diagnostics, no mandatory account or internet; disclose every Windows startup/permission choice.

## 9. Architecture constraints / technical debt before implementation

Current Windows stack: WPF/.NET 10; JSON reminders and pet settings under local AppData; scheduler polling and tray process; Windows local TTS; unsigned per-user Inno Setup. Preserve this reliable surface until replacements are tested.

Proposed design seams:
- `ReminderDomain`: scheduling rules, one-time and recurrence, snooze, history, validation, storage migrations.
- `ReminderPresentationCoordinator`: one active reminder scene, queue and lossless status reconciliation.
- `PetSceneRenderer`: art rigs, animation state machine, text/controls, reduced-motion rendering.
- `NarrationCoordinator`: local TTS ownership, repeats, cancel-on-session changes, silent fallback.
- `PetProfileStore`: per-species versioned settings, safe local migration, validated asset references.
- `FirstRunCoordinator`: idempotent first-run flow only after profile/data-presence inspection; never replace existing reminders.
- `Theme/DesignTokens`: consistent WPF resource dictionary and responsive typography; visuals in Canva/Figma remain reference, not runtime dependencies.

**Decision gate:** current JSON can remain for low-volume data. Do not convert to SQLite simply because “premium” sounds larger. Introduce a transactional data layer only with a tested migration+rollback case and a clear requirement from advanced recurrence/history.

## 10. Release management

This approval locks **direction and work sequence**, not every detailed interaction or visual asset. Exact logo artwork, character rigs, new recurrence semantics, how strongly voice repeats, fullscreen interruption behavior and typography need visual prototype + acceptance before implementation lock.

See `docs/V016_DELIVERY_PLAN.md` for milestone entry/exit criteria; `docs/V016_REMINDER_SPEC.md` for schedule behavior; `docs/V016_CHARACTER_EXPERIENCE.md` and `docs/V016_BRAND_ONBOARDING.md` for art and onboarding.

**Current deliverable v0.16 is a PRODUCT BLUEPRINT, not a Windows EXE.** No claim of new reminder options/brand/onboarding code being live until future builds explicitly pass Windows CI and user QA.
