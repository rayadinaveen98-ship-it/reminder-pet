# Windows quality test matrix (manual, first alpha)

## P0: Reminders are trustworthy
| Test | Expected |
|---|---|
| Create 1-minute future reminder, leave dashboard open | Single pet overlay near expected minute, title exact, voice if enabled |
| Create 1-minute reminder, close dashboard window | Tray remains; overlay appears when due |
| Create two reminders due same second | Two overlays presented sequentially, not overlapping |
| Snooze 10m | Same reminder is pending for a new due time and alerts once later |
| Click Done | Status Completed; no further alert for occurrence |
| Quit tray manually | Process stops and makes no background promise |
| Restart app with reminders saved | Previously scheduled pending reminders remain |
| Inject malformed JSON storage | Error appears; data must not be silently erased |
| Create empty title / past due time | Human-readable input validation error |
| Disable speech | Visual alert still works; no speech |
| Lock Windows session during reminder | Secure lock screen not overlaid; show queued alert on unlock if session signals were processed |
| Sleep through due moment and resume within 15m | Pet catches up; marked Delivered |
| Sleep through due moment and resume after 15m | Reminder shown as Missed, not spoken hours late |

## P1: Polish and durability
| Test | Expected |
|---|---|
| 2 monitors & different DPI scaling | Popup is fully on-screen and avoids taskbar |
| Play fullscreen game / presentation | Future policy avoids interruption |
| Disable/restrict Windows voice output | Popup still appears |
| Windows focus assist enabled | In-app overlay policy documented; future fallback handles system notification rules |
| Restart in different local timezone | One-time absolute instant remains the same |
| Start two app processes | No duplicate scheduler; second instance behavior clear |
| 500+ reminders | Reasonable UI speed, low idle CPU use |
| Turn on high contrast / text scaling | Controls remain usable, legible |

## Recording results
For each test capture machine Windows edition/build, UTC offset, voice installation, display scale, expected fire timestamp, observed popup timestamp, whether app and session were active, errors and screenshot if useful.

## Release gate
No "perfect", "exactly guaranteed", "runs after force-quit" or "locked-screen overlay" marketing claims. Publish only once P0 passes on actual Windows devices.

## Additional v0.2 checks
- Set **Start with Windows** and verify the registry entry starts PetPing silently after signing in.
- Move the executable after enabling startup; verify the previous startup path is flagged for re-registration.
- Toggle speech off, restart PetPing and verify it stays off.
- Corrupt `settings.json`: reminders should still start with default voice settings; settings data must not be silently erased.
- Force-stop with an unanswered alert and relaunch within 15 minutes; the pet alert should reappear.
- Force-stop and relaunch after 15 minutes: app should show the alert in history, not replay it over apps.

## v0.3 device-specific acceptance tests
- **Popup timeout:** let the pet fade/close after 45 seconds; dashboard must still show Needs Attention; Snooze should schedule another attempt.
- **Session lock:** lock the session while the pet is onscreen; it must not speak or draw on the secure lock screen, and should reappear after unlocking.
- **Manual action consistency:** with a pet visible, mark that reminder Done (or Snooze) from the dashboard; the old pet should close.
- **Missed batch:** create several reminders, leave device unavailable until 16+ minutes overdue, then relaunch; one tray summary may appear, all should remain in history as Missed.
- **No false promise:** Windows may suppress tray balloons; do not rely on them as the only notification.

## v0.4 floating pet visual QA
- Confirm orange cat is free-standing beside the bubble rather than inside a large notification rectangle.
- Confirm arrival slides from the right and idle floating/tail animations do not interfere with clicks.
- Test a maximized video browser and regular window; large-window overlay should be upper right; ordinary desktop overlay should be elevated bottom right.
- Hover over reminder and wait more than 45 seconds: it should not vanish until mouse leaves and timer expires.
- Test Done and Snooze; verify both persist and close the popup. Test ×: hide but leave in Needs Attention.
- Test 100/125/150% display scaling, dark desktop, two monitors, 720p and 4K screen.
- Confirm voice continues announcing reminder names with Speak Reminders enabled and remains quiet when disabled.
- Test reminder while Windows session is locked: no secure lock-screen overlay.

## v0.5 reliability QA on a real Windows computer
- Simulate storage unavailable while creating/snoozing/completing: reminder should remain in previous state; one generic error notice; no silent status change.
- Open `%LOCALAPPDATA%\PetPing\diagnostics.log`: entries should include UTC time, fixed event codes and exception types, **never reminder names**.
- Test Windows Speech test button with voice enabled, then disabled and on a machine without installed voice. Voice failure must not prevent visual reminders.
- Lock while voice speaks: it should stop on lock; no secure lock screen pet or speech.
- Sleep past a due time, wake <=15m late: catch up once; wake >15m late: Needs Attention/missed and generic tray info subject to OS settings.
- Disable toast/balloon notifications: the dashboard Needs Attention must remain the source of truth.
- Force pet presentation failure in developer QA: generic tray notification attempt without losing Delivered state. Do not claim this fallback always shows under Focus Assist.
- Verify no high CPU or repeating log spam after filesystem failure; scheduler retry backoff 30 seconds.

## v0.6 repeat/edit QA
- New daily repeat: choose tomorrow at 19:00. Confirm list says Every day, then after delivery see a separate future occurrence and previous Needs Attention until completed.
- Weekly: choose Sunday, confirm next event remains on Sunday at the same local wall-clock time.
- Snooze one delivered daily occurrence: tomorrow's recurrence should remain exactly once and the snoozed event should announce once later.
- Create then Edit upcoming to a new title/date/time; confirm persisted after app close/restart.
- Edit an upcoming repeat to One time; confirm future series no longer advances after that standalone reminder is delivered.
- Old JSON from v0.5 should load; never wipe `%LOCALAPPDATA%\PetPing` during upgrade.
- Sleep for more than one repeat period; on wake no flood of hundreds of stale alerts; one future pending occurrence remains.
- Lock/unlock and voiced reminders require physical Windows QA as before.

## v0.7 physical appearance QA
- Launch pet in Playful mode: entrance slide, bob, tail motion, blink and speaking mouth when TTS starts; mouth stops when speech stops.
- Gentle mode: soft fade and blink, no continuous body/tail movement.
- Off mode: no entrance slide, bobbing, tail wag, blink or exit fade. Reminder text/actions still visible; speech may display a static talking mouth.
- Switch Top-left, Bottom-left, Top-right, Bottom-right manually; overlay must stay inside target monitor working area.
- Auto mode: maximized Netflix/browser near top right; regular desktop at lower right. Check 100/125/150% scaling and two mixed-DPI monitors.
- Disable voice: no speech and no speech-mouth state, but popup still displays and Done/Snooze work.
- Change pet preference, quit tray, restart EXE: saved choice persists, existing reminders unchanged.
- Test corrupt `pet-appearance.json`: app starts with defaults without erasing reminders or file.
- Repeat full Windows sleep/wake and lock/unlock regression tests, not implied passed by GitHub CI.

- Click Done and Snooze with successful disk writes: pet should briefly say a friendly confirmation before disappearing.
- Simulate disk write failure while clicking Done/Snooze: pet must remain visible rather than falsely claiming saved completion; a generic error notification may be suppressed by OS.
