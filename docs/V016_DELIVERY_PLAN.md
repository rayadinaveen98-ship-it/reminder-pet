# PetPing — v0.16–v0.21 Delivery Plan and Release Gates

**Decision:** approved long-range expansion before Windows stable v1.0. **This plan is locked in sequence, not promised dates.**
**Starting point:** merged Windows v0.15 with 122 CI smoke checks and successful synthetic v0.14→v0.15 installer upgrade.
**Team model:** product/UX lead + original character designer + Windows/WPF engineer + reminder/domain engineer + accessibility/reliability QA. Assign responsibilities even when one developer handles several roles.
**Distribution target:** offline-first Windows installer + portable test ZIP. Android native only after Windows release maturity. **Budget:** avoid mandatory paid APIs, licensed assets, subscriptions or hosted services.

## 1. Milestone plan (progressive value, not a big-bang rewrite)

| Version | Goal | Primary deliverables | Exit gate |
|---|---|---|---|
| **v0.16 — Product blueprint** | Lock architecture and quality bar | Product PRD, reminder/domain spec, brand/UX/onboarding brief, character/art/scene bible, draft tokens, prioritized backlog, decision log | Docs merged, specs consistent, no production code changes, user can review |
| **v0.17 — Brand + rich UI** | Consistent premium Windows appearance | Original logo candidates; design system; full Home/Reminders/My Pets/Studio/Settings mockups; native responsive WPF components; empty/error/loading states | User visual review; WPF CI + UI/keyboard smoke; no reminder regressions |
| **v0.18 — Reminder 2.0** | Powerful scheduling without data loss | Quick and advanced editor, selectable snooze, weekly-day/interval schedules, priority/quiet controls, categories/search as time allows; precise recurrence preview | DST/month edge/rollback/crash/upgrade test suite; real reminder save/missed tests |
| **v0.19 — Character experience** | Truly crafted responsive companions | Approved original Cat/Dog/Bunny art and layered rigs; varied idle/arrival/voice/Done/Snooze/exit; stronger readability; deep Studio options; effects & performance budget | Recorded Windows scene QA for each pet, high-DPI and accessibility; no leaking timers/voice |
| **v0.20 — First-run + guidance** | Delightful safe welcome for new users | Branded welcome/splash, choice of pet, explicit voice/motion/startup preferences, first reminder, contextual controls, Help/replay, painless upgrade skip | Fresh-install, migration and interrupted-onboarding test matrix; zero old-data reset |
| **v0.21 — Integrated quality** | Reliable prerelease and operational evidence | End-to-end integration; startup, locked, sleep, fullscreen, mixed monitors/DPI, performance, accessibility, real upgrade and release documents | All P0 test cases passed and P1 regressions triaged; CI artifacts, hashes, installation guide |
| **v1.0 RC → stable** | Public or trusted limited distribution | Versioned release candidate, signed/unsigned distribution decision, privacy/troubleshooting notes, user acceptance and tag | User signs off, no known data-loss/duplicate-notification P0, defensible public claims |

**Scope control:** not every custom repeat, theme and accessory must ship at v0.18/0.19. If a P1 capability threatens P0 integrity, defer it and record the change. Avoid endless unvalidated ornament.

## 2. Critical path and dependencies

```text
v0.16 product requirements + acceptance criteria
  ├─ v0.17 brand + tokens + navigational UX
  │    ├─ v0.19 original pet rig asset pipeline
  │    └─ v0.20 welcome/onboarding native pages
  └─ v0.18 schedule domain + safe migrations
       └─ v0.20 first user-created reminder behavior
v0.17 + v0.18 + v0.19 + v0.20
  → v0.21 integrated desktop/device quality
  → Windows v1.0 RC review
  → Windows stable release
  → Android A1
```

**Never modify the scheduling domain just to repaint the dashboard.** Keep reminder schema changes isolated in v0.18 and test migrations from v0.15; UI and pet upgrades should not silently mutate reminders.

## 3. Work packages with measurable acceptance

### W16-01 Product PRD and IA (P0)
- [x] Define user problems, target audiences, job stories and Home/Reminders/My Pets/Pet Studio/Settings/Help.
- [x] Define current-vs-planned distinction, P0/P1/P2 and success criteria.
- [x] Explicit platform limitations and privacy.
- [ ] User reviews priority split; changes become recorded decisions, not ad hoc deletions.

### W16-02 Reminder engine specification (P0)
- [x] User stories for flexible snooze, selected weekdays, intervals, monthly/yearly, priority/quiet.
- [x] Deterministic recurrence/DST/occurrence semantics and non-lossy migration gates.
- [x] Template and medication-reminder scope kept as basic user-configured reminders.
- [ ] Implement as v0.18 code slices after tests and data fixtures are agreed.

### W16-03 Brand and design system (P0)
- [x] Three logo exploration directions, asset requirements, UI principles, accessibility color/spacing proposal.
- [x] Structured first-run and replay flows.
- [ ] Produce actual original editable logo variants / complete high fidelity UI mockups.
- [ ] Visual selection and palette/character user approval.
- [ ] Build WPF theme and component library in v0.17.

