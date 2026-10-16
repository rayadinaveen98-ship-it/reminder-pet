# PetPing v0.18 — R2-A Reminder Editor Validation

**Branch:** `feature/v0.18-r2a-snooze-editor`. **PR:** #20 (draft; not released).
**v0.17 latest laptop-test preview is still awaiting hands-on approval.**

## Product outcome
Create/edit should be quick and predictable. An invalid reminder must never appear to save or silently become a different date or repeat choice.

## Delivered in the draft branch
- One pure, shared `ReminderEditorRules` domain validator for names (1–120 visible UTF-16 characters after trim), supported repeat choices and local 24-hour date/time.
- `ReminderStore` and the WPF editor use the same name rules; validation occurs before mutation and disk writes.
- Empty/missing name, missing date, invalid `HH:mm`, past instant, daylight-saving skipped hour and ambiguous repeated hour produce clear errors; no guessing of an offset.
- Live preview of the next scheduled local date/time and repeat pattern; view updates when date, clock or repeat changes.
- New reminder default +5m selects **both** date and time from the same future instant, handling midnight/daylight-saving rollover. Previously the UI could select tomorrow's time with today's date.
- Guard against UI save reentrancy: Add and Quick actions are temporarily disabled while saving; already-saved data and legacy JSON stay untouched.
- Existing one-minute quick add, daily and weekly scheduling, edit/cancel, keyboard access and error toast remain intact.
- Local-only, no new fields or remote APIs.

## Deterministic automated regressions
- Title trim, exactly 120 chars, reject whitespace and 121 chars.
- Supported None/Daily/Weekly repeat mapping and reject an unset option.
- Date + time rollover across midnight (23:58 + 5m -> next day 00:03).
- Future clock parse, missing date/time, invalid 24:00 and past due timestamp.
- DST invalid spring hour, ambiguous fall hour, and correct summer UTC offset using a synthetic timezone (no local PC timezone assumption).
- Reject an invalid edit without changing the existing in-memory reminder or any persisted JSON.
- Previous R2-A flexible snooze tests remain in the same CI regression suite.

## Deferred / not claimed
- This branch is an **engineering preview**, not a v0.18 installer release.
- Live UI focus/tab-order, 200%-scaling, screen-reader behavior, modal/date picker validation and actual alert playback still require the user's Windows laptop.
- R2-B multi-weekday recurrence and interval schedules need independent domain rules and migration review before UI or persistence changes. Do NOT insert unsupported recurrence enum values into current v0.17 JSON.
- Preserved v0.17 release gate: never claim sleep/secure-lock exact-time reminders are guaranteed.

## Acceptance sequence
1. Use v0.17 laptop acceptance checklist before promoting this branch.
2. On a dev build from PR #20, create a valid one-time, daily, weekly, and quick +1m reminder. Verify live preview and actual saved date.
3. Invalid input: blank/space name, 24:00, past date/time; verify no reminder is created and existing edits are intact.
4. At 23:58 local, check suggested date becomes tomorrow (or use deterministic core check).
5. Test at 810×640, 125/150/200% DPI, navigation and keyboard focus.
6. Run Windows CI and v0.15→v0.17 installer preservation before any later merge/release decision.
