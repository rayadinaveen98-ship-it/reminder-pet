# PetPing v0.18 R2-B — Custom repeat calculation prototype

**Draft only:** Pure calculations on a separate branch. No WPF controls, reminder persistence, JSON migration, app-version bump, installer release or production behavior change.

## Product intent
- Selected weekdays, such as **Monday / Wednesday / Friday at 7 PM**.
- Every N weeks (1–52) with an explicit week anchor; Monday is calendar week start, based on the original local start date.
- Every N days (1–365) measured from original user-entered local date.
- Future instant displayed and calculated in the user's selected time zone, preserving the intended wall-clock hour across DST.

## Pure domain implementation
- `AdvancedRepeatRules.NextWeekdayOccurrence(...)` and `NextDayIntervalOccurrence(...)` return the first occurrence **strictly after** the supplied clock.
- Deterministic sorted, deduplicated weekdays; no empty set, invalid `DayOfWeek` or unsupported intervals.
- Reuses existing `RecurrenceRules.ResolveWallTime`: DST gap moves forward to the first valid minute for that date, DST overlap chooses the earlier instant. Requested local time remains an input for the following dates.
- Advances in bounded time even after decades offline: at most three matching interval blocks are inspected, no unbounded historical replay.
- No need for permissions, servers, accounts, telemetry or third-party packages.

## Tests
- Mon/Wed/Fri selection, week boundary and every-other-week schedule.
- Start midweek and reject selected weekdays earlier than the recurrence anchor.
- Duplicate input days normalize.
- N-day interval, many years of downtime and bounded return horizon.
- Missing/invalid weekdays, 0/too-large interval, invalid local clock.
- Synthetic DST spring missing hour and next-week restoration, fall repeated-hour choice.

## Integration **NOT** completed
1. Obtain written choices for how users expect schedule anchors, edited recurring series and bulk delete to behave before storing new recurrence metadata.
2. Write additive versioned schema options while guaranteeing legacy `RepeatPattern.Daily/Weekly` and v0.17 reminder JSON remain readable. Never reinterpret old enum values or overwrite incompatible files.
3. Separate recurring series definition from delivered event only if rollback/duplicates require it; atomic Save and one-next-occurrence guarantee remain hard gates.
4. Add preview UI only after persistence, timezone and crash/restart tests pass. The v0.17 Windows physical acceptance remains open, so this research branch should stay unmerged.
5. Keep monthly/yearly policy decisions and quiet-hours work in later gated slices.

Related product spec: `docs/V016_REMINDER_SPEC.md`, section R2-B. This is not evidence of live custom recurrence in PetPing.
