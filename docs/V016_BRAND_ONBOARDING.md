# PetPing — Brand, UI and First-Run Experience Blueprint

**Status:** v0.16 direction locked; visual exploration and onboarding implementation scheduled for later phases. **No final logo has been generated/approved.**
**Priority:** original visual identity → high-fidelity native UI spec → first-run guidance with truthful OS behavior.

## 1. Brand positioning and personality

**Working brand:** PetPing (preserve existing app and repository names while legal/name checks are pending).
**Functional proposition:** timely reminders with approachable controls.
**Emotional signature:** a thoughtful, gently playful companion that encourages, not scolds.
**Voice and writing:** friendly, concise, never hyperactive, technical only in troubleshooting, never guilt-inducing.
**Working tagline option:** "Your reminders, with a little personality."
**Naming gate:** keep PetPing until user reviews availability/brand collision. No rebranding or URL changes as part of a design-only milestone.

### Three logo exploration directions (NOT chosen finals)
1. **Friendly Signal:** small rounded bell/signal mark with an abstract ear/animal silhouette; excellent at 16–48px.
2. **Companion Monogram:** a custom P formed through a paw or alert curve, simple enough for tray and taskbar.
3. **Pet + Ping:** an original cat/dog/bunny-neutral icon paired with a gentle ping dot—avoid implying only Cat is available.

Choose one identity family only after monochrome legibility, taskbar readability and user review. No paid licensed font/logo templates required.

### Required final identity assets (brand phase)
- Primary horizontal logo, compact stacked logo and symbol-only 16/24/32/48/128/256px variants.
- Windows .ico (multiple embedded sizes), tray-friendly monochrome icons, startup/installer mark, light/dark versions and safe spacing.
- Source artwork stored in editable vector format with documented origin/license, not only raster screenshots.
- Original silent welcome animation option and separate soft sound cue; sound timing synced with logo by event/state, not a guessed timer.
- Strong brand color/shape/typography guideline; logo always recognizable on light and dark backgrounds.
- Native installer Start Menu/taskbar icon and About UI must use the same approved asset.

## 2. Aesthetic direction

**Style:** sophisticated cozy productivity, boutique 2D game artistry, measured personality. No huge gradients, neon gaming HUDs, random emojis in production UI, tiny text or excessive cards.

Use the previous approved ivory/lavender/apricot palette as the *starting point*, then refine with contrast tests. Existing tokens are historical, not a mandate against improvement.

UI fundamentals:
- Inter/system-native Segoe UI style font with predictable WPF typography and legal redistribution; no font download required.
- 8px spatial rhythm, breathable whitespace, visible hierarchy and clear “primary action.”
- Rounded but not inflated corners; balanced dense task list and spacious pet-led Home.
- Both light and dark themes contemplated, but dark mode only ships after same accessibility QA.
- Readability prioritized over mascot space: Home can be playful, Reminders must remain efficient.
- Contrast at WCAG AA for core text/controls as design target, verified on actual WPF render.
- Fluid desktop navigation: sidebar on wide screens, slim mode on narrower windows, noncropped dialog/controls at 200% scaling.
- Thoughtful states: empty, loading, saved, invalid, conflict, backup recovery, offline (expected), missing speech voice, disabled start-at-login.
- Hover/focus/press feedback useful but subtle; keyboard equivalents and readable tooltips.
- No default filled box around floating reminder text. Action controls remain separate clear chips.

## 3. Visual screen contract

| Screen | Visual priority | Content hierarchy | Experience nuance |
|---|---|---|---|
| Welcome | Brand + friendly cat/dog/bunny-neutral symbol | logo → tagline → Get Started / Skip tour | beautiful and fast, no forced audio |
| Setup | Trust and control | voice → startup → motion and placement → Continue | system choices off until opted in |
| Home | Ready state | Next due + quick add, active pet, attention count, small day summary | subtle pet idle; never distract |
| Reminders | Efficient task work | quick create → Today/Upcoming/Attention → filters/search → list | expandable Advanced, clear status |
| My Pets | Character selection | curated premium illustrated 3-card gallery | selection is real, saved |
| Pet Studio | Larger animated art | stage → Appearance / Behavior / Voice / Effects / Preview | saved per species with reversible changes |
| Settings | Confidence | Alerts, Accessibility, Local data, Windows startup | no jargon, no ambiguous toggle |
| Help | Self-service | “How reminders work,” keyboard, troubleshooting, replay guide | practical, not text-heavy |
| Popup | Focused | character, unboxed task text, Done/Snooze/Mute/Hide | readable over diverse wallpapers |

## 4. First-run experience: six meaningful steps

