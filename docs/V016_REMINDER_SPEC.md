# PetPing — Reminder System 2.0 Product and Engineering Specification

**Milestone:** v0.16 specification; implementation planned primarily for v0.18.
**Current working behavior:** one-time, Daily and Weekly (same weekday) reminders, Done, 10-minute Snooze, Needs Attention, 15-minute catch-up, local TTS, persisted reminders and history. **Do not report the features below as shipped.**

## 1. Principle: useful first, powerful on demand

Create the common reminder with **Title + When + Save**, with optional repeat. Advanced options live behind **More options** rather than burdening first-time users.

### Editor layout
- **Quick:** title, local date/time, Save; “in 10/30/60 minutes” shortcuts may be provided without overriding the chosen date without confirmation.
- **Schedule:** One time / Every day / Every week / Custom. Custom opens a clear recurrence builder with a live example (“Every Mon, Wed and Fri at 7:00 PM”).
- **Presentation:** priority, speech allowed, reminder narration behavior, snooze choices, how long the pet stays. Start with an inherited global default.
- **Organize:** optional category, note (plain text only), pinned state.
- **Review:** accessible explanation of next occurrence and missed policy before Save.

## 2. Prioritized requirement matrix

| ID | Requirement | Priority | MVP behavior / acceptance |
|---|---|---|---|
| R-01 | Single reminder CRUD + pending edit | P0 | No duplicate IDs; edit is atomic; failure leaves original visible |
| R-02 | Existing Daily / Weekly | P0 | No regression, timezone-aware; original time preserved around DST |
| R-03 | Flexible Snooze | P0/P1 | Keep existing 10m; add 5/15/30/60m + custom with valid bounds, clear next due |
| R-04 | Priority | P1 | Normal default, Low/Urgent optional; urgency changes presentation, never scheduling truth |
| R-05 | Weekly multi-day | P1 | Choose weekdays with one or more checked; next future local weekday is deterministic |
| R-06 | Every N days/weeks | P1 | Valid N bounds, explicit anchor date and timezone; no duplicate recurrences |
| R-07 | Monthly / Yearly | P1 | Explicit end-of-month / leap day policies in creation preview and persistence |
| R-08 | Quiet hours | P0 for expanded spoken behavior | Suppress speech and optional intrusive overlay according to user setting while preserving reminder status; show on next permitted state |
| R-09 | Repeat until response | P0 for continuity | Re-announcement after completed utterance; intervals adjustable; Mute/Hide/Done/Snooze cancel and do not overlap |
| R-10 | Tags/categories and search | P1 | Optional, fast local filter; don't require a category for every task |
| R-11 | Pinned + bulk actions | P1 | Confirmation for destructive bulk edits; never affect unrelated recurrence series |
| R-12 | Priority-aware appearance | P1 | High priority can change label/accent/optional sound, not forced volume or unbounded speech |
| R-13 | Templates (work, study, hydration, birthdays, habits) | P1 | Convenience presets only; all are ordinary editable reminders, not separate untested engines |
| R-14 | Checklist / routine reminders | P2 | Separate schema and measured demand; no fake checklist completion via one Done click |
| R-15 | Advanced calendar integration or AI text parsing | P2 | Separate privacy/consent/connector milestone, not v1.0 |
| R-16 | Full export/import and preview | P1/P2 | Validated user-operated backup first; no silent replacement or data duplication |

## 3. Scheduling domain: exact semantics

