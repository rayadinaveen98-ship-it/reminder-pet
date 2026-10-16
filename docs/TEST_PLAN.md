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

## v0.8 QA checklist
- Filter All/Today/Upcoming/Needs Attention/History; verify no existing reminder disappears from data when switching views.
- Select a reminder then refresh/filter; selection is retained if the item remains visible, otherwise cleared.
- Check empty states with no reminders or no matching filter entries.
- Ctrl+N focuses the new reminder field; F5 refreshes; Esc cancels an active edit.
- Test Delete confirmation: No preserves reminder, Yes deletes it; for repeating reminders, removing the next future event stops its series. Older history is unchanged.
- Test at 125–200% Windows scaling and keyboard-only focus/Tab order; check action row wrapping on small window widths.
- Installer: run portable build quit from tray; install unsigned setup with per-user privilege, optionally create desktop shortcut; confirm Start Menu launch, installed app operation and uninstall entry.
- Uninstall/reinstall: verify `%LOCALAPPDATA%\PetPing\reminders.json`, preferences and logs remain intact. Optional Start with Windows should point to installed EXE when previously enabled.
- This installer is unsigned. Real Windows SmartScreen/security warning behavior varies and requires user verification; do not promise warnings never appear.

## v0.8 automated evidence (separate from manual QA)
- CI run https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38877847189 passed 64 core checks including Today/Upcoming/Attention/History filter membership, order and immutability.
- Windows WPF executable compiled and portable ZIP uploaded.
- Inno Setup 6 installer compiled. Windows hosted runner installed it silently, checked `PetPing.Windows.exe`, uninstalled it silently, and checked that a sentinel in `%LOCALAPPDATA%\PetPing` survived.
- **Not tested by CI:** interactive installer wizard, Start Menu use, real user SmartScreen decisions, Windows screen readers, actual lock/sleep, mixed DPI or correctness of existing data after a real user's upgrade.

## v0.9 manual recovery and installer beta-quality checks
- **Back up the entire `%LOCALAPPDATA%\PetPing` folder first.** Use a test folder/mock reminders for destructive corruption tests; never intentionally corrupt live reminders.
- Simulate malformed primary JSON with a known-good older `.bak` file on a disposable test account. Start PetPing: recovery prompt should clearly say the backup is older and might omit latest edits.
- Choose No: startup aborts and both primary and backup remain unchanged. Choose Yes: it starts from valid backup, retains the unreadable original as `reminders.unreadable-*.json`, and leaves `.bak` intact.
- Test absent or corrupt `.bak`: no restore action offered, primary remains intact and user gets a startup error. Do not silently start with zero reminders.
- Click Open data and diagnostics folder: Windows Explorer opens user LocalAppData PetPing. Confirm log files contain no reminder text; no uploads are triggered.
- Perform an **actual v0.8 installer → v0.9 installer upgrade** from a backed-up live account. Verify Start with Windows when selected, recurrence history, voice, pet settings and uninstall does not remove data.
- Test screen-reader focus, DPI 125–200%, second monitor, lock/sleep/wake and multiple overdue reminders. These remain unverified by CI alone.

## v0.9 CI-verified, device tests pending
- Green run: https://github.com/rayadinaveen98-ship-it/reminder-pet/actions/runs/38881511852 (72 core checks, installer install/reinstall/uninstall smoke passed).
- The CI reinstall test used the SAME v0.9 installer twice, not a v0.8→v0.9 transition. Do not claim real upgrade compatibility until device evidence exists.
- The recovery rules were covered in isolated temp directories; actual Windows recovery dialogs should be tested with a disposable account/reminders, not production reminder files.

## v0.11 walking companion / narration QA
- Test a 1-minute reminder: pet walks in from left/right edge (Auto random), stops near mid-screen, THEN reads title. Text should have **no colored speech bubble**.
- Check all four controls. Done persists completion then reacts, Snooze schedules 10 minutes later and reacts, Mute silences repeat while pet remains, Hide stops voice and leaves Needs Attention.
- Leave a reminder unanswered after speech finishes: next announcement should begin around 60 seconds **after previous utterance ends**, not overlap it. Sound toggle Off prevents subsequent speech.
- Use Motion Off: pet appears directly at center with accessible controls; Gentle glides more quickly without steps.
- During speech, lock Windows or suspend laptop: narration must stop and PetPing must not display on secure lock screen; unanswered reminder reappears safely after unlock/wake.
- Test at Windows 100%, 125%, 150%, 200%, and on multiple monitors if available: pet should never disappear off monitor or block whole desktop clicks.
- Simultaneous reminder due times should queue and not overlap voices. Delete a queued reminder from dashboard and verify it does not show after its predecessor.
- No app startup, pet texture or animation tests can be inferred solely from .NET/XAML compilation; physical QA required.

### v0.11 Windows automated verification (manual QA still open)
- GitHub run 38890848806 succeeded: 86 smoke checks including geometry/voice repeat rules, compiled Windows WPF and unsigned Inno installer, verified install/reinstall/uninstall preserved test data.
- No claim that real pet crossing animation, button visibility/hit testing, speech repeat exact timing or Windows sleep behavior is verified by CI. User testing required.

## v0.13 three-pet physical QA
- Open My Pets. Choose each Cat/Dog/Bunny and preview the next on-desktop walking reminder. Confirm visible anatomical differences and corresponding name in UI; not merely changed label or thumbnail.
- Open Pet Studio: change palettes Apricot/Cocoa/Snow/Lavender, None/Bow/Scarf accessory, Small/Medium/Large, personality and Done/Snooze action. See preview update and test pet behavior on screen.
- Change narrator repeat to 30, 60 and 120 seconds; ensure actual reannouncement follows the chosen delay after speech finishes. Confirm Mute and Hide stop repeating immediately.
- Quit tray app and reopen: same pet, palette, accessories, size and actions remain. Existing saved reminders must not change.
- Corrupt only a **disposable test** pet-profile.json and restart: app chooses safe cat default without deleting the corrupt file or reminders.
- Physical tests for 100–200% scaling, pet controls and keyboard focus, second monitor, session lock, suspend/resume. CI is not physical confirmation.