**Entry rule:** onboarding appears only if no completed first-run marker exists AND no existing reminder/profile data indicating an upgrade, unless user intentionally chooses Replay setup. Marker stored independently and written **after** user finishes or explicitly skips, without rewriting reminder files.

| Step | User sees | Interaction | Default / skip policy |
|---|---|---|---|
| **1. Welcome** | Logo, one-line product promise, animated but optional companion peek | “Get started” / “Skip setup” | Never block existing user |
| **2. How it works** | Three illustrated beats: choose time → pet visits → respond | “Next”; Back; Skip | 15–30s read total; no carousel overload |
| **3. Choose pet** | Milo/Buddy/Luna art and distinct short personality previews | Choose / Preview | Milo default; skip allowed |
| **4. Set preferences** | Speak reminders toggle, how often repeat, Full/Gentle/Off, optional Start With Windows | Explicit toggles and Test voice with user click | No forced voice or startup; explain exact consequence |
| **5. First reminder** | Minimal input example or blank editor, preset “In 1 minute” available | Create real user reminder / Skip | Never seed fake personal reminders silently |
| **6. Ready** | Pet celebratory but controlled; “You’re all set” and short Done/Snooze tutorial | Open dashboard / Preview example | Mark onboarding complete only once and keep replayable |

**No “cute/funny guide chooser” step** that can block progress. Choosing a pet IS the single character decision; onboarding should feel inviting, not laborious.

### Guidance after onboarding
- Inline contextual micro-tip at first arrival: Done completes, Snooze reschedules, Hide keeps Needs Attention, Mute stops voice.
- Dismissible tips, once by default; always accessible via Settings → Help.
- Quick keyboard shortcuts and taskbar/tray explanation.
- “PetPing keeps reminders running when you close the dashboard, but not if you Quit, sign out or shut down.” Truthful local-service copy.
- Explain Windows sleep/lock behavior, system notifications and why an alert may be in Needs Attention.
- Separate help page about restoring a **validated backup**; never automate destructive repair without explicit consent.

## 5. State flow and safety

Welcome → optional tour → pet selection → preferences → user-controlled first reminder → ready; Back preserves in-progress selections in memory.

**Upgrade guard:** a v0.15 user upgrading to onboarding version must land on their previous Home, retain reminders, profile, app-start settings and voice choices. Onboarding must never reset stored profile or overwrite normal reminders. A safe “What's new” optional brief may appear once, but not a forced first-run setup.

**Skip rule:** if user skips setup, write a minimal completion marker and leave all preexisting preferences untouched.

**Permission/auto-start rule:** Windows startup is opt-in and reversible, never silently enabled by install/onboarding. Speech test is silent until user presses Test. No permission prompts unrelated to actual OS needs.

**Animation rule:** reduced motion and Windows accessibility settings override splash/brand intro; on-screen text remains legible when no animation runs.

**Error rule:** first-run crashes midway may restart from a safe step without creating duplicate reminders or resetting previously saved data. Saving first reminder is transactional.

## 6. Onboarding wording drafts for design team

Welcome heading: **“Meet your new reminder companion.”**
Subtitle: **“A little help remembering what matters.”**
How it works: **“Set a reminder. Your pet drops by when it's time. You decide what happens next.”**
Voice opt-in: **“Let your pet speak reminders aloud.”**
Startup: **“Start PetPing when you sign in to Windows.”**
Reminder intro: **“What's one thing you'd like help remembering?”**
Ready: **“You're all set. Your companion is ready when you need it.”**

These are product copy proposals; verify long labels, keyboard mnemonics and localized wording before art signoff.

## 7. Milestone deliverables and approvals

**v0.16:** brand territory, logo briefs, native UI screen map, onboarding logic and text, token draft; user approves direction, not final art.
**v0.17:** polished editable mockups of ALL key screens, reusable WPF visual theme/control library and initial brand asset candidates; visual review gate.
**v0.19:** character art source, motion rig, pet scene/render improvements.
**v0.20:** working welcome, setup, onboarding and guidance implemented with no data regression.
**v0.21:** QA, responsive/mixed-DPI, accessibility, performance, installer, optional code signing strategy and public release readiness.

### Design review checklist
- Is the UI richly crafted **without** feeling crowded or slow?
- Does each screen have one primary next action and nontechnical labels?
- Is the logo legible in a tiny Windows tray icon and monochrome?
- Does dark/light text remain readable over panels and floating desktop?
- Can an existing user safely skip onboarding after an upgrade?
- Do character customization previews reflect the actual saved pet?
- Does the experience still work fully with voice/motion OFF?

**No logo, splash executable or onboarding wizard is claimed built by this specification.**