### Domain entity design (candidate, not migration yet)
\`\`\`text
ReminderDefinition:
  DefinitionId (GUID)
  Title (plain text, 1–120 visible characters)
  CreatedAt / UpdatedAt
  TimeZoneId (Windows time zone for Windows runtime)
  FirstLocalDate / LocalTime
  Schedule = OneTime | EveryDay | EveryWeek | WeeklyDays | EveryNDays | Monthly | Yearly
  ScheduleOptions = anchor date, selected weekday mask, interval, date-adjustment policy
  Priority = Low | Normal | Urgent
  DeliveryPreferences = inherited or per-reminder overrides
  CategoryId? / Note? / IsPinned?
ReminderOccurrence:
  OccurrenceId, DefinitionId, ScheduledAtInstant, OriginalLocalWallTime
  DueAtInstant, Status, DeliveredAt?, FinishedAt?, SnoozeCount, ContinuationCreated
\`\`\`
Do **not** swap the existing Reminder/SeriesId format until round-trip fixtures, a migration strategy and a rollback path are proven. Schema version and source version must be explicit. Separate recurrence *definition* from occurrences only if it demonstrably improves safety; don't rewrite all history for a cosmetic requirement.

### Recurrence rules
- Store absolute instance time **and** user-intended local wall clock/time zone; never assume all days are 24 hours.
- Weekly-days recurrence computes the first future selected local weekday after the occurrence; no floating weekday set or locale-dependent parse.
- Month-end policy shown to user: e.g., repeat on last valid day vs skip invalid date. Default proposed: clamp to month’s last day, but **not locked** until preview and user review.
- Yearly Feb 29 policy shown to user: next Feb 28 or only leap years; proposed default “only on Feb 29 in leap years,” but **not locked**.
- Spring DST gap: adjust forward to first valid local minute, retain requested wall time for later occurrences; fall ambiguity: deterministic earlier instant, explained in help.
- Long downtime: advance to the next future instance in a bounded number of calculations. Do not queue thousands of missed repeats.
- Each recurrence continuation is **transactional and idempotent**; crash/reload/snooze cannot generate a second next occurrence.
- Uninstalled/app-quit/PC off: do not promise exact-time delivery; preserve Needs Attention/missed semantics when app next runs.

### One-time vs series actions
- **Done occurrence:** complete this delivery, leave future series progression alone.
- **Snooze occurrence:** moves only the unanswered occurrence; does not shift the weekly schedule’s long-term anchor.
- **Edit this occurrence:** explicitly show how it differs from “Edit all future occurrences.”
- **Delete:** confirmation asks whether only this occurrence or entire future series where applicable.
- **Hide and Mute** change presentation, not stored completion state.
- **Missing storage permission:** don't display Done success; show error and allow retry; preserve prior on-disk and in-memory version.

## 4. Priority, urgency and human-friendly defaults

Priority isn't a guarantee of bypassing Windows policy:
- **Low:** visual-only or gentle pet, optional sound.
- **Normal:** user's selected pet and default voice/motion behavior.
- **Urgent:** stronger accessible visual label and permitted sound; never force full-screen overlay, volume change, focus theft, or continuous nonstop TTS.
- Default remains **Normal**. User can override global urgency appearance and quiet rules.
- “Medicine reminder” must be a template for self-entered dates/times only; avoid dosage recommendations, adherence claims, medical-device positioning, and discouraging professional care.

### Snooze rules
- Offer one click 5/10/15/30/60 minutes and a bounded custom duration (proposed 1 minute–24 hours).
- In callback and UI, show exact next local due time and source of schedule.
- Optional “keep reminding until I respond” is preference-based. Repeat spoken message only if speech enabled, session active, and Windows user is present; default pause >=30s; never overlap, even across multiple due reminders.
- Support an optional gentle follow-up to missed reminders, with a **maximum** count and cooldown. No infinite escalation.

### Quiet hours / fullscreen
- Quiet hours specified as local weekly schedule; timezone changes and cross-midnight intervals must be tested.
- Separate switches for voice, sound effects and visual pet; quiet hours must not silently mark reminders Completed.
- When Windows is locked, secure-screen popup is impossible. On unlock, reconcile saved overdue state and show Needs Attention instead of pretending exact delivery occurred.
- Prefer minimizing intrusive effects in fullscreen and during screen sharing if detectable; provide user control and an accessible non-intrusive fallback.

## 5. Search / organizer / templates

Home remains an at-a-glance view. Reminder list has visible Today, Upcoming, Needs Attention, History and All filters; optional category, priority, repeat and search controls grouped in a collapsible toolbar. Search is **local, case-insensitive**, performs no network calls and never logs query text.

Template examples are shortcuts only:
- “Start studying” → title + next hour proposal, editable.
- “Take a break” → one-time timer, editable.
- “Drink water” → user-chosen intervals; no health claims.
- “Remember birthday” → yearly schedule with explicit leap-day rule.
- “Take my medication” → user-entered title/time; no dose or care advice.

## 6. Storage, validation and migration

1. Export/backup existing reminder database before schema update; write additive version markers.
2. On import/migration validate ID uniqueness, due offsets, recurrence options, enum values, note lengths and plausible recurrence bounds.
3. Stage proposed upgrade to a temporary file, validate **read-back**, then replace atomically.
4. On failed write or invalid JSON, roll back and preserve original unreadable file; don't start with a silently empty list.
5. Maintain backwards compatibility reading v0.15 reminders or explicit opt-in migration route that can reverse safely.
6. Include restart / double-tick / sleep / timezone shift / v0.15 upgrade / corrupted store / store permission failures in smoke fixtures.

## 7. Experience acceptance criteria

- A novice can create a one-time reminder without opening Advanced.
- A weekly-days schedule preview says exact days and next date; calculations match persisted state after app restart.
- No duplication from snooze, recurrence progression, twice firing timer or UI retry.
- Quiet hours, Hide, Mute and Motion Off retain truthful Needs Attention state.
- Urgent never breaks Windows privacy/focus/secure-screen boundaries.
- All reminder edit/delete transitions have keyboard support and descriptive undo/confirmation where appropriate.
- List views remain usable at 200% Windows scaling; empty states explain what to do.
- Data migration preserves preexisting reminder records and per-pet profiles; no P0 data loss.

## 8. Delivery slices for v0.18 (proposed order)

**R2-A (mandatory):** stronger create/editor validation and customizable snooze + status regression tests.
**R2-B:** weekday combinations and interval repeat + timezone/DST suite.
**R2-C:** monthly/yearly and explicit edge-case policy previews (only after A/B stable).
**R2-D:** priorities, optional category/tag, search/filter; quiet hours + presentation integration.
**R2-E (conditional):** template gallery/bulk actions and migration check.

Gate each slice with green Windows CI and **no uncontrolled schema migration**. Anything slipping the release-quality budget remains post-v1.0 rather than undermining the core.
