# PetPing — Product Blueprint v0.1 (historical)

> Superseded as the active product requirements source by [v0.16 Product Blueprint](V016_PRODUCT_BLUEPRINT.md) and [v0.16 Delivery Plan](V016_DELIVERY_PLAN.md). This file is retained for early history and does not describe the current v0.15 code or new planned advanced features.
**Stage:** product definition + first Windows source foundation (not a production release)  
**Order:** Windows desktop → Android → evaluate macOS/iOS  
**Public name:** TBD; PetPing is a working codename.

## Product promise
A small companion that appears when a user-chosen reminder is due, says what matters (when voice is enabled), and leaves after the user acts. Reliability outranks animation.

## Product principles
1. The reminder must remain useful even if the mascot fails to render or the voice is unavailable.
2. Creating a reminder takes only **what** and **when**. No account, internet, or AI service needed.
3. The companion does not live on top of other apps unless the user explicitly asks for it. Default = appear when due, then disappear.
4. Never pretend we can draw over Windows' secure lock screen or Android's secure lock screen without OS-approved behavior.
5. Transparency about power, notification, and permissions is part of UX.
6. Respect attention: do not steal keyboard focus, avoid full-screen interruption, and always provide a visible snooze action.
7. Privacy by default: reminder text lives locally; no analytics/telemetry in v0.
8. Built-in accessibility: keyboard-operable controls, readable text, reduced motion in later design pass, and audible alerts optional.

## Job stories
- When I'm concentrating on work, I want a small character to tell me exactly what I scheduled, so I can act without opening a productivity application.
- When I close the dashboard, I want reminders to keep working, so I don't have to remember to keep the window open.
- When I lock my laptop, I want to know later what I missed, rather than falsely marking the reminder completed.
- When I need quiet, I want to turn off speech while keeping visual reminders.

## Initial scope (Windows vertical slice)
- Native Windows dashboard with name, date and 24h time fields.
- Quick add 'in 1 minute' to test scheduling.
- Local reminder persistence in user AppData as JSON.
- System tray background operation after closing dashboard.
- Transparent borderless, always-on-top cartoon pet + reminder title.
- On-device speech via installed Windows speech voices.
- Done and Snooze 10 minutes; simple reminder history/status.
- Catch-up display for reminders <=15m overdue when application starts/resumes.
- Older reminders become Missed (shown in history), not silently delivered hours later.
- Session lock awareness: queue pet until unlocked when the OS reports lock.

## Out of scope for v0
- Android/iOS release; Android gets its own native reminder delivery stack.
- 24/7 background pet, elaborate pet raising, XP/gamification, camera/microphone.
- Calendar integrations, recurring reminders, natural-language parsing, cloud sync.
- Automatic wake-from-sleep via Task Scheduler or guaranteed alarms after process termination (start-at-login preference exists in v0.2).
- Toast fallback, multi-monitor placement preference, quiet hours and focus/full-screen detection.
- Official 2D pet animation assets, installer/signing/store publication.

## Canonical experience
1. Launch app. Empty state emphasizes a single new reminder input.
2. Enter "Drink water" and set future date/time. Tap Add reminder.
3. Close dashboard. The tray icon indicates reminder process is active.
4. When due, a small overlay opens near the taskbar but away from the screen center, speaks once when allowed, and displays title.
5. Tap Done to complete, or Snooze 10m to schedule another alert.
6. If the machine is asleep and resumes within 15 minutes, show late alert; if later, retain Missed status for review.

## Voice and personality guidelines
- Voice: casual but concise, no repetitive chatter: "Hey! It's reminder time. [title]."
- Mascot v0: small original vector cat as an *engineering placeholder*, not approved final artwork.
- Default voice ON for explicit reminder delivery; easy toggle in dashboard.
- No emotional guilt or punishments for missed reminders. A pet should encourage, not nag.
- User-provided reminder text must not be interpreted as commands, scripts or markup.

## UX metrics to instrument locally in later beta (opt-in if exported)
- Successful due-time delivery rate while Windows app is running and session unlocked.
- Difference between scheduled time and first visible popup.
- Percent delivered, missed, snoozed, completed.
- Minutes to create first reminder.
- Crash-free reminder sessions; idle memory; CPU wake frequency.
- First-week repeat usage and user self-report on interruption quality.

## Success gate before Android
- All P0 behavior passes manual Windows 10/11 test matrix.
- Repeated reminder delivery near expected time while app is running; no duplicates in basic test.
- Simulated crash/restart has persistent reminders and no silent data reset.
- Locked/sleeping behavior correctly described; app cannot misrepresent guarantees.
- Tray exit and popup behavior are understandable to a non-technical user.