### W16-04 Character/performance bible (P0)
- [x] Separate Cat/Dog/Bunny anatomy, emotion sets, behavior and complete scene.
- [x] Motion/effects budgets, safe overlay and legibility contract.
- [ ] Original final layered pet artwork and animation-ready rigs.
- [ ] Measured scene on Windows laptop before release claims.

### W16-05 Cross-chat continuity (P0)
- [x] Canonical project status, engineering log and roadmap will point to these blueprints.
- [ ] Record PR merge commit and green CI (if CI runs for design-only PR) and next implementation task.
- [ ] Preserve v0.15 fallback and existing reminders unchanged.

## 4. Version-specific quality and release criteria

| Gate | What to inspect | Failure action |
|---|---|---|
| Source gate | All planned code has tests and peer-style review against PRD | Fix source before build/merge |
| Reminder safety | No data loss, invalid repeats, double next occurrence, silent corrupt reset | Block release |
| Audio/overlay | Voice cancel works and alerts readable, no fullscreen hit capture | Block or downgrade feature safely |
| Visual quality | Layout at 100–200% DPI, complete assets and consistent state styles | Revise art/UX before claiming premium |
| Accessibility | Tab order, visible focus, automation labels, reduced motion, text contrast | Treat critical task failure as blocker |
| Performance | No unbounded particles/timers/memory; record 20-cycle device benchmarks | Optimize/remove decorative effect |
| Installation | Fresh install, real prior version upgrade, backup retention and uninstall | Block release |
| Platform truthfulness | Lock/sleep/fullscreen/notification limits accurately explained | Fix copy and fallback behavior |
| Data privacy | No reminder titles in diagnostics; no silent sync or forced account | Block release |
| User acceptance | Explicit pass/fail/deferred on Windows physical scenarios | Record unresolved gates, do not claim stable |

## 5. Product decision log — locked vs awaiting evidence

**LOCKED NOW**
- PetPing local-first Windows-first native WPF path; Android follows Windows maturity.
- Full product upgrade before final v1.0; version sequence v0.16→v0.21 then RC.
- Reliable reminder engine and usability take precedence over character theatrics.
- Rich but legible premium design; friendly Cat/Dog/Bunny with authentic differentiating art.
- Floating unboxed reminder text by default and accessible control chips.
- Real per-pet customization, optional voice/effects, reduced-motion support.
- Logo, welcome/setup/onboarding/help are must-have project deliverables.
- GitHub handoff/checkpoints, tests before merge and distinguish CI from physical QA.

**PROPOSED, NOT LOCKED**
- Which of three visual logo concepts wins.
- Specific final color tokens, paid/free design tooling choice for editable assets.
- Monthly “31st” and leap-day recurrence policy; quiet-hours exact override semantics.
- Whether checklist/routines and advanced voice packs enter stable v1.0.
- Whether code signing will be purchased/obtained for public distribution.

## 6. Senior PM / engineering cadence

For each implementation increment: **spec → design prototype → accessibility review → implementation branch → tests → Windows CI → physical-device checklist → bug fixes → merge → docs → next increment**.

- Maximum one major cross-cutting risky change per PR (e.g., storage schema OR overlay architecture), except minor visual fixes.
- Every branch updates PROJECT_STATUS and ENGINEERING_LOG while active.
- A preview build must be explicitly labeled unsigned/test; only verified paths are shared.
- Don't label future art, animations or reminders "implemented" merely because they exist in mockups or docs.
- User feedback becomes acceptance criteria or backlog entry rather than automatically resetting previously locked requirements.

## 7. Risk register

| Risk | Severity | Mitigation |
|---|---|---|
| New recurrence rules break old reminders | Critical | additive/migrated schema, round-trip fixture suite, rollback, cross-version CI |
| New onboarding overwrites prior preferences | Critical | first-run guard, idempotent marker, migrate fixtures |
| Character effects hide text / block desktop | High | bounded actor, contrast fallback, click-through tests, Motion Off |
| Scope grows uncontrollably | High | P0/P1/P2 gates, cap user-facing controls per screen, defer risky P1 |
| Smooth animations degrade battery | High | frame timings and memory benchmarks, disposal, low-power mode |
| Copyright/trademark/art source uncertainty | High | own original layered artwork, license register, brand review |
| Unsigned installer blocked by Windows | High | signing/distribution decision and truthful warnings before public release |
| Fullscreen/lock/sleep delivery limitations | High | session-aware fallback, Needs Attention, no impossible guarantees |
| Limited design software seats/credits | Medium | code-versioned tokens, inexpensive/free editing workflows |
| Private notes stale vs actual source | Medium | canonical docs + tests + final merge/CI IDs |

## 8. Next atomic action after blueprint merge

**v0.17 Stage A: Brand identity and WPF visual foundation.** Create 3 genuinely original icon/logo concept families with source (not a collage), 2 light/dark palette options, native component inventory and high-fidelity wireframes for Home and Reminders. Require user visual review before mass UI replacement. Keep v0.15 Windows build available as a stable fallback. Then incrementally implement WPF components, leaving recurrence untouched until v0.18.

**This deliverable is a planning checkpoint, not a Windows v0.16 installer.**
